using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace DrDax.RadioClient {
	/// <summary>
	/// Raidstacija, kuras kanāli aprakstīti XML failā, kā tas bija pirmajā Radio versijā.
	/// </summary>
	/// <remarks>Otrajā versijā nāca klāt Time atribūta (laika joslas nosaukums) atbalsts.</remarks>
	[Export(typeof(Station))]
	public class RadioXmlStation : XmlStation {
		public RadioXmlStation() : base("Radio.xml") {}

		/// <param name="number">Channel elementa kārtas numurs sākot ar nulli.</param>
		public override Channel GetChannel(byte number) {
			var channelX=radioDoc.Descendants("Channel").ElementAtOrDefault(number);
			if (channelX == null) throw new ChannelNotFoundException(number);
			string url=channelX.Attribute("Url").Value;
			Channel channel; BitmapImage logo; TimeZoneInfo timezone;
			try {
				logo=new BitmapImage();
				logo.BeginInit();
				logo.CacheOption=BitmapCacheOption.OnLoad;
				logo.StreamSource=new MemoryStream(Convert.FromBase64String(radioDoc.Descendants("Logo").First(l => l.Attribute("Id").Value == channelX.Attribute("LogoId").Value).Value));
				logo.EndInit();
			} catch { logo=null; }
			try {
				timezone=TimeZoneInfo.FindSystemTimeZoneById(channelX.Attribute("Time").Value+" Standard Time");
			} catch { timezone=null; }
			if (url.StartsWith("mms")) channel=new MmsChannel(url, logo, timezone, null, null);
			else if (url.StartsWith("http")) channel=new IcyHttpChannel(url, logo, timezone, null, null); // "Icy", lai atbalstītu plašāku kanālu loku.
			else channel=null;
			return channel;
		}
	}
}