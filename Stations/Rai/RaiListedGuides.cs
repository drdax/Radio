using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DrDax.RadioClient;

namespace Rai {
	/// <summary>Parastā Rai kanāla raidījumu saraksts, kurš zināms visai dienai.</summary>
	public class RaiListedGuide : PagedListedGuide {
		/// <summary>Raidījuma dati ar precizitāti līdz minūtei un raidījuma aprakstu.</summary>
		private static readonly Regex broadcastRx=new Regex(@"ora"">(?'time'[012][0-9]:[0-5][0-9])<\/span> <span class=""info""><a (href=""(?'url'[^""]+))?[\s\S]+?MediapolisCategory="""">(?'caption'[^<]+)<\/[\s\S]+?""eventDescription"">(?'description'[\s\S]+?)<span class=""solotesto"">", RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.ExplicitCapture);
		private readonly string radioName;

		internal RaiListedGuide(string radioName, TimeZoneInfo timezone) : base(timezone, new SimpleGuideMenu("Sul programma")) {
			this.radioName=radioName;
		}

		protected override async Task FillGuide(DateTime date) {
			TimeSpan lastTime=new TimeSpan(0); // Parasti diena sākas sešos.
			foreach (Match match in broadcastRx.Matches(await client.DownloadStringTaskAsync(
				string.Format("http://rai.it/dl/portale/html/palinsesti/guidatv/static/{0}_{1:yyyy_MM_dd}.html", radioName, date)))) {
				TimeSpan time=TimeSpan.Parse(match.Groups["time"].Value); // HH:mm
				if (time < lastTime) date=date.AddDays(1);
				lastTime=time;
				AddBroadcast(date.Add(time),
					match.Groups["caption"].Value.Trim().ToCapitalized(),
					match.Groups["description"].Value.Trim().ToCapitalized(), // Apraksts mēdz būt tukšs.
					match.Groups["url"].Success ? match.Groups["url"].Value:null);
			}
		}
	}
	/// <summary>Web sērijas Rai kanāla raidījumu saraksts, kurš zināms visai dienai.</summary>
	public class RaiWebListedGuide : DescriptionListedGuide {
		/// <summary>Raidījuma dati ar precizitāti līdz sekundei un zināmu izpildītāju.</summary>
		private static readonly Regex broadcastRx=new Regex(@"ora"">(?'time'[012][0-9]:[0-5][0-9]:[0-5][0-9])</span> <span class=""info""><[^>]+MediapolisCategory="""">(?'caption'[^<]+)</a></span>\n<div data-autore=""(Wr[678]|(?'author'[^""]+)|)""[^>]+>\n?(?'description1'[^<]+)?<span class=""solotesto"">(?'description2'[^<]+)?<", RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.ExplicitCapture);
		private readonly string radioName;

		internal RaiWebListedGuide(string radioName, TimeZoneInfo timezone) : base(timezone, null) {
			this.radioName=radioName;
		}

		protected override async Task FillGuide(DateTime date) {
			TimeSpan lastTime=new TimeSpan(0); // Parasti diena sākas sešos.
			StringBuilder sb=new StringBuilder(255);
			foreach (Match match in broadcastRx.Matches(await client.DownloadStringTaskAsync(
				string.Format("http://rai.it/dl/portale/html/palinsesti/guidatv/static/{0}_{1:yyyy_MM_dd}.html", radioName, date)))) {
				TimeSpan time=TimeSpan.Parse(match.Groups["time"].Value); // HH:mm:ss
				if (time < lastTime) date=date.AddDays(1);
				lastTime=time;
				sb.Clear();
				if (match.Groups["author"].Success) sb.Append("Autore: "+match.Groups["author"].Value.ToCapitalized());
				if (match.Groups["description1"].Success) {
					if (sb.Length != 0) sb.Append(Environment.NewLine);
					sb.Append(match.Groups["description1"].Value);
				}
				if (match.Groups["description2"].Success) {
					if (sb.Length != 0) sb.Append(Environment.NewLine);
					sb.Append(match.Groups["description2"].Value.ToCapitalized());
				}
				AddBroadcast(date.Add(time), match.Groups["caption"].Value.ToCapitalized(), sb.ToString());
			}
		}
	}
}