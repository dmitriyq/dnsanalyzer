using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Dns.DAL;
using Dns.DAL.Enums;
using Dns.DAL.Models;
using Grfc.Library.Common.Enums;
using Grfc.Library.Common.Extensions;
using Grfc.Library.Notification.Models;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Dns.Site.Services
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

		private const string EMAIL_SUBJECT = "[Система выявления DNS атак]";

		private readonly ILogger<NotifyService> _logger;
		private readonly DnsDbContext _dbContext;
		private readonly string _emailFrom;

		public NotifyService(ILogger<NotifyService> logger, DnsDbContext dbContext, string emailFrom)
		{
			_logger = logger;
			_dbContext = dbContext;
			_emailFrom = emailFrom;
		}

		public async Task<NotificationBase> BuildAttackMessage(string user, params int[] attackIds)
		{
			_logger.LogInformation("Consctructing attack message");
			var notify = new NotificationBase() { Login = user, SiteType = SiteTypes.Dns };

			var attacksToNotify = await _dbContext
				.DnsAttacks
				.Where(x => attackIds.Contains(x.Id))
				.Include(x => x.Histories)
				.ToListAsync().ConfigureAwait(false);

			notify.Email = BuildAttackEmailMessage(attacksToNotify);
			return notify;
		}

		public async Task<NotificationBase> BuildGroupMessage(string user, params int[] attackIds)
		{
			_logger.LogInformation("Consctructing group attack message");
			var notify = new NotificationBase() { Login = user, SiteType = SiteTypes.Dns };

			List<AttackGroups> attacksToNotify = await _dbContext
				.AttackGroups
				.Where(x => attackIds.Contains(x.Id))
				.Include(x => x.GroupHistories)
				.Include(x => x.Attacks)
				.ToListAsync().ConfigureAwait(false);

			notify.Email = BuildGroupEmailMessage(attacksToNotify);
			return notify;
		}

		private NotificationEmail BuildAttackEmailMessage(IEnumerable<Attacks> attacks)
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
					.Replace(EMAIL_URL_KEY, attack.AttackGroupId.ToString());
				tableBody.AppendChild(HtmlNode.CreateNode(newRow));
			}
			return new NotificationEmail
			{
				Body = htmlBody.DocumentNode.OuterHtml,
				From = EnvironmentExtensions.GetVariable(_emailFrom),
				Subject = "[Система выявления DNS атак]"
			};
		}

		private NotificationEmail BuildGroupEmailMessage(IEnumerable<AttackGroups> attacks)
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
			return new NotificationEmail
			{
				Body = htmlBody.DocumentNode.OuterHtml,
				From = EnvironmentExtensions.GetVariable(_emailFrom),
				Subject = EMAIL_SUBJECT
			};
		}
	}
}
