using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Dns.DAL.DataMigration
{
	class Program
	{
		public const string SQL_CONNECTION = "data source=opdns.cmim.ru;Initial Catalog=DnsAttack;User id=domainCheckerUser;Password=domainCheckerUser;MultipleActiveResultSets=True;App=EntityFramework";
		public const string PG_CONNECTION = "Server=k8s.cmim.ru;Port=5432;Database=dnsdb;User Id=dnsuser;Password=dnsuser;";

		const string AttackGroups = nameof(AttackGroups);
		const string AttackHistories = nameof(AttackHistories);
		const string DnsAttacks = nameof(DnsAttacks);
		const string DomainExcludedNames = nameof(DomainExcludedNames);
		const string DomainInfo = nameof(DomainInfo);
		const string DomainNs = nameof(DomainNs);
		const string GroupHistories = nameof(GroupHistories);
		const string IpInfo = nameof(IpInfo);
		const string NameServers = nameof(NameServers);
		const string Notes = nameof(Notes);
		const string StatisticHistories = nameof(StatisticHistories);
		const string SuspectDomains = nameof(SuspectDomains);
		const string WhiteDomains = nameof(WhiteDomains);

		static string[] TABLES = new string[]
		{
			AttackGroups,
			AttackHistories,
			DnsAttacks,
			DomainExcludedNames,
			DomainInfo,
			DomainNs,
			GroupHistories,
			IpInfo,
			NameServers,
			Notes,
			StatisticHistories,
			SuspectDomains,
			WhiteDomains
		};

		static void Main(string[] args)
		{
			Console.WriteLine("Starting migartion...\r\n");
			Migrate();
			Console.ReadLine();

		}
		private static void Migrate()
		{
			var sqlOpt = new DbContextOptionsBuilder<MSSQL.MsSqlContext>();
			sqlOpt.UseSqlServer(SQL_CONNECTION);
			sqlOpt.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

			var pgOpt = new DbContextOptionsBuilder<DnsDbContext>();
			pgOpt.UseNpgsql(PG_CONNECTION);

			using (var sql = new MSSQL.MsSqlContext(sqlOpt.Options))
			using (var pg = new DnsDbContext(pgOpt.Options))
			using (var transaction = pg.Database.BeginTransaction())
			{
				pg.TruncateTables(TABLES);
				pg.DisableTriggers(TABLES);

				pg.MigrateTable(AttackGroups, sql.AttackGroups, pg.AttackGroups,
					ag => new Dns.DAL.Models.AttackGroups
					{
						DateBegin = ag.DateBegin.ToUniversalTime(),
						DateClose = ag.DateClose?.ToUniversalTime(),
						Id = ag.Id,
						Status = ag.Status
					}, maxId: () => sql.AttackGroups.Max(x => x.Id));

				pg.MigrateTable(AttackHistories, sql.AttackHistories, pg.AttackHistories,
					ah => new Dns.DAL.Models.AttackHistories
					{
						AttackId = ah.AttackId,
						CurrentStatus = ah.CurrentStatus,
						Date = ah.Date.ToUniversalTime(),
						Id = ah.Id,
						PrevStatus = ah.PrevStatus
					}, maxId: () => sql.AttackHistories.Max(x => x.Id));

				pg.MigrateTable(DnsAttacks, sql.DnsAttacks, pg.DnsAttacks,
					a => new Dns.DAL.Models.Attacks
					{
						AttackGroupId = a.AttackGroupId,
						BlackDomain = a.BlackDomain,
						Id = a.Id,
						Ip = a.Ip,
						IpBlocked = a.IpBlocked,
						Status = a.Status,
						SubnetBlocked = a.SubnetBlocked,
						WhiteDomain = a.WhiteDomain
					}, maxId: () => sql.DnsAttacks.Max(x => x.Id));

				pg.MigrateTable(DomainExcludedNames, sql.DomainExcludedNames, pg.DomainExcludedNames,
					den => new Dns.DAL.Models.DomainExcludedNames
					{
						BlackDomain = den.BlackDomain,
						Id = den.Id,
						WhiteDomain = den.WhiteDomain
					}, maxId: () => sql.DomainExcludedNames.Max(x => x.Id));

				pg.MigrateTable(DomainInfo, sql.DomainInfo, pg.DomainInfo,
					di => new Dns.DAL.Models.DomainInfo
					{
						Company = di.Company,
						DateCreate = di.DateCreate?.ToUniversalTime(),
						DateUntil = di.DateUntil?.ToUniversalTime(),
						Id = di.Id,
						Registrant = di.Registrant
					}, maxId: () => sql.DomainInfo.Max(x => x.Id));

				pg.MigrateTable(DomainNs, sql.DomainNs, pg.DomainNs,
					dn => new Dns.DAL.Models.DomainNSs
					{
						DomainId = dn.DomainId,
						Id = dn.Id,
						NsId = dn.NsId
					}, maxId: () => sql.DomainNs.Max(x => x.Id));

				pg.MigrateTable(GroupHistories, sql.GroupHistories, pg.GroupHistories,
					gh => new Dns.DAL.Models.AttackGroupHistories
					{
						AttackGroupId = gh.AttackGroupId,
						CurrentStatus = gh.CurrentStatus,
						Date = gh.Date.ToUniversalTime(),
						Id = gh.Id,
						PrevStatus = gh.PrevStatus
					}, maxId: () => sql.GroupHistories.Max(x => x.Id));

				pg.MigrateTable(IpInfo, sql.IpInfo, pg.IpInfo,
					ii => new Dns.DAL.Models.IpInfo
					{
						Company = ii.Company,
						Country = ii.Country,
						Id = ii.Id,
						Ip = ii.Ip,
						Subnet = ii.Subnet
					}, maxId: () => sql.IpInfo.Max(x => x.Id));

				pg.MigrateTable(NameServers, sql.NameServers, pg.NameServers,
					ns => new Dns.DAL.Models.NameServers
					{
						Id = ns.Id,
						NameServer = ns.NameServer
					}, maxId: () => sql.NameServers.Max(x => x.Id));

				pg.MigrateTable(Notes, sql.Notes, pg.Notes,
					n => new Dns.DAL.Models.AttackNotes
					{
						AttackGroupId = n.AttackGroupId,
						Date = n.Date.ToUniversalTime(),
						Id = n.Id,
						Text = n.Text
					}, maxId: () => sql.Notes.Max(x => x.Id));

				pg.MigrateTable(StatisticHistories, sql.StatisticHistories, pg.StatisticHistories,
					sh => new Dns.DAL.Models.StatisticHistories
					{
						AttackComplete = sh.AttackComplete,
						AttackIntersection = sh.AttackIntersection,
						AttackNew = sh.AttackNew,
						AttackTotal = sh.AttackTotal,
						Date = sh.Date.ToUniversalTime(),
						GroupAttack = sh.GroupAttack,
						GroupComplete = sh.GroupComplete,
						GroupDynamic = sh.GroupDynamic,
						GroupNew = sh.GroupNew,
						GroupThreat = sh.GroupThreat,
						GroupTotal = sh.GroupTotal,
						Id = sh.Id
					}, maxId: () => sql.StatisticHistories.Max(x => x.Id));

				pg.MigrateTable(SuspectDomains, sql.SuspectDomains, pg.SuspectDomains,
					sd => new Dns.DAL.Models.SuspectDomains
					{
						Domain = sd.Domain,
						Id = sd.Id,
						Ip = sd.Ip
					}, false);

				pg.MigrateTable(WhiteDomains, sql.WhiteDomains, pg.WhiteDomains,
					wd => new Dns.DAL.Models.WhiteDomains
					{
						DateAdded = wd.DateAdded.ToUniversalTime(),
						Domain = wd.Domain,
						Id = wd.Id
					}, maxId: () => sql.WhiteDomains.Max(x => x.Id));

				pg.EnableTriggers(TABLES);
				pg.SaveChanges();
				transaction.Commit();
				Console.WriteLine("Migration Complete");
			}
		}
	}
}
