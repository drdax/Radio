using System;
using System.Globalization;
using System.Threading.Tasks;
using DrDax.RadioClient;

namespace Pieci {
	public class PieciGuide : TimedGuide {
		/// <summary>Pieci.lv JSONa datuma/laika formāts.</summary>
		public const string TimeFormat="yyyy-MM-dd HH:mm:ss";
		/// <summary>HTTP klients nākošās dziesmas noskaidrošanai.</summary>
		private readonly ProperWebClient client=new ProperWebClient();
		/// <summary>Pašreizējās dziesmas informācijas adrese.</summary>
		private readonly string currentUrl;
		/// <summary>Nākošās dziesmas informācijas adrese.</summary>
		private readonly string nextUrl;
		/// <summary>Laikazona raidījumu pārrēķinam.</summary>
		private readonly TimeZoneInfo timezone;
		/// <summary>FM kanāla dienas programma.</summary>
		private readonly PieciListedGuide listedGuide;

		internal PieciGuide(uint number, TimeZoneInfo timezone) : base(null) {
			currentUrl=string.Format("http://www.pieci.lv/shared/cache/current_st{0}.json", number);
			nextUrl=string.Format("http://www.pieci.lv/shared/cache/next_st{0}.json", number);
			this.timezone=timezone;
			listedGuide=number == 19 ? new PieciListedGuide(timezone):null;
		}

		public override async Task Start(bool initialize) {
			if (listedGuide != null) {
				await listedGuide.Start(initialize);
				PreviousBroadcast=listedGuide.PreviousBroadcast;
			}
			await base.Start(initialize);
		}
		protected override async Task UpdateBroadcasts() {
			Broadcast current=await GetBroadcast(true);
			if (CurrentBroadcast == null || CurrentBroadcast.Caption != current.Caption || CurrentBroadcast.Description != current.Description)
				PreviousBroadcast=CurrentBroadcast;
			CurrentBroadcast=current;
			NextBroadcast=await GetBroadcast(false); // Nākošā dziesma var nebūt un var arī būt aiznākošā.
		}
		private async Task<Broadcast> GetBroadcast(bool isCurrent) {
			// images var būt null
			// [{"id":"988879","artist":"Jackson 5","title":"Frosty the Snowman","runtime":"153.329","airtime":"2013-12-24 14:28:29","status":"cued","song_id":"8019","artist_id":"1133","station_id":"9","images":{"android":"http:\/\/cdn.pieci.lv\/images\/01-1-android.jpg","metadata":{"android":null,"ios":null,"desktop":null},"ios":"http:\/\/cdn.pieci.lv\/images\/01-1-ios.jpg","desktop":"http:\/\/cdn.pieci.lv\/images\/01-1-desktop.jpg"}}]
			var song=(await client.GetJson(isCurrent ? currentUrl:nextUrl)).Element("item");
			if (song == null) return listedGuide != null ? (isCurrent ? listedGuide.CurrentBroadcast:listedGuide.NextBroadcast):null; // Šis uz ilgu laiku aizstāj raidījumu, pat ja kļūst pieejama informācija par pašreizējo dziesmu.
			else {
				DateTime startTime=TimeZoneInfo.ConvertTime(DateTime.ParseExact(song.Element("airtime").Value, TimeFormat, CultureInfo.InvariantCulture), timezone, TimeZoneInfo.Local);
				DateTime endTime=startTime.AddSeconds(double.Parse(song.Element("runtime").Value, CultureInfo.InvariantCulture));
				string title=song.Element("title").Value; int splitIdx=title.IndexOf('('); // Iekavās mēdz būt detaļas, piemēram, koncerta nosaukums.
				Broadcast listBroadcast=null; // Var būt null arī, ja raidījumu saraksts par īsu.
				if (listedGuide != null) listBroadcast=isCurrent ? listedGuide.CurrentBroadcast:listedGuide.NextBroadcast;
				return new Broadcast(startTime, endTime < DateTime.Now ? DateTime.Now.AddSeconds(5):endTime,
					splitIdx == -1 ? title:(splitIdx == 0 ? title.Substring(1, title.Length-2):title.Substring(0, splitIdx-1)),
					string.Concat(song.Element("artist").Value,
						splitIdx > 0 ? Environment.NewLine+title.Substring(splitIdx+1, title.Length-splitIdx-2):null,
						listBroadcast != null ? Environment.NewLine+listBroadcast.Caption:null));
			}
		}
	}
}