using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dns.Site.Services;

namespace Dns.Site.VigruzkiService
{
	public partial class Client : IVigruzkiService
	{
		public Task<ICollection<VigruzkiRecordModel>> SearchContainsAsync(string query)
		{
			return ContainAsync(query);
		}

		public Task<ICollection<VigruzkiRecordModel>> SearchEqualAsync(string query)
		{
			return EqualAsync(query);
		}
	}
}
