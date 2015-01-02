using System.Windows;
using DrDax.RadioClient;

namespace Nrcu {
	public class GuideMenu : Menu<Guide> {
		public GuideMenu() : base(new MenuItemList { "Сторiнка ведучего" }) {}

		public override void HandleCommand(int itemIndex) {
			int id=((NrcuBroadcast)Source.CurrentBroadcast).PresenterId;
			if (id != 0)
				DefaultProgram.OpenPage("http://www.promin.fm/staff.html?id="+id);
			else MessageBox.Show("Сторiнка ведучего вiдсутня");
		}
	}
}