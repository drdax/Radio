using System;
using DrDax.RadioClient;
using System.Text.RegularExpressions;
using System.Text;

namespace Eurovision {
	public class EurovisionGuide : IcyGuide {
		private static readonly Regex titleRx=new Regex(@"^(?'artist'.+) - (?'caption'.+?) \((?'description'.+)\)$", RegexOptions.Compiled);

		public EurovisionGuide() : base(Encoding.GetEncoding("ISO-8859-1")) {}
		protected override Broadcast GetBroadcast(string title) {
			if (title == null) return null;
			Match match=titleRx.Match(title);
			return new Broadcast(DateTime.Now, DateTime.Now.AddHours(1),
				match.Groups["caption"].Value,
				string.Concat(match.Groups["artist"].Value, Environment.NewLine, match.Groups["description"].Value.Replace(") (", ", ")));
		}
	}
}