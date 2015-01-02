using DrDax.RadioClient;

namespace Vgtrk {
	public class MayakMenu : Menu<Channel> {
		public MayakMenu() : base(new MenuItemList {
			{ MenuIcon.Video, "Видео из студии" }
		}) {}

		public override void HandleCommand(int itemIndex) {
			// Majakam ir triju kvalitāšu plūsmas, paņem labāko. Aiz jautājuma zīmes nāk tie paši parametri, kuri ir playlist adresē, bet to secība lēkā.
			StudioWindow.Open(Items[0], "http://testlivestream.rfn.ru/live/smil:mayak.smil/playlist.m3u8?auth=vh&cast_id=81", 768, 576, @"chunklist_b1600000\.m3u8\?[^\n]+");
		}
	}
}