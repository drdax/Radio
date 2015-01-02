using System;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace DrDax.RadioClient {
	/// <summary>
	/// ShoutCast kanāls, kurš lieto "ICY" vārdu "HTTP" vietā plūsmas sākumā.
	/// </summary>
	public class ForcedIcyChannel : IcyChannel {
		public ForcedIcyChannel(string url, BitmapSource logo, TimeZoneInfo timezone, bool hasGuide, Brand brand, Menu<Channel> menu=null)
			: base(url, logo, timezone, hasGuide, brand, menu) {
			UserAgent="Winamp/2.x"; // Ja UserAgent būs līdzīgs pārlūkprogrammai, tad ShoutCast serveris atgriezīs HTML nevis MP3.
			EnableUnsafeHeaderParsing();
		}
		/// <summary>
		/// Atļauj nepareizas HTTP atbildes, piemēram, vārdu "ICY" "HTTP" vietā atbildes sākumā.
		/// </summary>
		/// <remarks>Ir tikai "enable" nevis "set", jo nav skaidrs, kurā brīdī droši var iestatīt "false".</remarks>
		// RedZ http://social.msdn.microsoft.com/forums/en-US/netfxnetcom/thread/ff098248-551c-4da9-8ba5-358a9f8ccc57
 		public static void EnableUnsafeHeaderParsing() {
			// Bibleotēka ar iekšējo klasi.
			Assembly assembly=Assembly.GetAssembly(typeof(System.Net.Configuration.SettingsSection));
			if (assembly != null) {
				// Iekšēja klase kā tips.
				Type settingsType=assembly.GetType("System.Net.Configuration.SettingsSectionInternal");
				if (settingsType != null) {
					// No iekšējās statiskās īpašības iegūst iekšējās iestatījumu klases instanci.
					// Ja instances vēl nav, to izveidos šī īpašība.
					object instance=settingsType.InvokeMember("Section",
						BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.NonPublic,
						null, null, new object[] { });
					if (instance != null) {
						// Iestata privāto bula lauku, kurš atļauj "nedrošas" HTTP galvenes.
						FieldInfo useUnsafeHeaderParsing=settingsType.GetField("useUnsafeHeaderParsing", BindingFlags.NonPublic | BindingFlags.Instance);
						if (useUnsafeHeaderParsing != null)
							useUnsafeHeaderParsing.SetValue(instance, true);
						// Ja vērtība būs false, tad nepazīstamas HTTP galvenes izraisīs kļūdas paziņojumu.
					}
				}
			}
		}
	}
}