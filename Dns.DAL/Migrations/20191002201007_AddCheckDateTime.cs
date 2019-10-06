using Microsoft.EntityFrameworkCore.Migrations;

namespace Dns.DAL.Migrations
{
    public partial class AddCheckDateTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropTable(
            //    name: "vSuspectDomains");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.CreateTable(
            //    name: "vSuspectDomains",
            //    columns: table => new
            //    {
            //        Company = table.Column<string>(type: "text", nullable: true),
            //        Country = table.Column<string>(type: "text", nullable: true),
            //        Domain = table.Column<string>(type: "text", nullable: true),
            //        Ip = table.Column<string>(type: "text", nullable: true),
            //        Subnet = table.Column<string>(type: "text", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //    });
        }
    }
}
