using System.ComponentModel.Composition;
using System.Windows.Media;
using DrDax.RadioClient;

namespace Riga {
	[Export(typeof(Station))]
	public class RigaStation : Station {
		public RigaStation() : base(new StationChannelList(true) {
			"Radio 101",
			"Capital FM",
			"Latvijas Kristīgais radio",
			"Rīga Radio",
			"Skonto",
			"Star FM",
			"Top Radio"
		}, "E. Europe Standard Time") { } // Latvijas laika josla.

		public override Channel GetChannel(byte number) {
			switch (number) {
				case 1:return new HttpChannel("http://a.radio101.lv:80/mp3-hq", GetResourceImage("101.png"), timezone, Station.UseGuide ? new SimpleIcyGuide(true):null,
					new Brand(0x666666.ToColor(), 0x28ACE2.ToColor(), Colors.White, 0x231F20.ToColor(), 0x231F20.ToColor(), Colors.White));
				case 2: return new IcyHttpChannel("http://radio2.capitalfm.lv:8000/", GetResourceImage("Capital.png"), timezone,
					Station.UseGuide ? new SimpleIcyGuide():null, // Alternatīva http://www.capitalfm.lv/script/onair/ reizi piecās sekundēs.
					new Brand(0x353434.ToColor(), 0xCC0000.ToColor(), Colors.White,
						0xCC0000.ToColor(), 0x7A0000.ToColor(), Colors.White, Colors.White));
				case 3:return new IcyHttpChannel("http://91.203.71.10:8006/", GetResourceImage("LKR.png"), timezone, Station.UseGuide ? new KristigaisGuide(timezone):null,
					new Brand(Colors.White, 0xBDBDBD.ToColor(), Colors.White, 0x621C3D.ToColor(), 0x621C3D.ToColor(), 0x9A5E74.ToColor()));
				case 4: return new HttpChannel("http://r.rigaradio.lv:8000/mp3", GetResourceImage("Riga.png"), timezone, Station.UseGuide ? new SimpleIcyGuide():null,
					new Brand(0x666666.ToColor(), 0xDC3D5A.ToColor(), 0xF9F9F9.ToColor(),
						new LinearGradientBrush(
							new GradientStopCollection {
								new GradientStop(0xF8C622.ToColor(), 0.25),
								new GradientStop(0x84BF41.ToColor(), 0.25), new GradientStop(0x84BF41.ToColor(), 0.5),
								new GradientStop(0x00A8DC.ToColor(), 0.5), new GradientStop(0x00A8DC.ToColor(), 0.75),
								new GradientStop(0xA43C85.ToColor(), 0.75)
							}, 0),
						Brushes.White
					));
				case 5: return new HttpChannel("http://skonto.ls.lv:8002/mp3",
					GetResourceImage("Skonto.png"), timezone, null, // http://radioskonto.lv/online_radio/now_play.php?tmp={RAND} ir jābūt dziesmas datiem, bet to nav.
					new Brand(0x333333.ToColor(), 0xD50000.ToColor(), Colors.White,
						0xED3B33.ToColor(), 0xA82A24.ToColor(), 0xFCFCFC.ToColor(), 0xFCFCFC.ToColor()));
				case 6: return new HttpChannel(true, "http://starfm.deac.lv:1935/live/starfm/",
					GetResourceImage("Star.png"), timezone, Station.UseGuide ? new StarGuide():null,
					new Brand(0xF6D80B.ToColor(), 0x8DD8F8.ToColor(), 0x8DD8F8.ToColor(),
							0x003652.ToColor(), 0x0488B6.ToColor(), 0x0488B6.ToColor(), 0x0488B6.ToColor()));
				case 7: return new IcyHttpChannel("http://195.13.200.164:8000/", GetResourceImage("Top.png"), timezone, Station.UseGuide ? new TopGuide(timezone):null,
					new Brand(Colors.Black, 0xE0001D.ToColor(), Colors.White,
						new LinearGradientBrush(0x0080C1.ToColor(), 0x0077BA.ToColor(), 90),
						new SolidColorBrush(0xFAFAFA.ToColor()),
						new LinearGradientBrush(
							new GradientStopCollection {
								new GradientStop(0xFFDA00.ToColor(), 0),
								new GradientStop(0xF1F200.ToColor(), 0.15),
								new GradientStop(0xFFDA00.ToColor(), 1)
							}, 90)
					));
				default: throw new ChannelNotFoundException(number);
			}
		}
	}
}