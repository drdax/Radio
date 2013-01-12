using System;
using System.IO;
using System.Text;

namespace DrDax.RadioClient {
	// No NAudio MP3 plūsmošanas demo, autors Mark Heath.
	// ICE meta datu dekodēšana no http://www.codeproject.com/Articles/11308/SHOUTcast-Stream-Ripper, autors espitech.
	/// <summary>
	/// Plūsma, kura no datu avota 1) nolasa tik baitu, cik prasīts 2) dekodē ICE meta datus, ja tādi ir.
	/// </summary>
	public class IcyFullReadStream : FullReadStream {
		#region ICE meta dati
		/// <summary>Meta datu atdalītāja biežums baitos.</summary>
		private readonly ushort metaInt;
		/// <summary>Meta datu bloka garums baitos.</summary>
		private int metaSize=0;
		/// <summary>Pašreiz lasāmie meta dati, kuri tiek savākti pa simbolam.</summary>
		private byte[] metaBytes;
		/// <summary>
		/// Nolasīto baitu skaits kopš iepriekšējā meta datu atdalītāja.
		/// </summary>
		private int metaCount=0;
		/// <summary>Pašlaik nolasīto meta datu baita indekss.</summary>
		private int metaOffset=0;
		/// <summary>Metode, kura apstrādā meta datus.</summary>
		private Action<byte[], int> metaHeaderCallback;
		#endregion

		public IcyFullReadStream(Stream sourceStream, ushort metaInt, Action<byte[], int> metaHeaderCallback) {
			this.sourceStream=sourceStream;
			this.metaInt=metaInt;
			if (metaInt != 0 && metaHeaderCallback == null)
				throw new ArgumentNullException("metaHeaderCallback", "Ja ir meta dati, tad jānorāda to apstrādātājs.");
			this.metaHeaderCallback=metaHeaderCallback;
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
					readAheadOffset=0;
					readAheadLength=sourceStream.Read(readAheadBuffer, 0, readAheadBuffer.Length);
					if (readAheadLength == 0) break;
					if (metaInt != 0) { // Ja iespējami meta dati.
						int nonMetaLength=0;
						// Analizē datus un atdala MP3 plūsmu no ICE meta datiem.
						for (int n=0; n < readAheadLength; n++) {
							if (metaSize != 0) { // Ja lasa metadatus.
								if (readAheadBuffer[n] == 0 || metaOffset == metaSize) { // Nulles baits apzīmē rindas beigas.
									metaHeaderCallback(metaBytes, metaOffset);
									n+=metaSize-metaOffset-1;
									metaSize=0;
								} else metaBytes[metaOffset++]=readAheadBuffer[n];
							} else if (metaCount++ == metaInt) { // Ja sasniedza meta datu sākumu.
								metaSize=readAheadBuffer[n]*16;
								metaBytes=new byte[metaSize];
								metaCount=0; metaOffset=0;
							} else readAheadBuffer[nonMetaLength++]=readAheadBuffer[n];
						}
						readAheadLength=nonMetaLength;
					}
				}
			}
			this.pos+=bytesRead;
			return bytesRead;
		}
		protected override void Dispose(bool disposing) {
			metaHeaderCallback=null;
			sourceStream.Dispose();
			base.Dispose(disposing);
		}
	}
}