using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Dns.DAL.DataMigration.MSSQL.Enums
{
	public enum AttackStatusEnum
	{
		/// <summary>
		/// Пустое
		/// </summary>
		[Display(Name = "")]
		None = 0,

		/// <summary>
		/// Пересечение
		/// </summary>
		[Display(Name = "Пересечение")]
		Intersection = 1,

		/// <summary>
		/// Прекращено
		/// </summary>
		[Display(Name = "Прекращено")]
		Completed = 3,

		/// <summary>
		/// Ожидание закрытия
		/// </summary>
		[Display(Name = "Ожидание закрытия")]
		Closing = 4,
	}
}
