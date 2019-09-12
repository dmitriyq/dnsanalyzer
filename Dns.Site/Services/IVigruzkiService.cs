using System.Collections.Generic;
using System.Threading.Tasks;
using Dns.Site.VigruzkiService;

namespace Dns.Site.Services
{
	public interface IVigruzkiService
	{
		Task<ICollection<VigruzkiRecordModel>> SearchEqualAsync(string query);

		Task<ICollection<VigruzkiRecordModel>> SearchContainsAsync(string query);
	}
}