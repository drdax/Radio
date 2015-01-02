using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace DrDax.RadioClient {
	/// <summary>
	/// Wowza M3U8 video sarakstu satura rettranslēšanas serveris pieslēgumam no Winodws Media.
	/// </summary>
	public class VideoServer : IDisposable {
		/// <param name="playlistUrl">playlist.m3u8 faila pilna adrese.</param>
		/// <param name="segmentsRegex">Segmentu faila nosaukuma regulārais izteiciens.</param>
		public VideoServer(string playlistUrl, string segmentsRegex) {
			if (string.IsNullOrEmpty(playlistUrl) || string.IsNullOrEmpty(segmentsRegex)) throw new ArgumentNullException();
			this.playlistUrl=playlistUrl;
			this.segmentsRx=new Regex(segmentsRegex);
			client.Headers.Add(HttpRequestHeader.UserAgent, "AppleCoreMedia/1.0.0.9B206 (iPad; U; CPU OS 5_1_1 like Mac OS X; en_us)");
		}
		/// <summary>Palaiž serveri. Tas kļūst pieejams <see cref="StreamUrl"/> adresē.</summary>
		public void Start() {
			cts=new CancellationTokenSource();
			worker=Task.Factory.StartNew(DoWork, cts.Token);
		}
		/// <summary>Apstādina serva procesu.</summary>
		public void Stop() {
			if (cts == null) return;
			listener.Abort();
			cts.Cancel();
			try { worker.Wait(); } catch {}
			cts.Dispose();
			worker=null; cts=null; listener=null;
		}
		public void Dispose() {
			if (worker != null) Stop();
			client.Dispose();
		}

		private void DoWork() {
			listener=new HttpListener(); // Jāveido katru reizi jauns, jo Close ir izsaucams vienreiz.
			listener.Prefixes.Add(StreamUrl);
			listener.Start();
			// Pirmais pieprasījums no mēdiju atskaņotāja vienmēr nobirst ar 1229 (ERROR_CONNECTION_INVALID), kad mēģina rakstīt datus.
			// Tāpēc vienkārši pasaka datu tipu un gaida nākamo pieprasījumu.
			HttpListenerContext context;
			try {
				context=listener.GetContext();
			} catch (HttpListenerException) { return; }
			HttpListenerResponse response=context.Response;
			response.ContentType=ResponseContentType;
			response.Close();
			if (cts.IsCancellationRequested) return;

			// Sagaida pareizo klienta pieslēgumu.
			try {
				context=listener.GetContext();
			} catch (HttpListenerException) { return; }
			// Video fragmentu atskaņošanas saraksta adrese. Parasti mainās katru sesiju.
			string segmentsUrl=segmentsRx.Match(client.DownloadString(playlistUrl)).Value;
			if (!segmentsUrl.StartsWith("http:")) // Youtube ir pilnās adreses.
				segmentsUrl=playlistUrl.Substring(0, playlistUrl.LastIndexOf('/')+1)+segmentsUrl;
			// Video failu mapes adrese. Parasti sakrīt ar sākotnēja video saraksta mapi.
			string folderUrl=segmentsUrl.Substring(0, segmentsUrl.LastIndexOf('/')+1);
			if (cts.IsCancellationRequested) return;
			byte[] buffer=new byte[65536]; int bytesRead;
			response=context.Response;
			response.ContentType=ResponseContentType;
			Stream output=response.OutputStream;
			bool outputEnded=false;
			byte[] key=null, iv=null; Int32 sequenceIdx=0;

			do {
				if (segments.Count == 0) {
					string playlist=client.DownloadString(segmentsUrl);
					Match keyMatch=keyRx.Match(playlist); // Te vēl var būt IV vērtība, bet atšifrēšana pirmkārt domāta Эхо Москвы, kura IV ir segmenta indekss.
					// Atslēgas adrese ir fragmentu sarakstā un tai nevajadzētu mainīties uztveršanas laikā. Vismaz Эхо gadījumā atslēga nemainās, bet tās iegūšanai ir nepieciešams sesijas identifikators.
					if (keyMatch.Success && key == null) {
						key=client.DownloadData(keyMatch.Groups[1].Value);
						iv=new byte[16];
					}
					if (key != null) sequenceIdx=int.Parse(sequenceRx.Match(playlist).Groups[1].Value);

					// Iegūst fragmentu sarakstu.
					foreach (Match match in segmentRx.Matches(playlist))
						segments.Enqueue(Tuple.Create(double.Parse(match.Groups["duration"].Value, CultureInfo.InvariantCulture), match.Groups["file"].Value));
				}

				if (cts.IsCancellationRequested) break;
				var segment=segments.Dequeue();
				DateTime lastOpenTime=DateTime.UtcNow;
				Stream input=client.OpenRead(segment.Item2.StartsWith("http:") ? segment.Item2:folderUrl+segment.Item2);

				AesManaged aes=null; ICryptoTransform decryptor=null;
				if (key != null) {
					byte[] binaryIdx=BitConverter.GetBytes(sequenceIdx);
					Array.Reverse(binaryIdx);
					Buffer.BlockCopy(binaryIdx, 0, iv, 12, 4); // Pirmie baiti ir nulles, pēctam četri baiti sastāv no kārtas numura.
					aes=new AesManaged { Key=key, IV=iv, Mode=CipherMode.CBC };
					decryptor=aes.CreateDecryptor();
					input=new CryptoStream(input, decryptor, CryptoStreamMode.Read);
				}

				// Padod fragmenta saturu klientam.
				while (!cts.IsCancellationRequested && (bytesRead=input.Read(buffer, 0, buffer.Length)) != 0)
					try {
						output.Write(buffer, 0, bytesRead);
					} catch (HttpListenerException) {
						// ex.ErrorCode == 64 (ERROR_NETNAME_DELETED) nozīmē, ka klients godīgi atslēdzies, pārējās kļūdas ir nopietnākas.
						outputEnded=true;
						break;
					}
				input.Dispose();

				if (key != null) { decryptor.Dispose(); aes.Dispose(); }
				sequenceIdx++;
		
				if (outputEnded) break;
				// Gaida, kamēr būs pieejams nākamais fragments.
				while (!cts.IsCancellationRequested && (DateTime.UtcNow-lastOpenTime).TotalSeconds < segment.Item1)
					Thread.Sleep(100);
			} while (!cts.IsCancellationRequested);

			if (listener.IsListening) response.Close();
			segments.Clear();
			listener.Close();
		}

		/// <summary>Pašlaik uztveramo video fragmentu ilgumi sekundēs un failu nosaukumi.</summary>
		private readonly Queue<Tuple<double, string>> segments=new Queue<Tuple<double, string>>(3);
		/// <summary>Attālo datu iegūšanas klients.</summary>
		private readonly ProperWebClient client=new ProperWebClient(Encoding.ASCII);
		/// <summary>HTTP servera process.</summary>
		private HttpListener listener;
		/// <summary>Video servera process.</summary>
		private Task worker;
		/// <summary>Servera proces apstādināšanas pieprasījums.</summary>
		private CancellationTokenSource cts;
		/// <summary>Video fragmenta regulārais izteiciens.</summary>
		private static readonly Regex segmentRx=new Regex(@"#EXTINF:(?'duration'[0-9]+(\.[0-9]+)?),\r?\n(?'file'[^\n\r]+)", RegexOptions.Compiled);
		/// <summary>Šifrešanas atslēgas adreses regulārais izteiciens.</summary>
		private static readonly Regex keyRx=new Regex(@"EXT-X-KEY:METHOD=AES-128,URI=""([^""]+)""", RegexOptions.Compiled);
		/// <summary>Pirmā saraksta fragmenta kārtas numura regulārais izteiciens.</summary>
		private static readonly Regex sequenceRx=new Regex("EXT-X-MEDIA-SEQUENCE:([0-9]+)", RegexOptions.Compiled);
		/// <summary>Fragmentu faila adreses regulārais izteiciens.</summary>
		private readonly Regex segmentsRx;
		/// <summary>Sākotnējā atskaņošanas saraksta adrese.</summary>
		private readonly string playlistUrl;
		/// <summary>Video plūsmas adrese, kuru atvērt atskaņotājā.</summary>
		/// <remarks>Tikai localhost neprasa papildus lietotāja tiesības un darbības, reģistrējot adresi.
		/// Ports var būt brīvi izvēlēts un adresei noteikti jābeidzas ar slīpsvītru.</remarks>
		public const string StreamUrl="http://localhost:1984/video/";
		/// <summary>Video plūsmas datu tips, kuru atgriež klientam.</summary>
		private const string ResponseContentType="video/mp2t";
	}
}