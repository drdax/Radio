using System;
using System.Windows.Media;
using DrDax.RadioClient;

namespace Rai {
	public class RaiStation : Station {
		public RaiStation() : base(new StationChannelList {
			"Rai radio 1",
			"Rai radio 2",
			"Rai radio 3",
			"Filodiffusione 4",
			"Filodiffusione 5",
			"Web radio 6",
			"Web radio 7",
			"Web radio 8",
			"Iso radio",
			"Gr Parlamento"
		}, "Central Europe Standard Time") {} // Itālijas laika josla.

		public override Channel GetChannel(uint number) {
			string streamUrl, logoName=null;
			Int32 fromColorHex=0, toColorHex=0;
			bool isWebRadio=number >= 6 && number <= 8, hasGuide=true; // Dažiem WebRadio ir pilnīgi tukša raidījumu lapa.
			// JSON ar adresēm, no kura atlasītas Android domātās http://rai.it/dl/portaleRadio/popup/ContentSet-003728e4-db46-4df8-83ff-606426c0b3f5-json.html
			switch (number) {
				case 1: streamUrl="http://mediapolis.rai.it/relinker/relinkerServlet.htm?cont=152946";
					logoName="Uno"; fromColorHex=0x3494D2; toColorHex=0x0069AC;
					break;
				case 2: streamUrl="http://mediapolis.rai.it/relinker/relinkerServlet.htm?cont=153172";
					logoName="Due"; fromColorHex=0xD92439; toColorHex=0xA70014;
					break;
				case 3: streamUrl="http://mediapolis.rai.it/relinker/relinkerServlet.htm?cont=153177";
					logoName="Tre"; fromColorHex=0x3FBE6F; toColorHex=0x009037;
					break;
				case 4: streamUrl="http://mediapolis.rai.it/relinker/relinkerServlet.htm?cont=153308";
					logoName="FD4"; fromColorHex=0xC2B594; toColorHex=0x98875B;
					break;
				case 5: streamUrl="http://mediapolis.rai.it/relinker/relinkerServlet.htm?cont=153311";
					logoName="FD5"; fromColorHex=0xC8C8C8; toColorHex=0x7B7B7B;
					break;
				case 6: streamUrl="http://mediapolis.rai.it/relinker/relinkerServlet.htm?cont=364598";
					break;
				case 7: streamUrl="http://mediapolis.rai.it/relinker/relinkerServlet.htm?cont=364608";
					hasGuide=false;
					break;
				case 8: streamUrl="http://mediapolis.rai.it/relinker/relinkerServlet.htm?cont=364611";
					hasGuide=false;
					break;
				case 9: streamUrl="http://mediapolis.rai.it/relinker/relinkerServlet.htm?cont=153315";
					logoName="Iso"; fromColorHex=0x53B0BC; toColorHex=0x167E8C;
					break;
				case 10: streamUrl="http://mediapolis.rai.it/relinker/relinkerServlet.htm?cont=153316";
					logoName="Parlamento"; fromColorHex=0xAF2B47; toColorHex=0x760D24;
					break;
				default: throw new ChannelNotFoundException(number);
			}
			if (isWebRadio) {
				logoName="Web"+number; fromColorHex=0x806BB3; toColorHex=0x584099;
			}
			return new UrlChannel(streamUrl, GetResourceImage(logoName+".png"), timezone, hasGuide,
				new Brand(Colors.White, fromColorHex.ToColor(), Colors.White, toColorHex.ToColor(),
					fromColorHex.ToColor(), toColorHex.ToColor(), 0x202020.ToColor(), 0x202020.ToColor()));
		}
		public override Guide GetGuide(uint number) {
			string radioName=null;
			bool isWebRadio=false;
			switch (number) {
				case 1: radioName="RadioUno"; break;
				case 2: radioName="RadioDue"; break;
				case 3: radioName="RadioTre"; break;
				case 4: isWebRadio=true; radioName="Fd4"; break;
				case 6: isWebRadio=true; radioName="WebRadio"+number; break;
				case 9: radioName="Isoradio"; break;
				case 10: radioName="GrParlamento"; break;
			}
			if (isWebRadio) return new RaiWebListedGuide(radioName, timezone);
			if (radioName != null) return new RaiListedGuide(radioName, timezone);
			else if (number == 5) return new RaiPollingGuide(number);
			throw new ChannelNotFoundException(number);
		}
		public override string GetHomepage(uint number) {
			switch (number) {
				case 1: return "http://www.radio1.rai.it/";
				case 2: return "http://www.radio2.rai.it/";
				case 3: return "http://www.radio3.rai.it/";
				case 4: return "http://www.fd4.rai.it/";
				case 5: return "http://www.fd5.rai.it/";
				case 6: return "http://www.wr6.rai.it/";
				case 7: return "http://www.wr7.rai.it/";
				case 8: return "http://www.wr8.rai.it/";
				case 9: return "http://www.isoradio.rai.it/";
				default: return "http://www.grparlamento.rai.it/";
			}
		}
	}
}