using System;
using System.Collections.Generic;

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

		protected ListedGuide(TimeZoneInfo timezone) {
			this.timezone=timezone;
			guide=new List<Tuple<DateTime, TBroadcastData>>(30);
		}

		/// <summary>Šo metodi vienreiz jāizsauc apakšklases konstruktorā.</summary>
		protected void Initialize() {
			DateTime now=TimeZoneInfo.ConvertTime(DateTimeOffset.Now, timezone).DateTime;
			try { FillGuide(now.Date); } catch {}
			if (guide.Count < 2) return;
			if (guide[0].Item1 > now) {
				// Ja pirmais šīsdienas raidījums ir nākotnē, tad pieprasa vakardienas raidījumus...
				var nextGuide=new Tuple<DateTime, TBroadcastData>[guide.Count];
				guide.CopyTo(nextGuide);
				guide.Clear();
				try { FillGuide(now.AddDays(-1).Date); } catch {}
				// ...un apvieno tos ar šodienas.
				guide.AddRange(nextGuide);
				if (nextGuide[0].Item1.Date == nextGuide[nextGuide.Length-1].Item1.Date)
					shiftType=ShiftType.Start;
				else shiftType=ShiftType.StartEnd;
			} else shiftType=guide[0].Item1.Date == guide[guide.Count-1].Item1.Date ?
				(guide[0].Item1.TimeOfDay == new TimeSpan(0) ? ShiftType.None:ShiftType.Start):ShiftType.StartEnd;
			// Atrod pašreizējo raidījumu pēc sākumlaika.
			for (currentIdx=1; currentIdx < guide.Count; currentIdx++)
				if (guide[currentIdx].Item1 > now) break; // Ja nākamais raidījums.
			currentIdx--; // ^ atrada nākamo, bet vajag pašreizējo.
			base.StartTimer();
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
		protected abstract void FillGuide(DateTime date);

		protected override void UpdateBroadcasts() {
			PreviousBroadcast=CurrentBroadcast == null ? GetBroadcast(currentIdx-1):CurrentBroadcast; // Iepriekšējais raidījums pirmajā reizē drīkst būt null.
			if (NextBroadcast == null && currentIdx == guide.Count-1) { // Ja sāk atskaņot ar pēdējo raidījumu.
				CurrentBroadcast=GetBroadcastWithGuide();
				resetIndex=true;
			} else CurrentBroadcast=NextBroadcast == null ? GetBroadcast(currentIdx):NextBroadcast;

			if (currentIdx == guide.Count-2) { // Ja pašreizējais raidījums ir pirmspēdējais, nomaina raidījumu sarakstu.
				currentIdx++;
				NextBroadcast=GetBroadcastWithGuide();
				resetIndex=true;
			} else if (resetIndex) {
				NextBroadcast=GetBroadcast(0); // Raidījums no jaunā saraksta.
				currentIdx=0;
				resetIndex=false;
			} else {
				currentIdx++;
				NextBroadcast=GetBroadcast(currentIdx); // Raidījums no esošā saraksta.
			}
		}
		/// <summary>
		/// Atgriež pašreizējo raidījumu ar sākumlaiku no esošā un beigu laiku no nākamās dienas saraksta.
		/// Nomaina raidījumu sarakstu.
		/// </summary>
		private Broadcast GetBroadcastWithGuide() {
			var item=guide[currentIdx];
			DateTime tomorrow;
			switch (shiftType) {
				case ShiftType.None: tomorrow=guide[0].Item1.AddDays(1); break;
				case ShiftType.Start: tomorrow=item.Item1.AddDays(1); break;
				default:/*case ShiftType.StartEnd:*/ tomorrow=item.Item1; break;
			}
			guide.Clear();
			try { FillGuide(tomorrow.Date); } catch {}
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
		protected CaptionListedGuide(TimeZoneInfo timezone) : base(timezone) {}
		protected override Broadcast GetBroadcast(DateTime startTime, DateTime endTime, string caption) {
			return new Broadcast(startTime, endTime, caption);
		}
		protected void AddBroadcast(DateTime startTime, string caption) {
			guide.Add(Tuple.Create(startTime, caption));
		}
	}

	/// <summary>Diennakts raidījumu saraksts, kurš sastāv no nosaukumiem un detaļām.</summary>
	public abstract class DescriptionListedGuide : ListedGuide<Tuple<string, string>> {
		protected DescriptionListedGuide(TimeZoneInfo timezone) : base(timezone) { }
		protected override Broadcast GetBroadcast(DateTime startTime, DateTime endTime, Tuple<string, string> data) {
			return new Broadcast(startTime, endTime, data.Item1, data.Item2);
		}
		protected void AddBroadcast(DateTime startTime, string caption, string description) {
			guide.Add(Tuple.Create(startTime, Tuple.Create(caption, description)));
		}
	}
}