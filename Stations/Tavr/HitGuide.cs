using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using DrDax.RadioClient;

namespace Tavr {
	class HitGuide : IcyGuide {
		/// <summary>HTTP klients raidījma datu izgūšanai.</summary>
		private readonly ProperWebClient client=new ProperWebClient();
		private readonly TimeZoneInfo timezone;
		/// <summary>Radījums, kuru attēlo, kad nav nosaukta dziesma.</summary>
		private HitBroadcast broadcast;

		public HitGuide(TimeZoneInfo timezone) : base(Encoding.UTF8, new HitGuideMenu()) {
			this.timezone=timezone;
		}
		public override async Task Start(bool initialize) {
			CurrentBroadcast=await GetBroadcast(null, null); // Iestata kā pašreizējo, bet tas drīz kļūst par iepriekšējo.
			await SetDefaultBroadcast();
			await base.Start(initialize);
		}
		private async Task SetDefaultBroadcast() {
			if (broadcast == null || broadcast.EndTime < DateTime.Now) {
				string html=await client.DownloadStringTaskAsync("http://www.hitfm.ua/");
				var match=Regex.Match(html, @"style=""margin-right: 12px;width: 120px;"">\s+<a class=""bold"" href=""(?'url'[^""]+)"">(?'caption'[^<]+)<\/a>\s+<div>(?'description'[^<]*)<\/div>\s+<div class=""bold"">(?'start'[012][0-9]:[0-5][0-9]) - (?'end'[012][0-9]:[0-5][0-9])");
				if (match.Success) {
					DateTime today=TimeZoneInfo.ConvertTime(DateTimeOffset.Now, timezone).Date;
					broadcast=new HitBroadcast(TimeZoneInfo.ConvertTime(today.Add(TimeSpan.Parse(match.Groups["start"].Value)), timezone, TimeZoneInfo.Local),
						TimeZoneInfo.ConvertTime(today.Add(TimeSpan.Parse(match.Groups["end"].Value)), timezone, TimeZoneInfo.Local),
						match.Groups["caption"].Value, match.Groups["description"].Value, match.Groups["url"].Value);
				} else {
					DateTime now=DateTime.Now;
					broadcast=new HitBroadcast(now.Date.AddHours(now.Hour), now.Date.AddHours(now.Hour+1), "Хiт FM", null, null);
				}
			}
		}
		protected override async Task<Broadcast> GetBroadcast(string title) {
			if (title == null || title == "Xiт FM - www.hitfm.ua") {
				await SetDefaultBroadcast();
				return broadcast;
			}
			string description;
			return await GetBroadcast(title.SplitCaption(out description), description);
		}
		/// <returns>
		/// Pirmais (ja ir norādīts <paramref name="caption"/>) vai otrais (ja nav norādīts) raidījums no pēdējo dziesmu saraksta.
		/// </returns>
		private async Task<Broadcast> GetBroadcast(string caption, string description) {
			XElement item; string url=null;
			try {
				item=(await client.GetJson("http://www.hitfm.ua/ajax/songsonair/")).Elements("item").ElementAt(caption == null ? 1:0);
				if (item.Element("id").Value != "false") {
					if (item.Element("songId").Value == "false")
						url=string.Format("http://www.hitfm.ua/music/{0}-{1}/songs.html",
							item.Element("id").Value, item.Element("singerT").Value);
					else url=string.Format("http://www.hitfm.ua/music/{0}-{1}/song/{2}-{3}.html",
						item.Element("id").Value, item.Element("singerT").Value, item.Element("songId").Value, item.Element("songT").Value);
				}
				if (caption == null) {
					caption=item.Element("song").Value; description=item.Element("singer").Value;
				}
				System.Diagnostics.Debug.WriteLine(url);
			} catch { return null; }
			// [{"1":"Corona","2":"Rhythm Of The Night","3":"16:21","id":"60","singer":"Corona","singerT":"corona","songT":"rhythm-of-the-night","song":"Rhythm Of The Night","songId":"865","songPath":"C\/Corona - Rhythm Of The Night.mp3","perevod":1},{"1":"Maroon 5","2":"This Love","3":"16:17","id":"47","singer":"Maroon 5","singerT":"maroon-5","songT":"this-love","song":"This Love","songId":"464","songPath":"M\/Maroon 5 - This Love.mp3","perevod":1},{"1":"Mylene Farmer & Seal","2":"Les Mots","3":"16:14","id":false,"singer":"Mylene Farmer & Seal","singerT":"mylene-farmer-and-seal","songT":"les-mots","song":"Les Mots","songId":false,"songPath":"M\/Mylene Farmer %26 Seal - Les Mots.mp3","perevod":0},{"1":"\u041b\u0438\u0442\u0432\u0438\u043d\u0435\u043d\u043a\u043e \u041e\u043b\u044c\u0433\u0430","2":"\u041d\u0438\u0447\u0435\u0433\u043e \u043d\u0435 \u0431\u043e\u0438\u0441\u044f","3":"16:10","id":false,"singer":"\u041b\u0438\u0442\u0432\u0438\u043d\u0435\u043d\u043a\u043e \u041e\u043b\u044c\u0433\u0430","singerT":"litvinenko-olga","songT":"nichego-ne-boisia","song":"\u041d\u0438\u0447\u0435\u0433\u043e \u043d\u0435 \u0431\u043e\u0438\u0441\u044f","songId":false,"songPath":false,"perevod":0}]
			return new HitBroadcast(// Ir zināms sākumlaiks, bet nav ilgums.
				TimeZoneInfo.ConvertTime(TimeZoneInfo.ConvertTime(DateTimeOffset.Now, timezone).Date.Add(TimeSpan.Parse(item.Elements(XName.Get("item", "item")).ElementAt(2).Value)), timezone, TimeZoneInfo.Local), // hh:mm
				DateTime.Now.AddMilliseconds(Channel.DefaultTimeout), caption, description, url);
		}
	}
}