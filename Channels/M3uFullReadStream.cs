using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace DrDax.RadioClient {
	/// <summary>Wowza Media Server MP3 plūsma ar daļiņu sarakstu M3U failā.</summary>
	public class M3uFullReadStream : FullReadStream {
		/// <summary>Pašreiz lasāmā MP3 fragmenta (faila) indekss.</summary>
		private ushort chunkIdx;
		/// <summary>MP3 fragmenta (faila) adreses sagatave, kurā ielikt <see cref="chunkIdx"/>.</summary>
		private string urlFormat;

		/// <param name="baseUrl">Atskaņojamās plūsmas adreses sākumdaļa.</param>
		public M3uFullReadStream(string baseUrl) {
			string playlist;
			using (var client=new ProperWebClient(System.Text.Encoding.ASCII)) {
				playlist=client.DownloadString(baseUrl+"chunklist.m3u8");
			}
			urlFormat=baseUrl+"media_{0}.mp3"+new Regex("\\?wowzasessionid=([0-9]+)").Match(playlist).Value;
			chunkIdx=ushort.Parse(new Regex("MEDIA-SEQUENCE:([0-9]+)").Match(playlist).Groups[1].Value);
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
					if (sourceStream == null) SetSourceStream();
					readAheadLength=sourceStream.Read(readAheadBuffer, 0, readAheadBuffer.Length);
					if (readAheadLength == 0) SetSourceStream();
				}
			}
			this.pos+=bytesRead;
			return bytesRead;
		}

		/// <summary>Atver kārtējo MP3 fragmentu kā <see cref="sourceStream"/>.</summary>
		private void SetSourceStream() {
			HttpWebRequest request=(HttpWebRequest)WebRequest.Create(string.Format(urlFormat, chunkIdx));
			request.ApplyProxy(); request.Timeout=3000; // Trīs sekundes.
			HttpWebResponse response=(HttpWebResponse)request.GetResponse();
			sourceStream=response.GetResponseStream();
			chunkIdx++;
		}

		public override void Flush() {
			throw new InvalidOperationException();
		}
		public override long Seek(long offset, SeekOrigin origin) {
			throw new InvalidOperationException();
		}
		public override void SetLength(long value) {
			throw new InvalidOperationException();
		}
		public override void Write(byte[] buffer, int offset, int count) {
			throw new InvalidOperationException();
		}
	}
}