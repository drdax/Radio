using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DrDax.RadioClient {
	// No NAudio MP3 plūsmošanas demo, autors Mark Heath.
	// ICE meta datu dekodēšana no http://www.codeproject.com/Articles/11308/SHOUTcast-Stream-Ripper, autors espitech.
	/// <summary>
	/// Plūsma, kura no datu avota 1) nolasa tik baitu, cik prasīts 2) dekodē ICE meta datus, ja tādi ir.
	/// </summary>
	public class IcyStream : FullReadStream {
		#region ICE meta dati
		/// <summary>Meta datu atdalītāja biežums baitos.</summary>
		private ushort metaInt;
		/// <summary>Meta datu bloka garums baitos.</summary>
		private int metaSize;
		/// <summary>Pašreiz lasāmie meta dati, kuri tiek savākti pa simbolam.</summary>
		private byte[] metaBytes;
		/// <summary>
		/// Nolasīto baitu skaits kopš iepriekšējā meta datu atdalītāja.
		/// </summary>
		private int metaCount;
		/// <summary>Pašlaik nolasīto meta datu baita indekss.</summary>
		private int metaOffset;
		/// <summary>Metode, kura apstrādā meta datus.</summary>
		public Action<byte[], int> MetaHeaderCallback;
		/// <summary>Vai noderīgie dati no pašreizējā meta datu bloka jau apstrādāti.</summary>
		/// <remarks>Ieviests, lai <see cref="MetaHeaderCallback"/> netiktu izsaukts, kad <see cref="Read"/> nolasa lieko meta datu atlikumu.</remarks>
		private bool metaRead;
		#endregion
		private readonly string url;
		/// <summary>Programmas identifikators, griežoties pie servera.</summary>
		public string UserAgent;

		public IcyStream(string url) {
			this.url=url;
		}

		public override int Read(byte[] buffer, int offset, int count) {
			int bytesRead=0;
			while (bytesRead < count) {
				int readAheadAvailableBytes=readAheadLength-readAheadOffset;
				if (readAheadAvailableBytes > 0) {
					int toCopy=Math.Min(readAheadAvailableBytes, count-bytesRead /*vēl nekopēto baitu skaits*/);
					Array.Copy(readAheadBuffer, readAheadOffset, buffer, offset+bytesRead, toCopy);
					bytesRead+=toCopy; readAheadOffset+=toCopy;
				} else {
					if (sourceStream == null) break;
					readAheadOffset=0;
					readAheadLength=sourceStream.Read(readAheadBuffer, 0, readAheadBuffer.Length);
					if (readAheadLength == 0) break;
					if (metaInt != 0) { // Ja iespējami meta dati.
						int nonMetaLength=0;
						// Analizē datus un atdala MP3 plūsmu no ICE meta datiem.
						for (int n=0; n < readAheadLength; n++) {
							if (metaSize != 0) { // Ja lasa metadatus.
								if (readAheadBuffer[n] == 0 || metaOffset == metaSize) { // Nulles baits apzīmē rindas beigas.
									if (!metaRead) { MetaHeaderCallback(metaBytes, metaOffset); metaRead=true; }
									if (metaOffset == metaSize) { metaSize=0; n--; } // n=n-1, lai šo baitu apstrādātu ārpus meta datiem.
									else {
										int diff=Math.Min(metaSize-metaOffset, readAheadLength-n);
										metaOffset+=diff; n+=diff-1; // Izlaiž nevadzīgos baitus.
									}
								} else metaBytes[metaOffset++]=readAheadBuffer[n];
							} else if (metaCount++ == metaInt) { // Ja sasniedza meta datu sākumu.
								metaSize=readAheadBuffer[n]*16;
								metaBytes=new byte[metaSize];
								metaCount=0; metaOffset=0; metaRead=false;
							} else readAheadBuffer[nonMetaLength++]=readAheadBuffer[n];
						}
						readAheadLength=nonMetaLength;
					}
				}
			}
			return bytesRead;
		}
		public override async Task Open() {
			metaSize=0; metaCount=0; metaOffset=0; readAheadLength=0; metaInt=0;
			bool hasMetaTitles=MetaHeaderCallback != null;
			HttpWebRequest request=(HttpWebRequest)WebRequest.Create(url);
			request.ApplyProxy();
			request.UserAgent=UserAgent;
			//request.Timeout=connectionTimeout;
			request.Timeout=3000;
			request.ReadWriteTimeout=5000;
			// Pieprasa ICE meta datus.
			if (hasMetaTitles) request.Headers.Add("Icy-MetaData", "1");
			response=null;
			try {
				response=(HttpWebResponse)(await request.GetResponseAsync());
			} catch (WebException) {
				/*if (connectionTimeout == 30000) connectionTimeout=5000;
				else connectionTimeout+=5000;
				unexpectedStop.BeginInvoke(null, null);*/
				throw;
			}

			hasMetaTitles=hasMetaTitles && ushort.TryParse(response.GetResponseHeader("icy-metaint"), out metaInt);
			System.Diagnostics.Debug.WriteLine("metaint="+metaInt);
			sourceStream=response.GetResponseStream();
		}
		protected override void Dispose(bool disposing) {
			MetaHeaderCallback=null;
			if (sourceStream != null) sourceStream.Dispose();
			base.Dispose(disposing);
		}
	}
}