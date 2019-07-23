using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Dns.DAL.DataMigration
{
	public static class MigrationHelper
	{
		public static void TruncateTables(this DnsDbContext context, params string[] tableNames)
		{
			var query = $@"TRUNCATE ""{string.Join(@""", """, tableNames)}"" RESTART IDENTITY CASCADE;";
			context.Database.ExecuteSqlRaw(query);
			Console.WriteLine($"Tables {string.Join(", ", tableNames)} truncated");
		}
		public static void DisableTriggers(this DnsDbContext context, params string[] tableNames)
		{
			foreach (var table in tableNames)
			{
				context.Database.ExecuteSqlRaw($@"ALTER TABLE ""{table}"" DISABLE TRIGGER ALL;");
				Console.WriteLine($"Triggers disabled for table {table}");
			}
		}
		public static void EnableTriggers(this DnsDbContext context, params string[] tableNames)
		{
			foreach (var table in tableNames)
			{
				context.Database.ExecuteSqlRaw($@"ALTER TABLE ""{table}"" ENABLE TRIGGER ALL;");
				Console.WriteLine($"Triggers enabled for table {table}");
			}
		}
		public static void SetCurrentSquenceId(this DnsDbContext context, string tableName, int value)
		{
			var query = $@"SELECT setval('""{tableName}_Id_seq""', {value})";
			context.Database.ExecuteSqlRaw(query);
			Console.WriteLine($"Setted sequnce value {value} to table {tableName}");
		}
		public static void MigrateTable<T, O>(
			this DnsDbContext context,
			string tableName,
			DbSet<T> sourceDbSet, DbSet<O> destDbSet, Func<T, O> transformFunc,
			bool hasSequence = true, Func<int> maxId = null)
			where T : class where O : class
		{
			foreach (var item in sourceDbSet)
			{
				destDbSet.Add(transformFunc(item));
			}
			if (hasSequence && sourceDbSet.Any())
			{
				context.SetCurrentSquenceId(tableName, maxId());
			}
			context.SaveChanges();
			Console.WriteLine($"Table {tableName} migrated");
		}
	}
}
