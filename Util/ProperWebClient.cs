using System;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace DrDax.RadioClient {
	/// <summary>
	/// <see cref="WebClient"/>, kurš atbilst Radio programmas prasībām.
	/// </summary>
	/// <remarks>Vienmēr ir jāizmanto šī klase vai tās mantinieki, jo tā ņem vērā lietotāja iestatījumus.</remarks>
	public class ProperWebClient : WebClient {
		private readonly bool decompressResponse;
		public ProperWebClient() : this(Encoding.UTF8, false) {}
		/// <param name="encoding">Lejuplādējamā teksta kodu tabula. Pēc noklusējuma UTF8.</param>
		/// <param name="decompressResponse">Vai automātiski atspiest GZip HTTP atbildi.</param>
		public ProperWebClient(Encoding encoding, bool decompressResponse=false) {
			this.decompressResponse=decompressResponse;
			if (!Settings.Default.UseSystemProxy) this.Proxy=null;
			this.Encoding=encoding;
			Settings.Default.ProxyChanged+=Settings_ProxyChanged;
		}

		/// <param name="url">JSON dokumenta interneta adrese.</param>
		/// <returns>LINQ XML, kurš asinhroni iegūts no JSONa noklusējuma (UTF-8) kodējumā.</returns>
		public async Task<XElement> GetJson(string url) {
			var json=XDocument.Load(JsonReaderWriterFactory.CreateJsonReader(
				await this.DownloadDataTaskAsync(url),
				XmlDictionaryReaderQuotas.Max)).Root;
			System.Diagnostics.Debug.WriteLine(json.ToString());
			return json;
		}
		/// <param name="url">JSON dokumenta interneta adrese.</param>
		/// <returns>LINQ XML, kurš sinhroni iegūts no JSONa noklusējuma (UTF-8) kodējumā.</returns>
		public XElement GetJsonSync(string url) {
			// Ja ir vēlme izsaukt asinrono metodi, tad jālieto konstrukcija Task.Factory.StartNew(() => client.GetJson(url).Result).Result, sāvādāk GetJson.Result vai Wait() nobloķējas.
			var json=XDocument.Load(JsonReaderWriterFactory.CreateJsonReader(this.DownloadData(url), XmlDictionaryReaderQuotas.Max)).Root;
			System.Diagnostics.Debug.WriteLine(json.ToString());
			return json;
		}
		/// <param name="url">JSON dokumenta interneta adrese.</param>
		/// <returns>LINQ XML, kurš iegūts no JSONa specifiskā kodējumā (<see cref="Encoding"/>).</returns>
		public async Task<XElement> GetEncodedJson(string url) {
			var json=XDocument.Load(JsonReaderWriterFactory.CreateJsonReader(
				// Tāda ņemšanās ar parkodēšanu, jo JSON lasītājs atbalsta tikai Unicode paveidus.
				Encoding.UTF8.GetBytes(await this.DownloadStringTaskAsync(url)),
				XmlDictionaryReaderQuotas.Max)).Root;
			System.Diagnostics.Debug.WriteLine(json.ToString());
			return json;
		}

		private void Settings_ProxyChanged(bool useSystemProxy) {
			this.Proxy=useSystemProxy ? WebRequest.GetSystemWebProxy():null;
		}
		protected override WebRequest GetWebRequest(Uri address) {
			HttpWebRequest request=base.GetWebRequest(address) as HttpWebRequest;
			if (decompressResponse)
				request.AutomaticDecompression=DecompressionMethods.GZip;// DecompressionMethods.Deflate ir gana rets, lai ignorētu.
			return request;
		}
		protected override void Dispose(bool disposing) {
			base.Dispose(disposing);
			Settings.Default.ProxyChanged-=Settings_ProxyChanged;
		}
	}
}