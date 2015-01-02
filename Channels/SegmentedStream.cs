using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DrDax.RadioClient {
	/// <summary>Wowza Media Server MP3 plūsma ar daļiņu sarakstu M3U failā.</summary>
	public class SegmentedStream : FullReadStream {
		/// <summary>Pašreiz lasāmā MP3 fragmenta (faila) indekss.</summary>
		private int chunkIdx;
		/// <summary>MP3 fragmenta (faila) adreses sagatave, kurā ielikt <see cref="chunkIdx"/>.</summary>
		private string urlFormat;
		private readonly string baseUrl;

		/// <param name="baseUrl">Atskaņojamās plūsmas adreses sākumdaļa (ar slīpsvītru galā).</param>
		public SegmentedStream(string baseUrl) {
			this.baseUrl=baseUrl;
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
					if (readAheadLength == 0) Open().GetAwaiter().GetResult();
				}
			}
			return bytesRead;
		}

		/// <summary>Atver kārtējo MP3 fragmentu kā <see cref="sourceStream"/>.</summary>
		public override async Task Open() {
			try {
				if (urlFormat == null)
					using (var client=new ProperWebClient(System.Text.Encoding.ASCII)) {
						string playlist=await client.DownloadStringTaskAsync(baseUrl+"chunklist.m3u8");
						urlFormat=baseUrl+"media_{0}.mp3"+new Regex("\\?wowzasessionid=([0-9]+)").Match(playlist).Value;
						chunkIdx=int.Parse(new Regex("MEDIA-SEQUENCE:([0-9]+)").Match(playlist).Groups[1].Value);
					}
				else if (sourceStream != null) sourceStream.Dispose();

				HttpWebRequest request=(HttpWebRequest)WebRequest.Create(string.Format(urlFormat, chunkIdx));
				request.ApplyProxy(); request.Timeout=3000; // Trīs sekundes.
				HttpWebResponse response=(HttpWebResponse)(await request.GetResponseAsync());
				sourceStream=response.GetResponseStream();
			} catch (Exception) {
				// Lai nākošreiz mēģina iegūt jaunu atskaņošanas adresi.
				urlFormat=null;
				throw;
			}
			chunkIdx++;
		}
	}
}