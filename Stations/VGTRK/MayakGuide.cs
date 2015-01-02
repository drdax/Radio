using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using DrDax.RadioClient;

namespace Vgtrk {
	public class MayakGuide : DescriptionListedGuide {
		private readonly Regex idRx=new Regex("/id/([0-9]+)/", RegexOptions.Compiled);
		public MayakGuide(TimeZoneInfo timezone) : base(timezone, null) { }

		protected override async Task FillGuide(DateTime date) {
			// Nolasīto bloku saturs. Viens [bloka] raidījums var būt vairākas reizes dienā, bet tam atbilst viena bloka lappuse.
			var episodes=new Dictionary<int, Episode>(12);
			bool previousIsPast=true; // Vai iepriekšējais bloks ir pagātnē. Lieto pašreizējā bloka noteikšanai (jo tas nekā neizceļās).
			string episodeCaption=null; // Pašreizējā bloka nosaukums.
			var sb=new StringBuilder(200);

			foreach (var xEpisode in await GetFragments("http://radiomayak.ru/schedule/index/date/"+date.ToString("dd-MM-yyyy"))) {
				string className=xEpisode.Attribute("class").Value;
				if (className == "b-schedule__list-sub-list") {
					// Dienas programmā pašreizējais bloks ir izvērsts, paņem no tā visus raidījumus.
					foreach (var fragment in xEpisode.Elements("div")) {
						var data=fragment.Element("div").Elements("div");
						AddBroadcast(date.Add(TimeSpan.Parse(data.ElementAt(0).Value.Substring(0, 5))), // hh:mm
							GetCaption(data, episodeCaption), GetDescription(data, sb));
					}
				} else {
					var data=xEpisode.Element("div").Elements("div");
					var link=data.ElementAt(1).Element("h5").Element("a");
					if (previousIsPast && !className.EndsWith("past-show")) {
						// Tā kā pirmais bloka raidījums sākas kopā ar bloku, iegaumē tikai tā nosaukumu.
						episodeCaption=link.Value;
					} else {
						if (episodeCaption == null) {
							// Raidījumiem pirms pašreizējā paņem tikai bloka laiku un nosaukumu.
							AddBroadcast(date.Add(TimeSpan.Parse(data.ElementAt(0).Value)), // hh:mm
								link.Value, null);
						} else {
							episodeCaption=link.Value;
							string episodeTime=data.ElementAt(0).Value; // hh:mm

							int id=int.Parse(idRx.Match(link.Attribute("href").Value).Groups[1].Value);
							Episode episode;
							if (!episodes.TryGetValue(id, out episode)) {
								// Nākošajiem raidījumiem bloka saturu izgūst no atsevišķas lappuses.
								episode=new Episode(await GetFragments("http://radiomayak.ru"+link.Attribute("href").Value));
								episodes.Add(id, episode);
							}

							bool inBlock=false; // Vai pašreizējie bloka raidījumi atbilst atlasāmajam laikam.
							for (int n=episode.StartIdx; n < episode.FragmentsCount; n++) {
								var fragment=episode.Fragments.ElementAt(n);
								data=fragment.Element("div").Elements("div");
								if (data.Count() == 2) { // Starp blokiem ir atdalītāji, kurus veido mazāk div elementu.
									if (inBlock) { episode.StartIdx=n+1; break; }
									continue;
								}
								string fragmentTime=data.ElementAt(0).Value.Substring(0, 5); // hh:mm (Substring nogriež atdalītāju un beigu laiku)
								if (episodeTime == fragmentTime) inBlock=true;
								if (inBlock)
									AddBroadcast(date.Add(TimeSpan.Parse(fragmentTime)), GetCaption(data, episodeCaption), GetDescription(data, sb));
							}
						}
					}
					previousIsPast=className.EndsWith("past-show");
				}
			}
		}
		private async Task<IEnumerable<XElement>> GetFragments(string url) {
			string html=await client.DownloadStringTaskAsync(url);
			// Dokuments kopumā ir pēc HTML nevis XML prasībām, bet raidījumu saraksts ir formatēts strikti un ar atstatumiem, tāpēc to var apstrādāt kā struktūru.
			int startIdx=html.IndexOf("<div class=\"b-schedule__list\">");
			return XDocument.Parse(html.Substring(startIdx, html.IndexOf("\n    </div>", startIdx)-startIdx+11)).Root.Elements("div");
		}
		private string GetCaption(IEnumerable<XElement> data, string episodeCaption) {
			return string.Concat(Quotes.Format(data.ElementAt(1).Element("h5").Elements().ElementAt(0).Value), " (", episodeCaption, ")");
		}
		private string GetDescription(IEnumerable<XElement> data, StringBuilder sb) {
			var info=data.ElementAt(1).Elements();
			sb.Clear();
			string value=info.ElementAt(1).Value;
			if (value.Length != 0) sb.Append(Quotes.Format(value));
			if (info.Count() > 2) {
				if (sb.Length != 0) sb.AppendLine();
				sb.Append("В гостях ").Append(string.Join(", ", from a in info.ElementAt(2).Elements("a") select a.Value));
			}
			return sb.Length == 0 ? null:sb.ToString();
		}

		private class Episode {
			/// <summary>Visi bloka raidījumi un atdalītāji.</summary>
			public readonly IEnumerable<XElement> Fragments;
			public readonly int FragmentsCount;
			/// <summary>
			/// Nākamās raidījuma meklēšanas sākuma indekss <see cref="Fragments"/> kolekcijā.
			/// </summary>
			public int StartIdx=0;

			public Episode(IEnumerable<XElement> fragments) {
				this.Fragments=fragments;
				this.FragmentsCount=fragments.Count();
			}
		}
	}
}