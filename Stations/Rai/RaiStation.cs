using System;
using System.ComponentModel.Composition;
using System.Windows.Media;
using DrDax.RadioClient;

namespace Rai {
	[Export(typeof(Station))]
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

		public override Channel GetChannel(byte number) {
			string streamUrl, logoName=null, radioName=null;
			Int32 fromColorHex=0, toColorHex=0;
			bool isWebRadio=number >= 6 && number <= 8;
			switch (number) {
				case 1: streamUrl="mms://212.162.68.162/Radio_Uno";
					// http://www.rai.tv/dl/RaiTV/popup/player_radio.html?v=1
					// http://mediapolis.rai.it/relinker/relinkerServlet.htm?cont=162834
					// Alternatīva mms://212.162.68.162/RAI_ITALIA";
					// http://www.rai.tv/dl/RaiTV/popup/player_radio.html?v=8
					// http://mediapolis.rai.it/relinker/relinkerServlet.htm?cont=173887
					logoName="Uno"; fromColorHex=0x3494D2; toColorHex=0x0069AC; radioName="RadioUno";
					break;
				case 2: streamUrl="mms://212.162.68.162/Radio_Due";
					// http://rai.it/dl/RaiTV/popup/player_radio.html?v=2
					// http://mediapolis.rai.it/relinker/relinkerServlet.htm?cont=162063
					logoName="Due"; fromColorHex=0xD92439; toColorHex=0xA70014; radioName="RadioDue";
					break;
				case 3: streamUrl="mms://212.162.68.162/Radio_Tre";
					// http://www.rai.tv/dl/RaiTV/popup/player_radio.html?v=3
					// http://mediapolis.rai.it/relinker/relinkerServlet.htm?cont=162841
					logoName="Tre"; fromColorHex=0x3FBE6F; toColorHex=0x009037; radioName="RadioTre";
					break;
				case 4: //streamUrl="mms://a1745.l6935563744.c69355.g.lm.akamaistream.net/D/1745/69355/v0001/reflector:63744?auth=daEcrb2aDcjd0d_b7bNcBcybaaocabTcTcb-bqn.Q_-c0-HkvuAGw&aifp=V001";
					// http://www.rai.tv/dl/RaiTV/popup/player_radio.html?v=4
					streamUrl="http://mediapolis.rai.it/relinker/relinkerServlet.htm?cont=173799";
					logoName="FD4"; fromColorHex=0xC2B594; toColorHex=0x98875B;
					break;
				case 5: streamUrl="mms://212.162.68.163/FD_V_AUDITORIUM";
					// http://www.rai.tv/dl/RaiTV/popup/player_radio.html?v=5
					// http://mediapolis.rai.it/relinker/relinkerServlet.htm?cont=173832
					logoName="FD5"; fromColorHex=0xC8C8C8; toColorHex=0x7B7B7B;
					break;
				case 6: streamUrl="mms://212.162.68.162/WEBRADIO_1";
					// http://www.rai.tv/dl/RaiTV/popup/player_radio.html?v=9
					// http://mediapolis.rai.it/relinker/relinkerServlet.htm?cont=174078
					break;
				case 7: streamUrl="mms://212.162.68.102/WEBRADIO_2";
					// http://www.rai.tv/dl/RaiTV/popup/player_radio.html?v=10
					// http://mediapolis.rai.it/relinker/relinkerServlet.htm?cont=174083
					break;
				case 8: streamUrl="mms://212.162.68.163/WEBRADIO_3";
					// http://www.rai.tv/dl/RaiTV/popup/player_radio.html?v=11
					// http://mediapolis.rai.it/relinker/relinkerServlet.htm?cont=174086
					break;
				case 9: streamUrl="mms://212.162.68.163/ISORADIO";
					// http://www.rai.tv/dl/RaiTV/popup/player_radio.html?v=6
					// http://mediapolis.rai.it/relinker/relinkerServlet.htm?cont=173875
					logoName="Iso"; fromColorHex=0x53B0BC; toColorHex=0x167E8C; //radioName="radio_three";
					break;
				case 10: streamUrl="mms://212.162.68.163/RADIO_PARLAMENTO";
					// http://www.rai.tv/dl/RaiTV/popup/player_radio.html?v=7
					// http://mediapolis.rai.it/relinker/relinkerServlet.htm?cont=173879
					logoName="Parlamento"; fromColorHex=0xAF2B47; toColorHex=0x760D24; radioName="GrParlamento";
					break;
				default: throw new ChannelNotFoundException(number);
			}
			if (isWebRadio) {
				logoName="Web"+number; fromColorHex=0x806BB3; toColorHex=0x584099; radioName="WebRadio"+number;
			}
			Guide guide=null;
			if (Station.UseGuide) {
				if (radioName != null) guide=new RaiListedGuide(isWebRadio, radioName, timezone);
				else if (number == 4 || number == 5) guide=new RaiPollingGuide(number);
			}
			return new MmsChannel(streamUrl, GetResourceImage(logoName+".png"), timezone, guide,
				new Brand(Colors.White, fromColorHex.ToColor(), Colors.White,
					fromColorHex.ToColor(), toColorHex.ToColor(), 0x202020.ToColor(), 0x202020.ToColor()));
		}
	}
}