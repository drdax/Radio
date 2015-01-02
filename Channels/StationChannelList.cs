using System;
using System.Collections.Generic;

namespace DrDax.RadioClient {
	/// <summary>
	/// Stacijas kanālu saraksts attēlošanai palaišanas sarakstā.
	/// </summary>
	/// <remarks>Atslēga ir kanāla identifikators stacijas ietvaros.
	/// Vērtība ir kanāla attēlojamais nosaukums un ikonas indekss resursos.</remarks>
	public class StationChannelList : Dictionary<uint, Tuple<string, int>> {
		private readonly bool autoIconIndex;
		/// <param name="autoIconIndex">
		/// Vai piešķirt ikonas indeksu, kurš sakrīt ar kanāla kārtas numuru. Pēc noklusējuma piešķir nulli.
		/// </param>
		public StationChannelList(bool autoIconIndex=false)
			: base(7) { // Vidējais kanālu skaits šobrīd aptuveni pieci, bet, tā kā vārdnīca lieto HashHelpers.primes, tad nākamais mazākais ir septiņi.
			this.autoIconIndex=autoIconIndex;
		}

		public void Add(uint id, string caption, int iconIdx) {
			Add(id, Tuple.Create(caption, iconIdx));
		}
		public void Add(uint id, string caption) {
			Add(id, Tuple.Create(caption, autoIconIndex ? base.Count:0));
		}
		public void Add(string caption) {
			Add((byte)(base.Count+1), Tuple.Create(caption, autoIconIndex ? base.Count:0));
		}
		public void Add(string caption, int iconIdx) {
			Add((byte)(base.Count+1), Tuple.Create(caption, iconIdx));
		}
	}
}
