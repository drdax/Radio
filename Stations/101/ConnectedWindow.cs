using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using DrDax.RadioClient;

namespace Ru101 {
	/// <summary>Logs ar pieslēgumu JSON API.</summary>
	public abstract class ConnectedWindow : ProperWindow {
		protected async Task<XElement> GetJson(string url) {
			// {"status":0,"result":[ ... ],"errorCode":0,"errorMsg":"","allcount":"48"}
			return (await client.GetJson(url)).Element("result");
		}

		// Kanālu datu kodu tabula ir ASCII ar \uXXXX pierakstu. Bet tā kā datus lasa bināri, to šeit var neiestatīt.
		private readonly ProperWebClient client=new ProperWebClient();
	}
}