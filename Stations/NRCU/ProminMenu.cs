using DrDax.RadioClient;

namespace Nrcu {
	public class ProminMenu : Menu<Channel> {
		public ProminMenu() : base(new MenuItemList {
			{ MenuIcon.Video, "Веб-камера у студії" }
		}) {}
		public override void HandleCommand(int itemIndex) {
			ProminWindow.Open();
		}
	}
}