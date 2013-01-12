using System;
using System.Collections.Generic;
using System.Linq;
using DrDax.RadioClient;
using System.ComponentModel.Composition;
using System.Windows.Media;

namespace Misc {
	[Export(typeof(Station))]
	public class MiscStation : Station {
		public MiscStation() : base(new StationChannelList(true) {
			"ХiтFM"
		}) {}

		public override Channel GetChannel(byte number) {
			switch (number) {
				case 1: return new HttpChannel("http://195.95.206.17:80/HitFM", GetResourceImage("HitFM.png"),
					TimeZoneInfo.FindSystemTimeZoneById("E. Europe Standard Time"), Station.UseGuide ? new SimpleIcyGuide():null,
					new Brand(Colors.Black, 0x990000.ToColor(), Colors.White, 0xD11F1B.ToColor(), 0xA30C0B.ToColor(), 0xEFEFEF.ToColor(), Colors.White));
				default: throw new ChannelNotFoundException(number);
			}
		}
	}
}
