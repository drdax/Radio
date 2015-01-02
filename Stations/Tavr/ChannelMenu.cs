using DrDax.RadioClient;

namespace Tavr {
	public class ChannelMenu : Menu<Channel> {
		public ChannelMenu() : base(new MenuItemList {
			{ MenuIcon.Playlist, "Раньше звучали" }
		}) {}

		public override void HandleCommand(int itemIndex) {
			string url=null;
			// Apakšstacijām nav pilnas dienas atskaņošanas saraksta.
			switch (Source.Number) {
				case 1: url="http://www.radiomelodia.ua/playlist/"; break;
				case 2: url="http://www.rusradio.ua/playlist/"; break;

				case 3: url="http://www.hitfm.ua/playlist/"; break;
				case 4: url="http://www.hitfm.ua/player/happy/"; break;
				case 5: url="http://www.hitfm.ua/player/mj/"; break;
				case 6: url="http://www.hitfm.ua/player/romantic/"; break;
				case 7: url="http://www.hitfm.ua/player/disco/"; break;
				case 8: url="http://www.hitfm.ua/player/hits/"; break;

				case 9: url="http://www.kissfm.ua/playlist/"; break;

				case 13: url="http://www.radioroks.ua/playlist/"; break;
				case 14: url="http://www.radioroks.ua/player/kamtugeza/"; break;
				case 15: url="http://www.radioroks.ua/player/ballad/"; break;
				case 16: url="http://www.radioroks.ua/player/concert/"; break;
				case 17: url="http://www.radioroks.ua/player/ukr/"; break;
				case 18: url="http://www.radioroks.ua/player/beatles/"; break;
				case 19: url="http://www.radioroks.ua/player/hard/"; break;

				case 20: url="http://www.radiorelax.ua/playlist/"; break;
				// case 21: url="http://www.radiorelax.ua/player/nature/"; Dabas skaņām nav saraksta.
				case 22: url="http://www.radiorelax.ua/player/instrumental/"; break;
			}
			DefaultProgram.OpenPage(url);
		}
	}
}