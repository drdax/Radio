using System.Text.RegularExpressions;
using DrDax.RadioClient;

namespace Echo {
	public class ChannelMenu : Menu<Channel> {
		public ChannelMenu() : base(new MenuItemList {
			{ MenuIcon.Video, "Сетевизор" }
		}) {}

		public override void HandleCommand(int itemIndex) {
			// Atskaņotājs atrodas lapā http://echo.msk.ru/set/, kura ielāde sekojošo adresi. Flash versijā ar RTMP protokolu ir pieejamas vairākas plūsmas, bet M3U8 variantā tās visas saliktas vienā kadrā (dažreiz kadrs var attēlot arī vienu plūsmu).
			string sessionId; // Adresē mainās sesijas identifikators, tāpēc to katru reizi jāpārlādē.
			// Lai gan no lappuses var ņemt pilnu adresi, kopš reizes tā ir mainījusies Flash klientam, neatbilst patiesībai M3U8 versijai, tāpēc pagrābj tikai sesijas numuru.
			using (var client=new ProperWebClient())
				sessionId=Regex.Match(client.DownloadString("http://echomsk.onlinetv.ru/widget/live/echomsk.html"), @"\.m3u8\?s=[a-z0-9]+").Value;
			StudioWindow.Open(Items[0],
				"http://prague1.setevisor.tv:1935/echomsk/_definst_/echomsk.stream/playlist"+sessionId,
				640, 480, @"playlist\.m3u8\?[^\n]+"); // Aiz jautājumzīmes wowzasessionid un tā pati drošības sesija.
		}
	}
}