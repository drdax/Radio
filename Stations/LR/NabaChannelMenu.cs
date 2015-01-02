using DrDax.RadioClient;

namespace Lr {
	public class NabaChannelMenu : Menu<Channel> {
		public NabaChannelMenu() : base(new MenuItemList {
			{ MenuIcon.Playlist, "Spēlētās dziesmas" }
		}) {}
		public override void HandleCommand(int itemIndex) {
			DefaultProgram.OpenPage("http://www.naba.lv/playlist/");
		}
	}
}