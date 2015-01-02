using System;
using System.Windows;
using System.Windows.Media;
using DrDax.RadioClient;

namespace Bbc {
	public class BbcStation : Station {
		public BbcStation(): base(new StationChannelList {
			{ 1, "BBC Radio 1" },
			{ 11,"BBC Radio 1Xtra" },
			{ 2, "BBC Radio 2" },
			{ 3, "BBC Radio 3" },
			{ 4, "BBC Radio 4" },
			{ 7, "BBC Radio 4Xtra" },
			{ 5, "BBC Radio 5 Live" },
			{ 15,"BBC Radio 5 Sports xtra" },
			{ 6, "BBC Radio 6 Music" },
			{ 10,"BBC Asian Network" },
			{ 20,"BBC World Service" }
		}, "GMT Standard Time") {} // Londonas laika josla.

		public override Channel GetChannel(uint number) {
			string streamUrl, logoName;
			Int32 fromHex, toHex, backgroundHex, foregroundHex;
			// http://iplayerhelp.external.bbc.co.uk/help/playing_radio_progs/real_wma_streams
			// http://iplayerhelp.external.bbc.co.uk/help/playing_radio_progs/local_radio_streams ar World Service
			switch (number) {
				case 1:
				case 11:
					if (number == 1) {
						streamUrl="mms://wmlive-nonacl.bbc.net.uk/wms/bbc_ami/radio1/radio1_bb_live_int_ep1_sw0";
						logoName="One";
					} else {
						streamUrl="mms://wmlive-nonacl.bbc.net.uk/wms/bbc_ami/1xtra/1xtra_bb_live_int_ep1_sl0";
						logoName="1Xtra";
					}
					// 1 un 1xtra fonu krāsa mainās vairākas reizes dienā. fromHex ir no logotipa pogas fona šīm stacijām, toHex ir tām pašām neizvēlētām pogām.
					backgroundHex=0x1E1E1E; foregroundHex=0xEEEEEE; fromHex=0x212121; toHex=0x3F3F3F;
					break;
				case 2: streamUrl="mms://wmlive-nonacl.bbc.net.uk/wms/bbc_ami/radio2/radio2_bb_live_int_ep1_sl0";
					logoName="Two"; backgroundHex=0x0073CF; foregroundHex=0xF3F7FF; fromHex=0xBB5C1A; toHex=0xFEC93D;
					break;
				case 3: streamUrl="mms://wmlive-nonacl.bbc.net.uk/wms/bbc_ami/radio3/radio3_bb_live_int_ep1_sl0";
					logoName="Three"; backgroundHex=0xCD1A29; foregroundHex=0xEBDDDD; fromHex=0x7E0B10; toHex=0xDD1923;
					break;
				case 4: streamUrl="mms://wmlive-nonacl.bbc.net.uk/wms/bbc_ami/radio4/radio4_bb_live_int_ep1_sl1";
					logoName="Four"; backgroundHex=0x003366; foregroundHex=0xFCD164; fromHex=0x0C336A; toHex=0x2C5487;
					break;
				case 7: streamUrl="mms://wmlive-nonacl.bbc.net.uk/wms/bbc_ami/radio4/radio4xtra_bb_live_int_ep1_sl0";
					logoName="4Xtra"; backgroundHex=0x813F97; foregroundHex=0xBBD5D4; fromHex=0x312533; toHex=0x53395C;
					break; // Septiņi nevis četrpadsmit, jo tas ir vēsturiskais numurs.
				case 5: streamUrl="mms://wmlive-nonacl.bbc.net.uk/wms/bbc_ami/radio5/radio5_bb_live_int_ep1_sl0"; // Šim ir arī video plūsma, kura pieejama tikai Lielbritānijā.
					logoName="Five"; backgroundHex=0x135B72; foregroundHex=0xDAEFFF; fromHex=0x0A4553; toHex=0x0F7E9A;
					break;
				case 15: streamUrl="mms://wmlive-nonacl.bbc.net.uk/wms/bbc_ami/radio5/5spxtra_bb_live_int_ep1_sl0";
					logoName="Five"; backgroundHex=0x669933; foregroundHex=0xDCFFBA; fromHex=0x08320C; toHex=0x3A7A1A; // foreground jābūt 0x193C23, bet tas slikti lasās.
					break;
				case 6: streamUrl="mms://wmlive-nonacl.bbc.net.uk/wms/bbc_ami/6music/6music_bb_live_int_ep1_sl0";
					logoName="Six"; backgroundHex=0xD85215; foregroundHex=0xDADECC; fromHex=0x0B343A; toHex=0x256068;
					break;
				case 10: streamUrl="mms://wmlive-nonacl.bbc.net.uk/wms/bbc_ami/asiannet/asiannet_bb_live_int_ep1_sl0";
					logoName="Asian"; backgroundHex=0xD10074; foregroundHex=0xFEF0E0; fromHex=0x44082A; toHex=0x80074E;
					break;
				case 20: streamUrl="mms://a973.l3944038972.c39440.g.lm.akamaistream.net/D/973/39440/v0001/reflector:38972";
					logoName="World"; backgroundHex=0x991B1E; foregroundHex=0xFFFFFF; fromHex=0x431110; toHex=0xB30303;
					break;
				default: throw new ChannelNotFoundException(number);
			}
			Point center=number == 20 ? new Point(104, 110):new Point(146, 86);
			return new UrlChannel(streamUrl, GetResourceImage(logoName+".png"), timezone, true,
				new Brand(Colors.White, foregroundHex.ToColor(), foregroundHex.ToColor(), 0x212121.ToColor(),
					new SolidColorBrush(backgroundHex.ToColor()),
					new RadialGradientBrush(toHex.ToColor(), fromHex.ToColor()) {
						MappingMode=BrushMappingMode.Absolute, Center=center, RadiusX=340, RadiusY=340, GradientOrigin=center
					}));
		}
		public override Guide GetGuide(uint number) {
			return new BbcGuide(string.Concat(GetHomepage(number), "/programmes/schedules/",
				number == 1 ? "england/":(number == 4 ? "fm/":null)), timezone);
		}
		public override string GetHomepage(uint number) {
			const string domain="http://www.bbc.co.uk/";
			switch (number) {
				case  1: return domain+"radio1";
				case 11: return domain+"1xtra";
				case  2: return domain+"radio2";
				case  3: return domain+"radio3";
				case  4: return domain+"radio4";
				case  7: return domain+"radio4extra";
				case  5: return domain+"5live";
				case 15: return domain+"5livesportsextra";
				case  6: return domain+"6music";
				case 10: return domain+"asiannetwork";
				case 20: return domain+"worldserviceradio";
				default: return null;
			}
		}
	}
}