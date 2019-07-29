using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dns.DAL;
using Dns.Library;
using Dns.Site.Hubs;
using Dns.Site.Services;
using Grfc.Library.Auth.Helpers;
using Grfc.Library.Common.Enums;
using Grfc.Library.Common.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace Dns.Site
{
	public class Startup
	{
		private static bool IsDbMigrated = false;

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddDbContext<DnsDbContext>(opt =>
				opt.UseNpgsql(EnvironmentExtensions.GetVariable(EnvVars.PG_CONNECTION_STRING_WRITE), dbOpt => dbOpt.MigrationsAssembly("Dns.DAL")));
			services.AddDbContext<DnsReadOnlyDbContext>(opt =>
				opt.UseNpgsql(EnvironmentExtensions.GetVariable(EnvVars.PG_CONNECTION_STRING_READ)));

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

			services.AddControllers()
				.AddNewtonsoftJson();
			services.AddSignalR(opts => opts.EnableDetailedErrors = true);

			services.ConfigureSharedCookieAuthentication();

			services.AddAuthorization(opts =>
			{
				opts.AddPolicy("DnsPolicy", policy => policy.RequireAssertion(context => context.User.HasRole(UserRoles.DnsViewer)));
			});

			//ADD SERVICES
			services.AddScoped<AttackService>();
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
			app.UseSwaggerUI(x =>
			{
				x.SwaggerEndpoint("/swagger/v1/swagger.json", "Notification API");
			});
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
				endpoints.MapHub<AttackHub>("/attackHub");
			});
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
