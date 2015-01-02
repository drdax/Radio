using DrDax.RadioClient;

namespace Eurovision {
	public class ChannelMenu : Menu<Channel> {
		public ChannelMenu() : base(new MenuItemList {
			{ MenuIcon.Playlist, "Playlist" }
		}) {}

		public override void HandleCommand(int itemIndex) {
			DefaultProgram.OpenPage("http://www.escradio.com/playlist/");
		}
	}
}