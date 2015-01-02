using System;

namespace DrDax.RadioClient {
	public class PagedBroadcast : Broadcast {
		/// <summary>Pilna vai daļēja raidījuma lappuses adrese.</summary>
		public string PageUrl { get { return pageUrl; } }

		public PagedBroadcast(DateTime startTime, DateTime endTime, string caption, string description, string pageUrl)
			: base(startTime, endTime, caption, description) {
			this.pageUrl=pageUrl;
		}

		private readonly string pageUrl;
	}
}