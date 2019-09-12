using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;

namespace Dns.DAL.ViewHelpers
{
	public static class vSuspectDomainsHelper
	{
		public static OperationBuilder<SqlOperation> CreateSuspectView(this MigrationBuilder migrationBuilder)
		{
			return migrationBuilder.Sql(@"
				CREATE OR REPLACE view ""public"".""vSuspectDomains""
				as
				select
				sd.""Domain"" as ""Domain"",
				sd.""Ip"" as ""Ip"",
				i.""Company"" as ""Company"",
				i.""Country"" as ""Country"",
				i.""Subnet"" as ""Subnet""
				from
				""public"".""SuspectDomains"" sd
				left join ""public"".""IpInfo"" i on sd.""Ip"" = i.""Ip""
			");
		}

		public static OperationBuilder<SqlOperation> DropSuspectView(this MigrationBuilder migrationBuilder)
			=> migrationBuilder.Sql(@"DROP VIEW IF EXISTS ""public"".""vSuspectDomains""");
	}
}