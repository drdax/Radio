using System.Windows;
using DrDax.RadioClient;

namespace Tavr {
	public class HitGuideMenu : Menu<Guide> {
		public HitGuideMenu() : base(new MenuItemList {
			{ MenuIcon.Information, "Текст и перевод" }
		}) {}

		public override void HandleCommand(int itemIndex) {
			string url=((HitBroadcast)Source.CurrentBroadcast).PageUrl;
			if (url != null)
				DefaultProgram.OpenPage(url);
			else MessageBox.Show("Текст песни недоступен");
		}
	}
}