using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Dns.DAL.Enums
{
	public enum AttackGroupStatusEnum
	{
		/// <summary>
		/// Пустое
		/// </summary>
		[Display(Name = "")]
		None = 0,

		/// <summary>
		/// Ожидание проверки
		/// </summary>
		[Display(Name = "Ожидание проверки")]
		PendingCheck = 1,

		/// <summary>
		/// Атака
		/// </summary>
		[Display(Name = "Атака")]
		Attack = 2,

		/// <summary>
		/// Угроза
		/// </summary>
		[Display(Name = "Угроза")]
		Threat = 3,

		/// <summary>
		/// Динамические IP
		/// </summary>
		[Display(Name = "Динамические IP")]
		Dynamic = 4,

		/// <summary>
		/// Прекращена
		/// </summary>
		[Display(Name = "Прекращена")]
		Complete = 5,
	}
}
