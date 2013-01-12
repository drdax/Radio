﻿using System;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Xml;
using System.Xml.Linq;
using DrDax.RadioClient;

namespace Retro {
	/// <summary>Maskavas Ретро FM skanošās dziesmas.</summary>
	public class RetroRuGuide : TimedGuide {
		private readonly ProperWebClient client=new ProperWebClient(System.Text.Encoding.ASCII);
		// http://en.wikipedia.org/wiki/Unix_time
		private static readonly DateTime unixEpoch=new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		public RetroRuGuide() { StartTimer(); }

		protected override void UpdateBroadcasts() {
			// Alternatīva bez ilgumiem ir http://retrofm.ru/online/air/song.js
			// { "song": "WHAT A FEELING", "artist": "CARA IRENE" }
			XElement json=XElement.Load(JsonReaderWriterFactory.CreateJsonReader(
					client.DownloadData("http://retrofm.ru/online/air/playlist.js"),
					XmlDictionaryReaderQuotas.Max));
			/* Pēdējās trīs skanējušās dziesmas. Vienmēr latiņu burtiem.
			   [ { "id": "5693", "name": "ALWAYS ON MY MIND", "artist": "BOYS PET SHOP", "start": "1357163566", "duration": "218" },
			   { "id": "5691", "name": "ROZOVY VECHER+NY", "artist": "LASKOVY MAY SHATUNOV YURY", "start": "1357163391", "duration": "164" },
			   { "id": "5689", "name": "LE CAFE DES 3 COLOMBES", "artist": "DASSIN JOE", "start": "1357163176", "duration": "208" } ]
<root type="array">
  <item type="object">
    <id type="string">5703</id>
    <name type="string">TAKE A CHANCE ON ME</name>
    <artist type="string">ABBA</artist>
    <start type="string">1357164716</start>
    <duration type="string">222</duration>
  </item>
</root> */
			System.Diagnostics.Debug.WriteLine(json.ToString());
			PreviousBroadcast=CurrentBroadcast == null ? GetBroadcast(json.Elements("item").ElementAt(1)):CurrentBroadcast;
			CurrentBroadcast=GetBroadcast(json.Element("item"));
			DateTime now=DateTime.Now;
			// Mēdz pašreizējā dziesma noskanēt, bet dati par jauno neparādīties,
			// tāpēc uzgaidam 30 sekundes, kas atbilst oriģinālā Ретро FM uztvērēja pārbaudes laikam.
			if (CurrentBroadcast.EndTime < now) {
				PreviousBroadcast=CurrentBroadcast;
				CurrentBroadcast=new Broadcast(now, now.AddSeconds(30), "Ретро FM"); // Oriģinālā teksts "Слушайте Ретро FM!", kuru liek song.js.
			}
		}
		private Broadcast GetBroadcast(XElement item) {
			// Dziesmas sākumlaiks UTC laika joslā.
			DateTime start=GetLocalTime(item.Element("start").Value);
			return new Broadcast(start, start.AddSeconds(short.Parse(item.Element("duration").Value)),
				item.Element("name").Value.ToCapitalized(), item.Element("artist").Value.ToCapitalized());
		}
		public static DateTime GetLocalTime(string timestamp) {
			return TimeZoneInfo.ConvertTime(unixEpoch.AddSeconds(long.Parse(timestamp)), TimeZoneInfo.Local);
		}
	}
}