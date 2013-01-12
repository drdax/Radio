using System;
using System.Net;
using System.Text;

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
			if (!Properties.Settings.Default.UseSystemProxy) this.Proxy=null;
			this.Encoding=encoding;
		}
		protected override WebRequest GetWebRequest(Uri address) {
			HttpWebRequest request=base.GetWebRequest(address) as HttpWebRequest;
			if (decompressResponse)
				request.AutomaticDecompression=DecompressionMethods.GZip;// DecompressionMethods.Deflate ir gana rets, lai ignorētu.
			return request;
		}
	}
}