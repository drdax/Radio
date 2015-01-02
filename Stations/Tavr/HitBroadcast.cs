using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DrDax.RadioClient;

namespace Tavr {
	public class HitBroadcast : Broadcast {
		public readonly string PageUrl;
		public HitBroadcast(DateTime startTime, DateTime endTime, string caption, string description, string pageUrl)
			: base(startTime, endTime, caption, description) {
			PageUrl=pageUrl;
		}
	}
}