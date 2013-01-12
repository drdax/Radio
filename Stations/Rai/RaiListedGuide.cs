using System;
using System.Text.RegularExpressions;
using DrDax.RadioClient;

namespace Rai {
	/// <summary>Rai kanāla raidījumu saraksts, kurš zināms visai dienai.</summary>
	public class RaiListedGuide : DescriptionListedGuide {
		/// <summary>Raidījuma dati ar precizitāti līdz minūtei un raidījuma aprakstu.</summary>
		private static readonly Regex minutesRx=new Regex("ora\">(?'time'[012][0-9]:[0-5][0-9])<\\/[\\s\\S]+?MediapolisCategory=\"\">(?'caption'[^<]+)<\\/[\\s\\S]+?\"eventDescription\">(?'description'[\\s\\S]+?)<span class=\"solotesto\">", RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.ExplicitCapture);
		/// <summary>Raidījuma dati ar precizitāti līdz sekundei un zināmu izpildītāju.</summary>
		private static readonly Regex secondsRx=new Regex("ora\">(?'time'[012][0-9]:[0-5][0-9]:[0-5][0-9])<\\/[\\s\\S]+?MediapolisCategory=\"\">(?'caption'[^<]+)<\\/[\\s\\S]+?tore: <\\/span><strong>(?'description'[^<]+)<\\/", RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.ExplicitCapture);
		private readonly string radioName;
		private readonly Regex broadcastRx;

		internal RaiListedGuide(bool isWebRadio, string radioName, TimeZoneInfo timezone) : base(timezone) {
			this.radioName=radioName;
			broadcastRx=isWebRadio ? secondsRx:minutesRx;
			Initialize();
		}

		protected override void FillGuide(DateTime date) {
			TimeSpan lastTime=new TimeSpan(0); // Parasti diena sākas sešos.
			using (var client=new ProperWebClient(System.Text.Encoding.ASCII)) {
				foreach (Match match in broadcastRx.Matches(client.DownloadString(
					string.Format("http://rai.it/dl/portale/html/palinsesti/guidatv/static/{0}_{1:yyyy_MM_dd}.html", radioName, date)))) {
					TimeSpan time=TimeSpan.Parse(match.Groups["time"].Value); // HH:mm vai HH:mm:ss
					if (time < lastTime) date=date.AddDays(1);
					lastTime=time;
					AddBroadcast(date.Add(time),
						match.Groups["caption"].Value.Trim().ToCapitalized(),
						match.Groups["description"].Value.Trim().ToCapitalized()); // Apraksts mēdz būt tukšs.
				}
			}
		}
	}
}