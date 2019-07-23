using System;
using System.Collections.Generic;
using System.Text;
using Dns.DAL.DataMigration.MSSQL.Models;
using Microsoft.EntityFrameworkCore;

namespace Dns.DAL.DataMigration.MSSQL
{
	internal static class _Mapping
	{
		internal static void MapDns(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Attacks>(entity =>
			{
				entity.HasIndex(e => e.Id)
					.HasName("IX_Id");

				entity.HasIndex(e => e.BlackDomain)
					.HasName("IX_BlackDomain");
				entity.Property(e => e.BlackDomain)
					.HasMaxLength(255)
					.IsRequired();

				entity.HasIndex(e => e.WhiteDomain)
					.HasName("IX_WhiteDomain");
				entity.Property(e => e.WhiteDomain)
					.HasMaxLength(255)
					.IsRequired();

				entity.HasIndex(e => e.Ip)
					.HasName("IX_Ip");
				entity.Property(e => e.Ip)
					.HasMaxLength(45)
					.IsRequired();

				entity.Property(e => e.SubnetBlocked)
					.HasMaxLength(18);

				entity.HasIndex(e => e.Status)
					.HasName("IX_Status");

				entity.HasOne(g => g.AttackGroup)
					.WithMany(a => a.Attacks)
					.HasForeignKey(f => f.AttackGroupId)
					.HasConstraintName("FK_dbo.Attacks_dbo.AttackGroups_AttackGroupId");
			});

			modelBuilder.Entity<AttackGroups>(entity => {
				entity.HasIndex(e => e.Id)
					.HasName("IX_Id");
			});

			modelBuilder.Entity<AttackHistories>(entity => {
				entity.HasIndex(e => e.Id)
					.HasName("IX_Id");

				entity.HasOne(a => a.Attack)
					.WithMany(h => h.Histories)
					.HasForeignKey(f => f.AttackId)
					.HasConstraintName("FK_dbo.AttackHistories_dbo.Attacks_AttackId");
			});

			modelBuilder.Entity<AttackGroupHistories>(entity => {
				entity.HasIndex(e => e.Id)
					.HasName("IX_Id");

				entity.HasOne(a => a.AttackGroup)
					.WithMany(h => h.GroupHistories)
					.HasForeignKey(f => f.AttackGroupId)
					.HasConstraintName("FK_dbo.AttackGroupHistories_dbo.AttackGroups_AttackGroupId");
			});

			modelBuilder.Entity<AttackNotes>(entity => {
				entity.HasIndex(e => e.Id)
					.HasName("IX_Id");

				entity.HasOne(a => a.AttackGroup)
					.WithMany(h => h.Notes)
					.HasForeignKey(f => f.AttackGroupId)
					.HasConstraintName("FK_dbo.AttackNotes_dbo.AttackGroups_AttackGroupId");
			});

			modelBuilder.Entity<DomainInfo>(entity =>
			{
				entity.HasIndex(e => e.Id)
					.HasName("IX_Id");

				entity.HasIndex(e => e.Domain)
					.HasName("IX_Domain")
					.IsUnique(true);

				entity.Property(e => e.Domain)
					.HasMaxLength(255)
					.IsRequired();
			});

			modelBuilder.Entity<IpInfo>(entity =>
			{
				entity.HasIndex(e => e.Id)
					.HasName("IX_Id");

				entity.HasIndex(e => e.Ip)
					.HasName("IX_Ip")
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
					.HasName("IX_Id");

				entity.Property(e => e.NameServer)
					.HasMaxLength(255);
			});

			modelBuilder.Entity<DomainNSs>(entity =>
			{
				entity.HasIndex(e => e.Id)
					.HasName("IX_Id");

				entity.HasOne(d => d.Domain)
					.WithMany(dn => dn.DomainNS)
					.HasForeignKey(dn => dn.DomainId)
					.HasConstraintName("FK_dbo.DomainNSs_dbo.DomainInfo_DomainId");
				entity.HasOne(n => n.NameServer)
					.WithMany(dn => dn.DomainNS)
					.HasForeignKey(dn => dn.NsId)
					.HasConstraintName("FK_dbo.DomainNSs_dbo.NameServers_NsId");
			});

			modelBuilder.Entity<DomainExcludedNames>(entity =>
			{
				entity.HasIndex(e => e.Id)
					.HasName("IX_Id");

				entity.HasIndex(e => e.BlackDomain)
					.HasName("IX_BlackDomain");

				entity.Property(e => e.BlackDomain)
					.HasMaxLength(255)
					.IsRequired();

				entity.HasIndex(e => e.WhiteDomain)
					.HasName("IX_WhiteDomain");

				entity.Property(e => e.WhiteDomain)
					.HasMaxLength(255)
					.IsRequired();
			});

			modelBuilder.Entity<StatisticHistories>(entity =>
			{
				entity.HasIndex(e => e.Id)
					.HasName("IX_Id");
				entity.HasIndex(e => e.Date)
					.HasName("IX_Date")
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
					.HasName("IX_Id");

				entity.HasIndex(e => e.Domain)
					.HasName("IX_Domain")
					.IsUnique(true);

				entity.Property(e => e.Domain)
					.HasMaxLength(255)
					.IsRequired();
			});
		}
	}
}
