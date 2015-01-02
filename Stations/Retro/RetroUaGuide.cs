using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using DrDax.RadioClient;

namespace Retro {
	/// <summary>Kijevas Ретро FM skanošās dziesmas.</summary>
	public class RetroUaGuide : RetroGuide {
		public RetroUaGuide(string genre) {
			this.genre=genre;
		}
		protected override async Task UpdateBroadcasts() {
			XElement json=await client.GetJson("http://retro.ua/on_air/onair.json");
			XElement item;
			if (CurrentBroadcast == null) {
				item=json.Element("playlists").Element(genre).Elements("item").ElementAt(1);
				DateTime start=GetLocalTime(item.Element("start_ts").Value);
				PreviousBroadcast=new Broadcast(start, start.AddSeconds(short.Parse(item.Element("duration").Value)),
					item.Element("song").Value.ToCapitalized(), item.Element("artists").Elements("item").ElementAt(0).Element("name").Value.ToCapitalized());
			} else if (CurrentBroadcast.Caption != StubCaption) PreviousBroadcast=CurrentBroadcast;
			item=json.Element("playlists").Element(genre).Elements("item").ElementAt(0);
			DateTime now=DateTime.Now;
			if (item == null || SetCurrentBroadcast(new Broadcast(
				GetLocalTime(item.Element("start_ts").Value), GetLocalTime(item.Element("stop_ts").Value),
				item.Element("song").Value.ToCapitalized(), item.Element("artists").Elements("item").ElementAt(0).Element("name").Value.ToCapitalized()), now)) {
				// Ukrainas uztvērējs pārbauda raidījumus ik 5 sekundes reizinātas ar gadījuma skaitli, bet šeit aizkave 30 sekundes tāpat kā Maskavas kanālam.
				CurrentBroadcast=new Broadcast(now, now.AddSeconds(30), StubCaption);
			}
		}
		// Saprotamais laiks ir Kijevas laika joslā un lai to lieku reizi nepārveidotu, tiek lietots Unix laiks.
		//private DateTime ParseTime(string time) { return DateTime.ParseExact(time, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture); }
		private readonly string genre;
	}
}