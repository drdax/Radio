using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DrDax.RadioClient;

namespace Super {
	/// <summary>
	/// Eiropas hītu radio un Super FM raidījumu informācija no ICE plūsmas.
	/// </summary>
	public class EhrGuide : IcyGuide {
		private static readonly Regex
			titleRx=new Regex(@"^(Nr\. (?'position'[0-9]+) )?-ARTIST- ?(?'artist'.+) ?-TITLE- ?(?'title'.+?)(?'first' ?-FIRST ON AIR-(?'month'[01][0-9])\.(?'year'[0-9]{4}))?$", RegexOptions.Compiled),
			namesRx=new Regex("^Varda dienu svin- (.+)", RegexOptions.Compiled);

		protected override Task<Broadcast> GetBroadcast(string title) {
			if (string.IsNullOrEmpty(title)) return NullTaskBroadcast;
			string caption, description;
			Match match=titleRx.Match(title);
			if (match.Success) {
				string month=null;
				switch (match.Groups["month"].Value) {
					case "01": month="janvāra"; break;
					case "02": month="februāra"; break;
					case "03": month="marta"; break;
					case "04": month="aprīļa"; break;
					case "05": month="maija"; break;
					case "06": month="jūnija"; break;
					case "07": month="jūlija"; break;
					case "08": month="augusta"; break;
					case "09": month="septembra"; break;
					case "10": month="oktobra"; break;
					case "11": month="novembra"; break;
					case "12": month="decembra"; break;
				}
				caption=match.Groups["title"].Value.ToCapitalized();
				if (match.Groups["first"].Success) // European Hit radio ir zināms pirmās atskaņošanas mēnesis,...
					description=string.Format("{0}{1}{2}Kopš {3}. gada {4}", match.Groups["artist"].Value.ToCapitalized(),
						match.Groups["position"].Success ? string.Format("{0}{1}. vieta", Environment.NewLine, match.Groups["position"].Value):null,
						Environment.NewLine, match.Groups["year"].Value, month);
				else description=match.Groups["artist"].Value.ToCapitalized(); // ... bet Super FM tikai izpildītājs.
			} else { // Vārdadienas svinētāji ir abās raidstacijās.
				match=namesRx.Match(title);
				if (match.Success) {
					caption=match.Groups[1].Value.Trim();
					description="vārdadienas svinētāji";
				}
				else return NullTaskBroadcast;
			}
			return Task.FromResult(new Broadcast(DateTime.Now, DateTime.Now.AddHours(1), caption, description));
		}
	}
}