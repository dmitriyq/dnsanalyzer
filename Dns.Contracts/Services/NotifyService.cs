using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dns.DAL;
using Dns.DAL.Enums;
using Dns.DAL.Models;
using Grfc.Library.Common.Enums;
using Grfc.Library.Common.Extensions;
using Grfc.Library.EventBus.Abstractions;
using Grfc.Library.Notification.Interfaces;
using Grfc.Library.Notification.Messages;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dns.Contracts.Services
{
	public class NotifyService : INotifyService
	{
		private static readonly string TemplatePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!, "Templates");
		private static readonly string EmailPath = Path.Combine(TemplatePath, "Email");

		private const string EMAIL_ATTACK_TEMPLATE = "dns_attack_ip.html";
		private const string EMAIL_ATTACK_TEMPLATE_ADDITION = "dns_attack_ip_row.html";

		private const string EMAIL_GROUP_TEMPLATE = "dns_attack_group.html";
		private const string EMAIL_GROUP_TEMPLATE_ADDITION = "dns_attack_group_row.html";

		private const string EMAIL_BLACK_DOMAIN_KEY = "{{BLACK_DOMAIN}}";
		private const string EMAIL_WHITE_DOMAIN_KEY = "{{WHITE_DOMAIN}}";
		private const string EMAIL_STATUS_KEY = "{{STATUS}}";
		private const string EMAIL_URL_KEY = "{{ATTACK_ID}}";
		private const string EMAIL_IP_KEY = "{{IP}}";
		private const string HOSTNAME_KEY = "{{HOSTNAME}}";

		private const string EMAIL_SUBJECT = "[Система выявления DNS атак]";

		private readonly IServiceProvider _serviceProvider;
		private readonly IMessageQueue _messageQueue;
		private readonly string _emailFrom;
		private readonly string _hostname;

		public NotifyService(IServiceProvider serviceProvider, string emailFrom, string hostname)
		{
			_serviceProvider = serviceProvider;
			_messageQueue = _serviceProvider.GetRequiredService<IMessageQueue>();
			_emailFrom = emailFrom;
			_hostname = hostname;
		}

		public NotificationMessage BuildAttackMessage(string user, params int[] attackIds)
		{
			var notify = new NotificationMessage(user, NotificationTypes.None, SiteTypes.Dns);

			using var scope = _serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<DnsDbContext>();
			var attacksToNotify = db
				.DnsAttacks
				.Where(x => attackIds.Contains(x.Id))
				.Include(x => x.Histories)
				.ToList();

			notify.EmailMessage = BuildAttackEmailMessage(attacksToNotify);
			return notify;
		}

		public NotificationMessage BuildGroupMessage(string user, params int[] groupIds)
		{
			var notify = new NotificationMessage(user, NotificationTypes.None, SiteTypes.Dns);

			using var scope = _serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<DnsDbContext>();
			List<AttackGroups> attacksToNotify = db
				.AttackGroups
				.Where(x => groupIds.Contains(x.Id))
				.Include(x => x.GroupHistories)
				.Include(x => x.Attacks)
				.ToList();

			notify.EmailMessage = BuildGroupEmailMessage(attacksToNotify);
			return notify;
		}

		private EmailMessage BuildAttackEmailMessage(IEnumerable<Attacks> attacks)
		{
			var emailBody = File.ReadAllText(Path.Combine(EmailPath, EMAIL_ATTACK_TEMPLATE));
			var row = File.ReadAllText(Path.Combine(EmailPath, EMAIL_ATTACK_TEMPLATE_ADDITION));

			var htmlBody = new HtmlDocument();
			htmlBody.LoadHtml(emailBody);

			var tableBody = htmlBody.DocumentNode.Descendants("tbody").First();

			foreach (var attack in attacks)
			{
				var lastChange = attack.Histories.OrderByDescending(x => x.Id).FirstOrDefault();
				var newStatus = lastChange.CurrentEnum.GetDisplayName();
				var statusText = newStatus;
				if (lastChange.PrevStatusEnum != AttackStatusEnum.None)
					statusText = $"{lastChange.PrevStatusEnum.GetDisplayName()} -> {newStatus}";

				var newRow = row
					.Replace(EMAIL_BLACK_DOMAIN_KEY, attack.BlackDomain)
					.Replace(EMAIL_WHITE_DOMAIN_KEY, attack.WhiteDomain)
					.Replace(EMAIL_STATUS_KEY, statusText)
					.Replace(EMAIL_IP_KEY, attack.Ip)
					.Replace(HOSTNAME_KEY, _hostname)
					.Replace(EMAIL_URL_KEY, attack.AttackGroupId.ToString());
				tableBody.AppendChild(HtmlNode.CreateNode(newRow));
			}
			return new EmailMessage
			{
				Body = htmlBody.DocumentNode.OuterHtml,
				From = _emailFrom,
				Subject = "[Система выявления DNS атак]"
			};
		}

		private EmailMessage BuildGroupEmailMessage(IEnumerable<AttackGroups> attacks)
		{
			var emailBody = File.ReadAllText(Path.Combine(EmailPath, EMAIL_GROUP_TEMPLATE));
			var row = File.ReadAllText(Path.Combine(EmailPath, EMAIL_GROUP_TEMPLATE_ADDITION));

			var htmlBody = new HtmlDocument();
			htmlBody.LoadHtml(emailBody);

			var tableBody = htmlBody.DocumentNode.Descendants("tbody").First();

			foreach (var attack in attacks)
			{
				var blackDomain = attack.Attacks.First().BlackDomain;
				var whiteDomain = attack.Attacks.First().WhiteDomain;
				var lastChange = attack.GroupHistories.OrderByDescending(x => x.Id).First();
				var newStatus = lastChange.CurrentEnum.GetDisplayName();
				var statusText = newStatus;
				if (lastChange.PrevStatusEnum != AttackGroupStatusEnum.None)
					statusText = $"{lastChange.PrevStatusEnum.GetDisplayName()} -> {newStatus}";

				var newRow = row
					.Replace(EMAIL_BLACK_DOMAIN_KEY, blackDomain)
					.Replace(EMAIL_WHITE_DOMAIN_KEY, whiteDomain)
					.Replace(EMAIL_STATUS_KEY, statusText)
					.Replace(EMAIL_URL_KEY, attack.Id.ToString());
				tableBody.AppendChild(HtmlNode.CreateNode(newRow));
			}
			return new EmailMessage
			{
				Body = htmlBody.DocumentNode.OuterHtml,
				From = _emailFrom,
				Subject = EMAIL_SUBJECT
			};
		}

		public EmailMessage BuildEmail() => new EmailMessage();

		public SynologyMessage BuildSynology() => new SynologyMessage();

		public TelegramMessage BuildTelegram() => new TelegramMessage();

		public Task SendAsync(NotificationMessage message)
		{
			return _messageQueue.PublishAsync(message);
		}
	}
}
