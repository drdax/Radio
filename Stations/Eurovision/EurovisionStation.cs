using System;
using System.ComponentModel.Composition;
using System.Windows.Media;
using DrDax.RadioClient;

namespace Eurovision {
	[Export(typeof(Station))]
	public class EurovisionStation : Station {
		public EurovisionStation() : base(new StationChannelList(true) {
			"Eurovision"
		}) {}

		public override Channel GetChannel(byte number) {
			if (number != 1) throw new ChannelNotFoundException(number);
			return new IcyHttpChannel("http://stream.escradio.com:8030/", GetResourceImage("ESC.png"),
			TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time"), Station.UseGuide ? new EurovisionGuide():null,
			new Brand(0xEEEEEE.ToColor(), 0x6BB6DE.ToColor(), 0xF7F7F7.ToColor(),
				new LinearGradientBrush(
					new GradientStopCollection {
						new GradientStop(0xD65A54.ToColor(), 0),
						new GradientStop(0xCC413B.ToColor(), 0.5),
						new GradientStop(0xB73A34.ToColor(), 0.5),
						new GradientStop(0x9E2F29.ToColor(), 1)
					}, 90),
				new ImageBrush(GetResourceImage("ESCbackground.jpg")) { Stretch=Stretch.None }));
		}
	}
}