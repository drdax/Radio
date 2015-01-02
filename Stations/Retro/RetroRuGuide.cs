using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using DrDax.RadioClient;

namespace Retro {
	/// <summary>Maskavas Ретро FM skanošās dziesmas.</summary>
	public class RetroRuGuide : RetroGuide {
		protected override async Task UpdateBroadcasts() {
			// Alternatīva bez ilgumiem ir http://retrofm.ru/online/air/song.js
			// { "song": "WHAT A FEELING", "artist": "CARA IRENE" }
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
			DateTime now=DateTime.Now;
			XElement json=await client.GetJson("http://retrofm.ru/online/air/playlist.js");
			if (CurrentBroadcast == null)
				PreviousBroadcast=GetBroadcast(json.Elements("item").ElementAt(1));
			else if (CurrentBroadcast.Caption != StubCaption) PreviousBroadcast=CurrentBroadcast;
			if (SetCurrentBroadcast(GetBroadcast(json.Element("item")), now))
				CurrentBroadcast=new Broadcast(now, now.AddSeconds(30), StubCaption);
		}
		private Broadcast GetBroadcast(XElement item) {
			// Dziesmas sākumlaiks UTC laika joslā.
			DateTime start=GetLocalTime(item.Element("start").Value);
			return new Broadcast(start, start.AddSeconds(short.Parse(item.Element("duration").Value)),
				item.Element("name").Value.ToCapitalized(), item.Element("artist").Value.ToCapitalized());
		}
	}
}