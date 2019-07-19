using System;
using System.Collections.Generic;
using System.Text;
using Dns.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace Dns.DAL
{
	public class DnsDbContext: DbContext
	{
		public DnsDbContext()
		{
		}

		public DnsDbContext(DbContextOptions<DnsDbContext> options)
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
		public virtual DbSet<SuspectDomainsView> vSuspectDomains { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.HasPostgresExtension("dblink");
			modelBuilder.HasPostgresExtension("citext");


			modelBuilder.Entity<Attacks>(entity =>
			{
				entity.HasIndex(e => e.Id)
					.HasName("IX_Attacks_Id");

				entity.HasIndex(e => e.BlackDomain)
					.HasName("IX_Attacks_BlackDomain");
				entity.Property(e => e.BlackDomain)
					.HasMaxLength(255)
					.IsRequired();

				entity.HasIndex(e => e.WhiteDomain)
					.HasName("IX_Attacks_WhiteDomain");
				entity.Property(e => e.WhiteDomain)
					.HasMaxLength(255)
					.IsRequired();

				entity.HasIndex(e => e.Ip)
					.HasName("IX_Attacks_Ip");
				entity.Property(e => e.Ip)
					.HasMaxLength(45)
					.IsRequired();

				entity.Property(e => e.SubnetBlocked)
					.HasMaxLength(18);

				entity.HasIndex(e => e.Status)
					.HasName("IX_Attacks_Status");

				entity.HasOne(g => g.AttackGroup)
					.WithMany(a => a.Attacks)
					.HasForeignKey(f => f.AttackGroupId)
					.HasConstraintName("FK_Attacks_AttackGroups_AttackGroupId");
			});

			modelBuilder.Entity<AttackGroups>(entity => {
				entity.HasIndex(e => e.Id)
					.HasName("IX_AttackGroups_Id");
				entity.Property(e => e.DateBegin).HasColumnType("timestamptz");
				entity.Property(e => e.DateClose).HasColumnType("timestamptz");

			});

			modelBuilder.Entity<AttackHistories>(entity => {
				entity.HasIndex(e => e.Id)
					.HasName("IX_AttackHistories_Id");

				entity.Property(e => e.Date).HasColumnType("timestamptz");

				entity.HasOne(a => a.Attack)
					.WithMany(h => h.Histories)
					.HasForeignKey(f => f.AttackId)
					.HasConstraintName("FK_AttackHistories_Attacks_AttackId");
			});

			modelBuilder.Entity<AttackGroupHistories>(entity => {
				entity.HasIndex(e => e.Id)
					.HasName("IX_AttackGroupHistories_Id");

				entity.Property(e => e.Date).HasColumnType("timestamptz");

				entity.HasOne(a => a.AttackGroup)
					.WithMany(h => h.GroupHistories)
					.HasForeignKey(f => f.AttackGroupId)
					.HasConstraintName("FK_AttackGroupHistories_AttackGroups_AttackGroupId");
			});

			modelBuilder.Entity<AttackNotes>(entity => {
				entity.HasIndex(e => e.Id)
					.HasName("IX_AttackNotes_Id");

				entity.Property(e => e.Date).HasColumnType("timestamptz");
				entity.Property(e => e.Text).HasColumnType("citext");

				entity.HasOne(a => a.AttackGroup)
					.WithMany(h => h.Notes)
					.HasForeignKey(f => f.AttackGroupId)
					.HasConstraintName("FK_AttackNotes_AttackGroups_AttackGroupId");
			});

			modelBuilder.Entity<DomainInfo>(entity =>
			{
				entity.HasIndex(e => e.Id)
					.HasName("IX_DomainInfo_Id");

				entity.HasIndex(e => e.Domain)
					.HasName("IX_DomainInfo_Domain")
					.IsUnique(true);

				entity.Property(e => e.DateCreate).HasColumnType("timestamptz");
				entity.Property(e => e.DateUntil).HasColumnType("timestamptz");

				entity.Property(e => e.Domain)
					.HasMaxLength(255)
					.IsRequired();
			});

			modelBuilder.Entity<IpInfo>(entity =>
			{
				entity.HasIndex(e => e.Id)
					.HasName("IX_IpInfo_Id");

				entity.HasIndex(e => e.Ip)
					.HasName("IX_IpInfo_Ip")
					.IsUnique();

				entity.Property(e => e.Ip)
					.HasMaxLength(45)
					.IsRequired();

				entity.Property(e => e.Subnet)
					.HasMaxLength(18);
			});

			modelBuilder.Entity<NameServers>(entity =>
			{
				entity.HasIndex(e => e.Id)
					.HasName("IX_NameServers_Id");

				entity.Property(e => e.NameServer)
					.HasMaxLength(255);
			});

			modelBuilder.Entity<DomainNSs>(entity =>
			{
				entity.HasIndex(e => e.Id)
					.HasName("IX_DomainNSs_Id");

				entity.HasOne(d => d.Domain)
					.WithMany(dn => dn.DomainNS)
					.HasForeignKey(dn => dn.DomainId)
					.HasConstraintName("FK_DomainNSs_DomainInfo_DomainId");
				entity.HasOne(n => n.NameServer)
					.WithMany(dn => dn.DomainNS)
					.HasForeignKey(dn => dn.NsId)
					.HasConstraintName("FK_DomainNSs_NameServers_NsId");
			});

			modelBuilder.Entity<DomainExcludedNames>(entity =>
			{
				entity.HasIndex(e => e.Id)
					.HasName("IX_DomainExcludedNames_Id");

				entity.HasIndex(e => e.BlackDomain)
					.HasName("IX_DomainExcludedNames_BlackDomain");

				entity.Property(e => e.BlackDomain)
					.HasMaxLength(255)
					.IsRequired();

				entity.HasIndex(e => e.WhiteDomain)
					.HasName("IX_DomainExcludedNames_WhiteDomain");

				entity.Property(e => e.WhiteDomain)
					.HasMaxLength(255)
					.IsRequired();
			});

			modelBuilder.Entity<StatisticHistories>(entity =>
			{
				entity.HasIndex(e => e.Id)
					.HasName("IX_StatisticHistories_Id");

				entity.Property(e => e.Date).HasColumnType("timestamptz");

				entity.HasIndex(e => e.Date)
					.HasName("IX_StatisticHistories_Date")
					.IsUnique();
			});

			modelBuilder.Entity<SuspectDomains>(entity =>
			{
				entity.HasKey(e => e.Id)
					.HasName("PK_SuspectDomain_Id");
				entity.Property(e => e.Id)
					.ValueGeneratedNever();
			});

			modelBuilder.Entity<WhiteDomains>(entity =>
			{
				entity.HasIndex(e => e.Id)
					.HasName("IX_WhiteDomains_Id");

				entity.Property(e => e.DateAdded).HasColumnType("timestamptz");

				entity.HasIndex(e => e.Domain)
					.HasName("IX_WhiteDomains_Domain")
					.IsUnique(true);

				entity.Property(e => e.Domain)
					.HasMaxLength(255)
					.IsRequired();
			});

			modelBuilder
				.Entity<SuspectDomainsView>()
				.ToTable("vSuspectDomains");
		}
	}
}
