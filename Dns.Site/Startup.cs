using System;
using Dns.Contracts.Messages;
using Dns.Contracts.Services;
using Dns.DAL;
using Dns.Site.EventHandlers;
using Dns.Site.Hubs;
using Dns.Site.Services;
using Grfc.Library.Auth.Helpers;
using Grfc.Library.Common.Enums;
using Grfc.Library.Common.Extensions;
using Grfc.Library.EventBus.Abstractions;
using Grfc.Library.EventBus.Extensions;
using Grfc.Library.EventBus.RabbitMq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using StackExchange.Redis;

namespace Dns.Site
{
	public class Startup
	{
		private static bool IsDbMigrated = false;

		public Startup(IConfiguration configuration, IWebHostEnvironment hostEnvironment)
		{
			Configuration = configuration;
			HostEnvironment = hostEnvironment;
		}

		public IConfiguration Configuration { get; }
		public IWebHostEnvironment HostEnvironment { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddDbContext<DnsDbContext>(optionsAction: opt =>
				opt.UseNpgsql(EnvironmentExtensions.GetVariable(Program.PG_CONNECTION_STRING_WRITE), dbOpt => dbOpt.MigrationsAssembly("Dns.DAL")),
				contextLifetime: ServiceLifetime.Transient,
				optionsLifetime: ServiceLifetime.Transient);
			services.AddDbContext<DnsReadOnlyDbContext>(optionsAction: opt =>
				opt.UseNpgsql(EnvironmentExtensions.GetVariable(Program.PG_CONNECTION_STRING_READ)),
				contextLifetime: ServiceLifetime.Transient,
				optionsLifetime: ServiceLifetime.Transient);

			services.AddSingleton<ConnectionMultiplexer>(__ =>
			{
				var redisConnection = EnvironmentExtensions.GetVariable(Program.REDIS_CONNECTION);
				return ConnectionMultiplexer.Connect(redisConnection);
			});
			services.AddSwaggerGen(x =>
			{
				x.SwaggerDoc("v1", new OpenApiInfo
				{
					Version = "v1",
					Title = "DNS API",
					Description = "Service of analyze domains"
				});
				x.IncludeXmlComments("Dns.Site.xml");
				x.DescribeAllEnumsAsStrings();
			});

			services.AddCors(options =>
			{
				options.AddPolicy("AllowAll",
					builder =>
					{
						builder
						.AllowAnyOrigin()
						.AllowAnyMethod()
						.AllowAnyHeader();
					});
			});

			services.AddStackExchangeRedisCache(opts =>
			{
				opts.Configuration = EnvironmentExtensions.GetVariable(Program.REDIS_CONNECTION);
				opts.InstanceName = typeof(Dns.Site.Startup).Namespace;
			});

			services.AddEasyNetQ(EnvironmentExtensions.GetVariable(Program.RABBITMQ_CONNECTION));

			services.AddControllersWithViews()
				.AddNewtonsoftJson();

			services.AddSignalR(opts =>
			{
				opts.EnableDetailedErrors = true;
				opts.MaximumReceiveMessageSize = int.MaxValue;
			});

			if (!EnvironmentExtensions.GetVariable(Program.DISABLE_AUTH).ToBool())
			{
				services.ConfigureSharedCookieAuthentication();

				services.AddAuthorization(opts =>
					opts.AddPolicy("DnsPolicy", policy => policy.RequireAssertion(context => context.User.HasRole(UserRoles.DnsViewer))));
			}
			else
			{
				services.AddAuthorization(opts =>
					opts.AddPolicy("DnsPolicy", policy => policy.RequireAssertion(_ => true)));
			}

			//ADD SERVICES
			services.AddTransient<AttackService>();
			services.AddTransient<INotifyService, NotifyService>(sp =>
			{
				var emailFrom = EnvironmentExtensions.GetVariable(Program.NOTIFICATION_EMAIL_FROM);
				return new NotifyService(sp, emailFrom);
			});
			services.AddTransient<IExcelService, ExcelService>();
			services.AddHttpClient<IUserService, AuthService.Client>((_, client) =>
				client.BaseAddress = new Uri(EnvironmentExtensions.GetVariable(Program.AUTH_SERVER_URL)));

			services.AddHttpClient<INotifyApiService, NotificationService.Client>((_, client) =>
				client.BaseAddress = new Uri(EnvironmentExtensions.GetVariable(Program.NOTIFY_SERVICE_URL)));
			services.AddHttpClient<IVigruzkiService, VigruzkiService.Client>((_, client) =>
				client.BaseAddress = new Uri(EnvironmentExtensions.GetVariable(Program.VIGRUZKI_SERVICE_URL)));

			services.AddTransient<DnsAnalyzerHealthCheckMessageHandler>();
			services.AddTransient<UpdatedAttackMessageHandler>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider provider)
		{
			MigrateDataBase(provider);

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseStaticFiles();

			app.UseCookiePolicy();
			app.UseCors("AllowAll");
			app.UseRouting();

			app.UseAuthentication();
			app.UseAuthorization();

			app.UseSwagger();
			app.UseSwaggerUI(x => x.SwaggerEndpoint("/swagger/v1/swagger.json", "Notification API"));
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapDefaultControllerRoute();
				endpoints.MapFallbackToController("Index", "Home");
				endpoints.MapHub<AttackHub>("/attackHub");
				endpoints.MapHub<HealthCheckHub>("/healthHub");
			});

			var messageQueue = app.ApplicationServices.GetRequiredService<IMessageQueue>();
			var healthCheckEventHandler = app.ApplicationServices.GetRequiredService<DnsAnalyzerHealthCheckMessageHandler>();
			var healthQueue = EnvironmentExtensions.GetVariable(Program.RABBITMQ_HEALTH_QUEUE);
			messageQueue.Subscribe<DnsAnalyzerHealthCheckMessage, DnsAnalyzerHealthCheckMessageHandler>(healthQueue, healthCheckEventHandler);

			var updateAttackHandler = app.ApplicationServices.GetRequiredService<UpdatedAttackMessageHandler>();
			var updateAttackSubId = EnvironmentExtensions.GetVariable(Program.RABBITMQ_ATTACK_UPDATE_QUEUE);
			messageQueue.Subscribe<UpdatedAttackMessage, UpdatedAttackMessageHandler>(updateAttackSubId, updateAttackHandler);
		}

		private void MigrateDataBase(IServiceProvider provider)
		{
			if (!IsDbMigrated)
			{
				var context = provider.GetService<DnsDbContext>();
				context.Database.Migrate();
				IsDbMigrated = true;
			}
		}
	}
}