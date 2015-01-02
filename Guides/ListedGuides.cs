using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DrDax.RadioClient {
	/// <summary>Raidījumu programma, kura sastāv no zināma diennakts raidījumu saraksta.</summary>
	/// <typeparam name="TBroadcastData">Informācija par raidījumu.</typeparam>
	public abstract class ListedGuide<TBroadcastData> : TimedGuide {
		private enum ShiftType {
			/// <summary>Diena sākas un beidzas pusnaktī.</summary>
			None,
			/// <summary>Diena sākas pēc pusnakts, bet beidzās tajā pašā datumā.</summary>
			Start,
			/// <summary>Diena sāks pēc pusnakts un beidzās nākamajā datumā.</summary>
			StartEnd
		}
		/// <summary>Pašreiz skanošā raidījuma kārtas numurs kopā <see cref="guide"/>.</summary>
		private int currentIdx;
		/// <summary>
		/// Vai <see cref="UpdateBroadcasts"/> izsaukumā jāapnullē <see cref="currentIdx"/>.
		/// </summary>
		private bool resetIndex=false;
		/// <summary>Vai pirmais raidījumu saraksts sniedzas nākamajā dienā.</summary>
		private ShiftType shiftType;
		/// <summary>Stacijas laika josla raidījumu laika pārrēķinam.</summary>
		protected readonly TimeZoneInfo timezone;
		/// <summary>Diennakts raidījumu kopa.</summary>
		protected readonly List<Tuple<DateTime, TBroadcastData>> guide;
		/// <summary>HTTP klients raidījma datu izgūšanai.</summary>
		protected readonly ProperWebClient client=new ProperWebClient();
		/// <summary>
		/// Tipizēts PropertyChanged notikums, kurš iestājas, kad nomainās nākošais raidījums.
		/// Uz šo parasti reģistrējas kombinētajos raidījumu sarakstos.
		/// </summary>
		public event Action<Broadcast> NextBroadcastChanged;

		protected ListedGuide(TimeZoneInfo timezone, Menu<Guide> menu)
			: base(menu) {
			this.timezone=timezone;
			guide=new List<Tuple<DateTime, TBroadcastData>>(30);
		}

		/// <summary>Šo metodi vienreiz jāizsauc apakšklases konstruktorā.</summary>
		public override async Task Start(bool initialize) {
			DateTime now=TimeZoneInfo.ConvertTime(DateTimeOffset.Now, timezone).DateTime;
			if (initialize) {
				await Task.Run(() => FillGuide(now.Date));
				if (guide.Count < 2) return;
				if (guide[0].Item1 > now) {
					// Ja pirmais šīsdienas raidījums ir nākotnē, tad pieprasa vakardienas raidījumus...
					var nextGuide=new Tuple<DateTime, TBroadcastData>[guide.Count];
					guide.CopyTo(nextGuide);
					guide.Clear();
					try { await Task.Run(() => FillGuide(now.AddDays(-1).Date)); } catch { }
					// ...un apvieno tos ar šodienas.
					guide.AddRange(nextGuide);
					if (nextGuide[0].Item1.Date == nextGuide[nextGuide.Length-1].Item1.Date)
						shiftType=ShiftType.Start;
					else shiftType=ShiftType.StartEnd;
				} else shiftType=guide[0].Item1.Date == guide[guide.Count-1].Item1.Date ?
					(guide[0].Item1.TimeOfDay == new TimeSpan(0) ? ShiftType.None:ShiftType.Start):ShiftType.StartEnd;
			}
			// Atrod pašreizējo raidījumu pēc sākumlaika.
			for (currentIdx=1; currentIdx < guide.Count && guide[currentIdx].Item1 <= now; currentIdx++) ;
			currentIdx--; // ^ atrada nākamo, bet vajag pašreizējo.
			resetIndex=false;
			await base.Start(initialize);
		}
		public override void Stop() {
			// Saglabā aktuālos raidījumus, ja saraksts jau ir nāktonē. Raidījumi var būt null, ja pirmīt jau izsauca Stop.
			if (NextBroadcast != null && NextBroadcast.StartTime < guide[0].Item1)
				guide.Insert(0, DataFromBroadcast(NextBroadcast));
			if (CurrentBroadcast != null && CurrentBroadcast.StartTime < guide[0].Item1)
				guide.Insert(0, DataFromBroadcast(CurrentBroadcast));
			if (PreviousBroadcast != null && PreviousBroadcast.StartTime < guide[0].Item1)
				guide.Insert(0, DataFromBroadcast(PreviousBroadcast));
			base.Stop();
		}
		/// <summary>
		/// Izgūst raidījuma datus no <paramref name="broadcastData"/>.
		/// Laiki tiek padoti, jo nepieciešami <see cref="Broadcast"/> konstruktoram.
		/// </summary>
		/// <param name="startTime">Raidījuma sākumlaiks lietotāja laika joslā.</param>
		/// <param name="endTime">Raidījuma beigu laiks lietotāja laika joslā.</param>
		/// <param name="broadcastData">Raidījuma dati, saprotami apakšklasei.</param>
		/// <returns>Pilnvērtīgs raidījums.</returns>
		protected abstract Broadcast GetBroadcast(DateTime startTime, DateTime endTime, TBroadcastData broadcastData);
		/// <summary>Aizpilda raidījumu sarakstu ar diennakts <paramef name="date"/> raidījumiem raidstacijas laika joslā.</summary>
		/// <remarks>Ja izraisa kļūdu, tad to apstrādā <see cref="MainWindow.StartGuide"/>.
		/// Izpildās fonā, jo parasti ir smaga darbība.</remarks>
		protected abstract Task FillGuide(DateTime date);
		/// <summary>Pieliek raidījuma datus saraksta sākumā.</summary>
		protected abstract Tuple<DateTime, TBroadcastData> DataFromBroadcast(Broadcast broadcast);

		protected override async Task UpdateBroadcasts() {
			PreviousBroadcast=CurrentBroadcast ?? GetBroadcast(currentIdx-1); // Iepriekšējais raidījums pirmajā reizē drīkst būt null.
			if (NextBroadcast == null && currentIdx == guide.Count-1) { // Ja sāk atskaņot ar pēdējo raidījumu.
				CurrentBroadcast=await GetBroadcastWithGuide();
				resetIndex=true;
			} else CurrentBroadcast=NextBroadcast ?? GetBroadcast(currentIdx);

			if (currentIdx == guide.Count-2) { // Ja pašreizējais raidījums ir pirmspēdējais, nomaina raidījumu sarakstu.
				currentIdx++;
				NextBroadcast=await GetBroadcastWithGuide();
				resetIndex=true;
			} else if (resetIndex) {
				NextBroadcast=GetBroadcast(0); // Raidījums no jaunā saraksta.
				currentIdx=0;
				resetIndex=false;
			} else {
				currentIdx++;
				NextBroadcast=GetBroadcast(currentIdx); // Raidījums no esošā saraksta.
			}
			if (NextBroadcastChanged != null) NextBroadcastChanged(NextBroadcast);
		}
		/// <summary>
		/// Atgriež pašreizējo raidījumu ar sākumlaiku no esošā un beigu laiku no nākamās dienas saraksta.
		/// Nomaina raidījumu sarakstu.
		/// </summary>
		private async Task<Broadcast> GetBroadcastWithGuide() {
			var item=guide[currentIdx];
			DateTime tomorrow;
			switch (shiftType) {
				case ShiftType.None: tomorrow=guide[0].Item1.AddDays(1); break;
				case ShiftType.Start: tomorrow=item.Item1.AddDays(1); break;
				default:/*case ShiftType.StartEnd:*/ tomorrow=item.Item1; break;
			}
			guide.Clear();
			await Task.Run(() => FillGuide(tomorrow.Date));
			if (guide.Count < 2) return null;
			return GetBroadcast(TimeZoneInfo.ConvertTime(item.Item1, timezone, TimeZoneInfo.Local), // Sākumlaiks no vecā saraksta.
				TimeZoneInfo.ConvertTime(guide[0].Item1, timezone, TimeZoneInfo.Local), // Beigu laiks no jaunā.
				item.Item2);
		}
		/// <summary>Atgriež raidījumu ar kārtas numuru <paramref name="idx"/>.</summary>
		/// <param name="idx">Raidījuma numurs kopā <see cref="guide"/>.</param>
		/// <returns>Atbilstošs raidījums vai <c>null</c>, ja nepareizs kārtas numurs.</returns>
		private Broadcast GetBroadcast(int idx) {
			if (idx < 0 || idx >= guide.Count-1) return null;
			var guideItem=guide[idx];
			return GetBroadcast(TimeZoneInfo.ConvertTime(guideItem.Item1, timezone, TimeZoneInfo.Local),
				TimeZoneInfo.ConvertTime(guide[idx+1].Item1, timezone, TimeZoneInfo.Local), // Raidījums beidzas, kad sākas nākamais.
				guideItem.Item2);
		}
	}

	/// <summary>Diennakts raidījumu saraksts, kurš sastāv no nosaukumiem.</summary>
	public abstract class CaptionListedGuide : ListedGuide<string> {
		protected CaptionListedGuide(TimeZoneInfo timezone, Menu<Guide> menu) : base(timezone, menu) {}
		protected override Broadcast GetBroadcast(DateTime startTime, DateTime endTime, string caption) {
			return new Broadcast(startTime, endTime, caption);
		}
		protected void AddBroadcast(DateTime startTime, string caption) {
			guide.Add(Tuple.Create(startTime, caption));
		}
		protected override Tuple<DateTime, string> DataFromBroadcast(Broadcast broadcast) {
			return Tuple.Create(TimeZoneInfo.ConvertTime(broadcast.StartTime, TimeZoneInfo.Local, timezone), broadcast.Caption);
		}
	}

	/// <summary>Diennakts raidījumu saraksts, kurš sastāv no nosaukumiem un detaļām.</summary>
	public abstract class DescriptionListedGuide : ListedGuide<Tuple<string, string>> {
		protected DescriptionListedGuide(TimeZoneInfo timezone, Menu<Guide> menu) : base(timezone, menu) {}
		protected override Broadcast GetBroadcast(DateTime startTime, DateTime endTime, Tuple<string, string> data) {
			return new Broadcast(startTime, endTime, data.Item1, data.Item2);
		}
		protected void AddBroadcast(DateTime startTime, string caption, string description) {
			guide.Add(Tuple.Create(startTime, Tuple.Create(caption, description)));
		}
		protected override Tuple<DateTime, Tuple<string, string>> DataFromBroadcast(Broadcast broadcast) {
			return Tuple.Create(TimeZoneInfo.ConvertTime(broadcast.StartTime, TimeZoneInfo.Local, timezone),
				Tuple.Create(broadcast.Caption, broadcast.Description));
		}
	}
	/// <summary>Diennakts raidījumu saraksts, kurš sastāv no nosaukumiem, detaļām un raidījumu adresēm.</summary>
	public abstract class PagedListedGuide : ListedGuide<Tuple<string, string, string>> {
		protected PagedListedGuide(TimeZoneInfo timezone, Menu<Guide> menu) : base(timezone, menu) { }
		protected override Broadcast GetBroadcast(DateTime startTime, DateTime endTime, Tuple<string, string, string> data) {
			return new PagedBroadcast(startTime, endTime, data.Item1, data.Item2, data.Item3);
		}
		protected void AddBroadcast(DateTime startTime, string caption, string description, string pageUrl) {
			guide.Add(Tuple.Create(startTime, Tuple.Create(caption, description, pageUrl)));
		}
		protected override Tuple<DateTime, Tuple<string, string, string>> DataFromBroadcast(Broadcast broadcast) {
			return Tuple.Create(TimeZoneInfo.ConvertTime(broadcast.StartTime, TimeZoneInfo.Local, timezone),
				Tuple.Create(broadcast.Caption, broadcast.Description, ((PagedBroadcast)broadcast).PageUrl));
		}
	}
}