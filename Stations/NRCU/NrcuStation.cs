using DrDax.RadioClient;

namespace Nrcu {
	public class NrcuStation : Station {
		public NrcuStation() : base(new StationChannelList {
			"Перший канал",
			"Другий канал «Промiнь»",
			"Третій канал «Культура»",
			"Всесвітня служба «Радіомовлення України»"
		}, "E. Europe Standard Time") {} // Ukrainas laika josla.

		public override Channel GetChannel(uint number) {
			if (number == 0 || number > 4) throw new ChannelNotFoundException(number);
			if (brand == null)
				brand=new Brand(0x003F8F.ToColor(), 0x005CAA.ToColor(), 0x003F8F.ToColor(), 0x005CAA.ToColor(),
					0xD6F5FF.ToColor(), 0xA8EAFF.ToColor(), 0xA8EAFF.ToColor(), 0xFCEEAB.ToColor());
			return new IcyChannel(string.Concat("http://nrcu.gov.ua:8000/ur", number, "-mp3"),
				GetResourceImage(number == 2 ? "Promin.png":"UkrRadio.png"),
				timezone, true, brand, number == 2 ? new ProminMenu():null);
		}
		public override Guide GetGuide(uint number) {
			return new NrcuGuide(number, timezone);
		}
		public override string GetHomepage(uint channelNumber) {
			switch (channelNumber) {
				case 1: return "http://www.nrcu.gov.ua/ua/11";
				case 2: return "http://www.promin.fm/";
				case 3: return "http://radiokultura.org/";
				case 4: return "http://www.nrcu.gov.ua/ua/17";
			}
			return null;
		}

		private Brand brand;
	}
}