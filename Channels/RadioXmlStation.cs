using System;
using System.Collections.Generic;
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
		public override Channel GetChannel(uint number) {
			if (radioDoc == null) return null;
			var channelX=radioDoc.Descendants("Channel").ElementAtOrDefault((int)number);
			if (channelX == null) throw new ChannelNotFoundException(number);
			BitmapImage logo; TimeZoneInfo timezone;
			string logoId=channelX.Attribute("LogoId") != null ? channelX.Attribute("LogoId").Value:null;
			if (logoId == null) logo=null;
			else try {
					logo=new BitmapImage();
					logo.BeginInit();
					logo.CacheOption=BitmapCacheOption.OnLoad;
					logo.StreamSource=new MemoryStream(Convert.FromBase64String(radioDoc.Descendants("Logo").First(l => l.Attribute("Id").Value == logoId).Value));
					logo.EndInit();
				} catch { logo=null; }
			try {
				timezone=TimeZoneInfo.FindSystemTimeZoneById(channelX.Attribute("Time").Value+" Standard Time");
			} catch { timezone=null; }
			var urlAttribute=channelX.Attribute("Url");
			if (urlAttribute != null) return new UrlChannel(urlAttribute.Value, logo, timezone, false, null, null);
			urlAttribute=channelX.Attribute("Icy");
			if (urlAttribute != null) return new ForcedIcyChannel(urlAttribute.Value, logo, timezone, false, null, null);
			urlAttribute=channelX.Attribute("IcyGuide");
			if (urlAttribute != null) return new ForcedIcyChannel(urlAttribute.Value, logo, timezone, true, null, null);
			throw new ChannelNotFoundException(number);
		}
		public override Guide GetGuide(uint channelNumber) {
			return new SimpleIcyGuide();
		}
		public override string GetHomepage(uint number) {
			var channelX=radioDoc.Descendants("Channel").ElementAtOrDefault((int)number);
			var urlAttribute=channelX.Attribute("Homepage");
			if (urlAttribute == null) return null;
			return urlAttribute.Value;
		}
	}
}