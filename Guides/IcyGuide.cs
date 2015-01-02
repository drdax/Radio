using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DrDax.RadioClient {
	/// <summary>Radio programma, kura ņem datus no ShoutCast plūsmas ICE meta datiem.</summary>
	public abstract class IcyGuide : Guide {
		// Praksē drīkst būt nulles garums, bet šeit pieņemsim, ka jābūt virsraksta vērtībai.
		private static readonly Regex metaHeaderRx=new Regex("StreamTitle='(.+?)';", RegexOptions.Compiled | RegexOptions.CultureInvariant);
		/// <summary>Iepriekšējā reizē saņemtie meta dati.</summary>
		private string lastMetaHeader=null;
		/// <summary>Meta datu teksta kodu tabula. Nosaka raidstacija, pēc noklusējuma ASCII.</summary>
		private readonly Encoding metaHeaderEncoding;
		/// <summary>Vai apstrādāt dziesmu meta datus.</summary>
		/// <remarks>Palaišanas process ir ilgs, tāpēc vajag novērst pirmslaika izsaukumus, kuri varētu cīnīties par WebClient piemēram.</remarks>
		private bool started=false;

		public override Task Start(bool initialize) {
			started=true;
			return Task.FromResult(0); // Pret brīdinājumu CS1998. 0 ir viena no gatavām atbildēm asinhronām metodēm.
		}
		public override void Stop() {
			started=false;
			lastMetaHeader=null;
			base.Stop();
		}

		/// <summary>Apstrādā meta datu title daļu un atgriež raidījumu.</summary>
		/// <remarks>Raidījuma beigu laiks netiek ņemts vērā.</remarks>
		/// <param name="metaTitle">Meta datu title daļa.</param>
		protected abstract Task<Broadcast> GetBroadcast(string metaTitle);

		/// <param name="metaHeaderEncoding">Meta datu kodu tabula.</param>
		protected IcyGuide(Encoding metaHeaderEncoding, Menu<Guide> menu) : base(menu, false, true) {
			if (metaHeaderEncoding == null) throw new ArgumentNullException("metaHeaderEncoding");
			this.metaHeaderEncoding=metaHeaderEncoding;
		}
		/// <summary>ICE radio programma ar ASCII kodu tabulu bez izvēlnes.</summary>
		protected IcyGuide() : this(Encoding.ASCII, null) {}

		/// <summary>
		/// Apstrādā meta datus no bināras formas līdz raidījumam.
		/// </summary>
		/// <param name="headerData">Meta dati, kā tie nolasīti no interneta plūsmas.</param>
		/// <param name="headerSize">Noderīgo baitu skaits masīvā <paramref name="headerData"/>.</param>
		public async void ProcessMetaHeader(byte[] headerData, int headerSize) {
			if (!started) return;
			string metaHeader=metaHeaderEncoding.GetString(headerData, 0, headerSize);
			if (metaHeader != lastMetaHeader) {
				System.Diagnostics.Debug.WriteLine(metaHeader);
				Match metaMatch=metaHeaderRx.Match(metaHeader);
				try {
					Broadcast broadcast=await GetBroadcast(metaMatch.Success ? metaMatch.Groups[1].Value.Trim():null);
					if (broadcast != null) {
						PreviousBroadcast=CurrentBroadcast;
						CurrentBroadcast=broadcast;
					}
				} catch {} // Gadījumā, ja izgāžas kāds Tīmekļa pieprasījums.
				lastMetaHeader=metaHeader;
			}
		}
	}
}