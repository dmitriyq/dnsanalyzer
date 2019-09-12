using Dns.DAL.DataMigration.MSSQL.Models;
using Microsoft.EntityFrameworkCore;

namespace Dns.DAL.DataMigration.MSSQL
{
	public class MsSqlContext : DbContext
	{
		public MsSqlContext()
		{
		}

		public MsSqlContext(DbContextOptions<MsSqlContext> options)
			: base(options)
		{
		}

		public virtual DbSet<Attacks> DnsAttacks { get; set; }
		public virtual DbSet<AttackHistories> AttackHistories { get; set; }
		public virtual DbSet<AttackGroups> AttackGroups { get; set; }
		public virtual DbSet<AttackNotes> Notes { get; set; }
		public virtual DbSet<AttackGroupHistories> GroupHistories { get; set; }
		public virtual DbSet<DomainInfo> DomainInfo { get; set; }
		public virtual DbSet<IpInfo> IpInfo { get; set; }
		public virtual DbSet<NameServers> NameServers { get; set; }
		public virtual DbSet<DomainNSs> DomainNs { get; set; }
		public virtual DbSet<DomainExcludedNames> DomainExcludedNames { get; set; }
		public virtual DbSet<SuspectDomains> SuspectDomains { get; set; }
		public virtual DbSet<StatisticHistories> StatisticHistories { get; set; }
		public virtual DbSet<WhiteDomains> WhiteDomains { get; set; }
#pragma warning disable 618
		public virtual DbQuery<SuspectDomainsView> vSuspectDomains { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			_Mapping.MapDns(modelBuilder);

			modelBuilder
			.Query<SuspectDomainsView>()
			.ToView("vSuspectDomains");
		}

#pragma warning enable 618
	}
}