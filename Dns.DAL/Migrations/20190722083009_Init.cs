using System;
using Dns.DAL.ViewHelpers;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Dns.DAL.Migrations
{
	public partial class Init : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropSuspectView();

			migrationBuilder.AlterDatabase()
				.Annotation("Npgsql:PostgresExtension:citext", ",,")
				.Annotation("Npgsql:PostgresExtension:dblink", ",,");

			migrationBuilder.CreateTable(
				name: "AttackGroups",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
					DateBegin = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
					DateClose = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
					Status = table.Column<int>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_AttackGroups", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "DomainExcludedNames",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
					WhiteDomain = table.Column<string>(maxLength: 255, nullable: false),
					BlackDomain = table.Column<string>(maxLength: 255, nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_DomainExcludedNames", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "DomainInfo",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
					Domain = table.Column<string>(maxLength: 255, nullable: false),
					Company = table.Column<string>(nullable: true),
					Registrant = table.Column<string>(nullable: true),
					DateCreate = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
					DateUntil = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_DomainInfo", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "IpInfo",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
					Ip = table.Column<string>(maxLength: 45, nullable: false),
					Subnet = table.Column<string>(maxLength: 18, nullable: true),
					Company = table.Column<string>(nullable: true),
					Country = table.Column<string>(nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_IpInfo", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "NameServers",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
					NameServer = table.Column<string>(maxLength: 255, nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_NameServers", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "StatisticHistories",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
					Date = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
					GroupNew = table.Column<int>(nullable: false),
					GroupThreat = table.Column<int>(nullable: false),
					GroupDynamic = table.Column<int>(nullable: false),
					GroupAttack = table.Column<int>(nullable: false),
					GroupComplete = table.Column<int>(nullable: false),
					GroupTotal = table.Column<int>(nullable: false),
					AttackNew = table.Column<int>(nullable: false),
					AttackIntersection = table.Column<int>(nullable: false),
					AttackComplete = table.Column<int>(nullable: false),
					AttackTotal = table.Column<int>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_StatisticHistories", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "SuspectDomains",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false),
					Domain = table.Column<string>(nullable: true),
					Ip = table.Column<string>(nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_SuspectDomain_Id", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "WhiteDomains",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
					Domain = table.Column<string>(maxLength: 255, nullable: false),
					DateAdded = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_WhiteDomains", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "DnsAttacks",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
					WhiteDomain = table.Column<string>(maxLength: 255, nullable: false),
					BlackDomain = table.Column<string>(maxLength: 255, nullable: false),
					Ip = table.Column<string>(maxLength: 45, nullable: false),
					IpBlocked = table.Column<bool>(nullable: false),
					SubnetBlocked = table.Column<string>(maxLength: 18, nullable: true),
					Status = table.Column<int>(nullable: false),
					AttackGroupId = table.Column<int>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_DnsAttacks", x => x.Id);
					table.ForeignKey(
						name: "FK_Attacks_AttackGroups_AttackGroupId",
						column: x => x.AttackGroupId,
						principalTable: "AttackGroups",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "GroupHistories",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
					Date = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
					PrevStatus = table.Column<int>(nullable: false),
					CurrentStatus = table.Column<int>(nullable: false),
					AttackGroupId = table.Column<int>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_GroupHistories", x => x.Id);
					table.ForeignKey(
						name: "FK_AttackGroupHistories_AttackGroups_AttackGroupId",
						column: x => x.AttackGroupId,
						principalTable: "AttackGroups",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "Notes",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
					Text = table.Column<string>(type: "citext", nullable: true),
					Date = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
					AttackGroupId = table.Column<int>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Notes", x => x.Id);
					table.ForeignKey(
						name: "FK_AttackNotes_AttackGroups_AttackGroupId",
						column: x => x.AttackGroupId,
						principalTable: "AttackGroups",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "DomainNs",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
					DomainId = table.Column<int>(nullable: false),
					NsId = table.Column<int>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_DomainNs", x => x.Id);
					table.ForeignKey(
						name: "FK_DomainNSs_DomainInfo_DomainId",
						column: x => x.DomainId,
						principalTable: "DomainInfo",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_DomainNSs_NameServers_NsId",
						column: x => x.NsId,
						principalTable: "NameServers",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "AttackHistories",
				columns: table => new
				{
					Id = table.Column<int>(nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
					Date = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
					PrevStatus = table.Column<int>(nullable: false),
					CurrentStatus = table.Column<int>(nullable: false),
					AttackId = table.Column<int>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_AttackHistories", x => x.Id);
					table.ForeignKey(
						name: "FK_AttackHistories_Attacks_AttackId",
						column: x => x.AttackId,
						principalTable: "DnsAttacks",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateIndex(
				name: "IX_AttackGroups_Id",
				table: "AttackGroups",
				column: "Id");

			migrationBuilder.CreateIndex(
				name: "IX_AttackHistories_AttackId",
				table: "AttackHistories",
				column: "AttackId");

			migrationBuilder.CreateIndex(
				name: "IX_AttackHistories_Id",
				table: "AttackHistories",
				column: "Id");

			migrationBuilder.CreateIndex(
				name: "IX_DnsAttacks_AttackGroupId",
				table: "DnsAttacks",
				column: "AttackGroupId");

			migrationBuilder.CreateIndex(
				name: "IX_Attacks_BlackDomain",
				table: "DnsAttacks",
				column: "BlackDomain");

			migrationBuilder.CreateIndex(
				name: "IX_Attacks_Id",
				table: "DnsAttacks",
				column: "Id");

			migrationBuilder.CreateIndex(
				name: "IX_Attacks_Ip",
				table: "DnsAttacks",
				column: "Ip");

			migrationBuilder.CreateIndex(
				name: "IX_Attacks_Status",
				table: "DnsAttacks",
				column: "Status");

			migrationBuilder.CreateIndex(
				name: "IX_Attacks_WhiteDomain",
				table: "DnsAttacks",
				column: "WhiteDomain");

			migrationBuilder.CreateIndex(
				name: "IX_DomainExcludedNames_BlackDomain",
				table: "DomainExcludedNames",
				column: "BlackDomain");

			migrationBuilder.CreateIndex(
				name: "IX_DomainExcludedNames_Id",
				table: "DomainExcludedNames",
				column: "Id");

			migrationBuilder.CreateIndex(
				name: "IX_DomainExcludedNames_WhiteDomain",
				table: "DomainExcludedNames",
				column: "WhiteDomain");

			migrationBuilder.CreateIndex(
				name: "IX_DomainInfo_Domain",
				table: "DomainInfo",
				column: "Domain",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_DomainInfo_Id",
				table: "DomainInfo",
				column: "Id");

			migrationBuilder.CreateIndex(
				name: "IX_DomainNs_DomainId",
				table: "DomainNs",
				column: "DomainId");

			migrationBuilder.CreateIndex(
				name: "IX_DomainNSs_Id",
				table: "DomainNs",
				column: "Id");

			migrationBuilder.CreateIndex(
				name: "IX_DomainNs_NsId",
				table: "DomainNs",
				column: "NsId");

			migrationBuilder.CreateIndex(
				name: "IX_GroupHistories_AttackGroupId",
				table: "GroupHistories",
				column: "AttackGroupId");

			migrationBuilder.CreateIndex(
				name: "IX_AttackGroupHistories_Id",
				table: "GroupHistories",
				column: "Id");

			migrationBuilder.CreateIndex(
				name: "IX_IpInfo_Id",
				table: "IpInfo",
				column: "Id");

			migrationBuilder.CreateIndex(
				name: "IX_IpInfo_Ip",
				table: "IpInfo",
				column: "Ip",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_NameServers_Id",
				table: "NameServers",
				column: "Id");

			migrationBuilder.CreateIndex(
				name: "IX_Notes_AttackGroupId",
				table: "Notes",
				column: "AttackGroupId");

			migrationBuilder.CreateIndex(
				name: "IX_AttackNotes_Id",
				table: "Notes",
				column: "Id");

			migrationBuilder.CreateIndex(
				name: "IX_StatisticHistories_Date",
				table: "StatisticHistories",
				column: "Date",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_StatisticHistories_Id",
				table: "StatisticHistories",
				column: "Id");

			migrationBuilder.CreateIndex(
				name: "IX_WhiteDomains_Domain",
				table: "WhiteDomains",
				column: "Domain",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_WhiteDomains_Id",
				table: "WhiteDomains",
				column: "Id");

			migrationBuilder.CreateSuspectView();
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropSuspectView();

			migrationBuilder.DropTable(
				name: "AttackHistories");

			migrationBuilder.DropTable(
				name: "DomainExcludedNames");

			migrationBuilder.DropTable(
				name: "DomainNs");

			migrationBuilder.DropTable(
				name: "GroupHistories");

			migrationBuilder.DropTable(
				name: "IpInfo");

			migrationBuilder.DropTable(
				name: "Notes");

			migrationBuilder.DropTable(
				name: "StatisticHistories");

			migrationBuilder.DropTable(
				name: "SuspectDomains");

			migrationBuilder.DropTable(
				name: "WhiteDomains");

			migrationBuilder.DropTable(
				name: "DnsAttacks");

			migrationBuilder.DropTable(
				name: "DomainInfo");

			migrationBuilder.DropTable(
				name: "NameServers");

			migrationBuilder.DropTable(
				name: "AttackGroups");
		}
	}
}