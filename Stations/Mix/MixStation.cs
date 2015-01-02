using System.Windows.Media;
using DrDax.RadioClient;

namespace Mix {
	public class MixStation : Station {
		public MixStation() : base(new StationChannelList {
			"Europa+",
			"MixFM",
			"Radio Baltkom",
			"Юмор FM"
		}, "E. Europe Standard Time") {} // Latvijas laika josla.

		public override Channel GetChannel(uint number) {
			if (brand == null) brand=new Brand(Colors.Black, 0xAC040B.ToColor(), Colors.White, Colors.Black, new SolidColorBrush(0xAB040B.ToColor()), Brushes.White);
			string name; // Nosaukums ir reģistrjūtīgs.
			switch (number) {
				case 1: name="995"; break;
				case 2: name="MixFM"; break;
				case 3: name="baltkom"; break;
				case 4: name="GoldFM"; break;
				default: throw new ChannelNotFoundException(number);
			}
			return new UrlChannel("http://91.90.255.111:80/"+name, GetResourceImage(name+".png"), timezone, false, brand, number == 2 || number == 3 ? new ChannelMenu():null);
		}
		public override Guide GetGuide(uint number) {
			return null;
		}
		public override string GetHomepage(uint number) {
			return "http://www.mixnews.lv/";
		}

		private Brand brand;
	}
}