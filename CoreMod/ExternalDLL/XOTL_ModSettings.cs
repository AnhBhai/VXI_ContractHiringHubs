using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VXIContractHiringHubs
{
	public class XOTL_ModSettings
	{
		public Dictionary<string, string> UnitToFactionCollection = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		public Dictionary<string, string> UnitToFactionVeeCollection = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
	}
}
