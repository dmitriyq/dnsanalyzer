using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using ClosedXML.Excel;
using Dns.DAL.Models;

namespace Dns.Library.Services
{
	public class ExcelService
	{
		public const string EXCEL_MIMETYPE = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

		public byte[] ExportSuspectDomains(IEnumerable<SuspectDomainsView> records)
		{
			var wb = new XLWorkbook();

			var stringType = typeof(string);
			var dt = new DataTable("Подозрительные домены");
			dt.Columns.Add("Домен", stringType);
			dt.Columns.Add("IP", stringType);
			dt.Columns.Add("Подсеть", stringType);
			dt.Columns.Add("Страна", stringType);
			dt.Columns.Add("Организация", stringType);

			foreach (var record in records)
			{
				dt.Rows.Add(record.Domain, record.Ip, record.Subnet, record.Country, record.Company);
			}
			var ws = wb.Worksheets.Add(dt);
			using (var ms = new MemoryStream())
			{
				wb.SaveAs(ms);
				return ms.ToArray();
			}
		}

		public byte[] ExportDnsAttack(IEnumerable<AttackGroups> models, DateTimeOffset from, DateTimeOffset to)
		{
			var wb = new XLWorkbook();

			var stringType = typeof(string);
			var dt = new DataTable($"DNS-атаки_{from.ToString("dd_MM_yyyy")}_{to.ToString("dd_MM_yyyy")}");
			dt.Columns.Add("Белый домен", stringType);
			dt.Columns.Add("Черный домен", stringType);
			dt.Columns.Add("IP", stringType);
			dt.Columns.Add("Дата обнаружения", stringType);
			dt.Columns.Add("Дата окончания", stringType);

			foreach (var record in models)
			{
				foreach (var ip in record.Attacks)
				{
					dt.Rows.Add(ip.WhiteDomain, ip.BlackDomain, ip.Ip, record.DateBegin, record.DateClose);
				}
			}
			var ws = wb.Worksheets.Add(dt);
			using (var ms = new MemoryStream())
			{
				wb.SaveAs(ms);
				return ms.ToArray();
			}
		}

		public byte[] ExportWhiteList(IEnumerable<WhiteDomains> records)
		{
			var wb = new XLWorkbook();

			var stringType = typeof(string);
			var dt = new DataTable("Белый список");
			dt.Columns.Add("ID", stringType);
			dt.Columns.Add("Домен", stringType);
			dt.Columns.Add("Дата добавления", typeof(DateTime));

			foreach (var record in records)
			{
				dt.Rows.Add(record.Id, record.Domain, record.DateAdded.LocalDateTime);
			}
			var ws = wb.Worksheets.Add(dt);
			using (var ms = new MemoryStream())
			{
				wb.SaveAs(ms);
				return ms.ToArray();
			}
		}
	}
}