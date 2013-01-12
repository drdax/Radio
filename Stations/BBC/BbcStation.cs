using System;
using System.ComponentModel.Composition;
using System.Windows.Media;
using DrDax.RadioClient;

namespace Bbc {
	[Export(typeof(Station))]
	public class BbcStation : Station {
		public BbcStation(): base(new StationChannelList {
			{ 1, "BBC Radio 1" },
			{ 11, "BBC Radio 1Xtra" },
			{ 2, "BBC Radio 2" },
			{ 3, "BBC Radio 3" },
			{ 4, "BBC Radio 4" },
			{ 7, "BBC Radio 4Xtra" },
			{ 5, "BBC Radio 5" },
			{ 15, "BBC Radio 5 Sports xtra" },
			{ 6, "BBC Radio 6 Music" },
			{ 10, "BBC Asian Network" },
			{ 20, "BBC World Service" }
		}, "GMT Standard Time") {} // Londonas laika josla.

		public override Channel GetChannel(byte number) {
			string streamUrl, logoName, radioName;
			Int32 fromColorHex, toColorHex;
			// http://iplayerhelp.external.bbc.co.uk/help/playing_radio_progs/real_wma_streams
			// http://iplayerhelp.external.bbc.co.uk/help/playing_radio_progs/local_radio_streams ar World Service
			switch (number) {
				case 1: streamUrl="mms://wmlive-nonacl.bbc.net.uk/wms/bbc_ami/radio1/radio1_bb_live_int_ep1_sw0";
					logoName="One"; fromColorHex=0x4C4C4C; toColorHex=0x2C2C2C; radioName="radio_one";
					break;
				case 11: streamUrl="mms://wmlive-nonacl.bbc.net.uk/wms/bbc_ami/1xtra/1xtra_bb_live_int_ep1_sl0";
					logoName="1Xtra"; fromColorHex=0x4C4C4C; toColorHex=0x2C2C2C; radioName="1xtra";
					break;
				case 2: streamUrl="mms://wmlive-nonacl.bbc.net.uk/wms/bbc_ami/radio2/radio2_bb_live_int_ep1_sl0";
					logoName="Two"; fromColorHex=0xFFA73E; toColorHex=0xFF5F00; radioName="radio_two";
					break;
				case 3: streamUrl="mms://wmlive-nonacl.bbc.net.uk/wms/bbc_ami/radio3/radio3_bb_live_int_ep1_sl0";
					logoName="Three"; fromColorHex=0xFF6E67; toColorHex=0xF80916; radioName="radio_three";
					break;
				case 4: streamUrl="mms://wmlive-nonacl.bbc.net.uk/wms/bbc_ami/radio4/radio4_bb_live_int_ep1_sl1";
					logoName="Four"; fromColorHex=0x3434A4; toColorHex=0x000057; radioName="radio_four";
					break;
				case 7: streamUrl="mms://wmlive-nonacl.bbc.net.uk/wms/bbc_ami/radio4/radio4xtra_bb_live_int_ep1_sl0";
					logoName="4Xtra"; fromColorHex=0xBC53D9; toColorHex=0x843F97; radioName="radio_four_extra";
					break; // Septiņi nevis četrpadsmit, jo tas ir vēsturiskais numurs.
				case 5: streamUrl="mms://wmlive-nonacl.bbc.net.uk/wms/bbc_ami/radio5/radio5_bb_live_int_ep1_sl0";
					logoName="5Live"; fromColorHex=0x48EBF0; toColorHex=0x1D868A; radioName="radio_five_live";
					break;
				case 15: streamUrl="mms://wmlive-nonacl.bbc.net.uk/wms/bbc_ami/radio5/5spxtra_bb_live_int_ep1_sl0";
					logoName="5Sport"; fromColorHex=0x76E14F; toColorHex=0x289800; radioName="radio_five_live_sports_extra";
					break;
				case 6: streamUrl="mms://wmlive-nonacl.bbc.net.uk/wms/bbc_ami/6music/6music_bb_live_int_ep1_sl0";
					logoName="Six"; fromColorHex=0x4DBFAD; toColorHex=0x178A78; radioName="6music";
					break;
				case 10: streamUrl="mms://wmlive-nonacl.bbc.net.uk/wms/bbc_ami/asiannet/asiannet_bb_live_int_ep1_sl0";
					logoName="Asian"; fromColorHex=0xF75CAB; toColorHex=0xD01876; radioName="asian_network";
					break;
				case 20: streamUrl="mms://a973.l3944038972.c39440.g.lm.akamaistream.net/D/973/39440/v0001/reflector:38972";
					logoName="World"; fromColorHex=0xEF563E; toColorHex=0xC0311A; radioName="world_service";
					break;
				default: throw new ChannelNotFoundException(number);
			}
			return new MmsChannel(streamUrl, GetResourceImage(logoName+".png"), timezone,
				Station.UseGuide ? new BbcGuide(radioName, timezone):null,
				new Brand(Colors.White, 0xF54897.ToColor() /* rozā no BBC iPlayer */, Colors.White,
					new LinearGradientBrush(fromColorHex.ToColor(), toColorHex.ToColor(), 90),
					new LinearGradientBrush(0x020202.ToColor(), 0x2F2F2F.ToColor(), 90)));
		}
	}
}