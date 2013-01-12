using System;
using System.Text;
using System.Text.RegularExpressions;

namespace DrDax.RadioClient {
	/// <summary>Radio programma, kura ņem datus no ShoutCast plūsmas ICE meta datiem.</summary>
	public abstract class IcyGuide : Guide {
		// Praksē drīkst būt nulles garums, bet šeit pieņemsim, ka jābūt virsraksta vērtībai.
		private static readonly Regex metaHeaderRx=new Regex("StreamTitle='(.+?)';", RegexOptions.Compiled | RegexOptions.CultureInvariant);
		/// <summary>Iepriekšējā reizē saņemtie meta dati.</summary>
		private string lastMetaHeader=null;
		/// <summary>
		/// Meta datu teksta kodu tabula. Nosaka raidstacija, pēc noklusējuma ASCII.
		/// </summary>
		private readonly Encoding metaHeaderEncoding;

		public override bool HasKnownDuration { get { return false; } }

		/// <summary>Apstrādā meta datu title daļu un atgriež raidījumu.</summary>
		/// <remarks>Raidījuma beigu laiks netiek ņemts vērā.</remarks>
		/// <param name="metaTitle">Meta datu title daļa.</param>
		protected abstract Broadcast GetBroadcast(string metaTitle);

		/// <param name="metaHeaderEncoding">Meta datu kodu tabula.</param>
		protected IcyGuide(Encoding metaHeaderEncoding) {
			if (metaHeaderEncoding == null) throw new ArgumentNullException("metaHeaderEncoding");
			this.metaHeaderEncoding=metaHeaderEncoding;
		}
		/// <summary>ICE radio programma ar ASCII kodu tabulu.</summary>
		protected IcyGuide() : this(Encoding.ASCII) {}

		/// <summary>
		/// Apstrādā meta datus no bināras formas līdz raidījumam.
		/// </summary>
		/// <param name="headerData">Meta dati, kā tie nolasīti no interneta plūsmas.</param>
		/// <param name="headerSize">Noderīgo baitu skaits masīvā <paramref name="headerData"/>.</param>
		public void ProcessMetaHeader(byte[] headerData, int headerSize) {
			string metaHeader=metaHeaderEncoding.GetString(headerData, 0, headerSize);
			if (metaHeader != lastMetaHeader) {
				System.Diagnostics.Debug.WriteLine(metaHeader);
				Match metaMatch=metaHeaderRx.Match(metaHeader);
				Broadcast broadcast=GetBroadcast(metaMatch.Success ? metaMatch.Groups[1].Value.Trim():null);
				if (broadcast != null) {
					PreviousBroadcast=CurrentBroadcast;
					CurrentBroadcast=broadcast;
				}
				lastMetaHeader=metaHeader;
			}
		}
	}
}