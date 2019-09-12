using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dns.DAL.Models;

namespace Dns.Site.Services
{
	public interface IExcelService
	{
		public const string EXCEL_MIMETYPE = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

		byte[] ExportSuspectDomains(IEnumerable<SuspectDomainsView> records);
		byte[] ExportDnsAttack(IEnumerable<AttackGroups> models, DateTimeOffset from, DateTimeOffset to);
		byte[] ExportWhiteList(IEnumerable<WhiteDomains> records);
	}
}
