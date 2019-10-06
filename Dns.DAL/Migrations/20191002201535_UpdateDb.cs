using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Dns.DAL.Migrations
{
    public partial class UpdateDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropColumn(
            //    name: "LastUpdate",
            //    table: "AttackGroups");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.AddColumn<DateTimeOffset>(
            //    name: "LastUpdate",
            //    table: "AttackGroups",
            //    type: "timestamptz",
            //    nullable: false,
            //    defaultValueSql: "NOW()");
        }
    }
}
