using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Reflection;
using System.Text;
using System.Windows.Media;

namespace DrDax.RadioClient {
	/// <summary>Paplašinājuma metodes esošām klasēm.</summary>
	public static class Extensions {
		/// <param name="text">Teksts, kurš jāpārveido.</param>
		/// <returns>Teksts, kuram katram vārdam pirmais burts ir lielais, pārējie mazi.</returns>
		public static string ToCapitalized(this string text) {
			// http://channel9.msdn.com/Forums/TechOff/252814-Howto-Capitalize-first-char-of-words-in-a-string-NETC/da6856faf4ab4ae29df69dea01538268
			// System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase neder,
			// jo tas ignorē esošos vārdus no lieliem burtiem, taisīt ToLower pirms tam nav vērts.
			if (string.IsNullOrEmpty(text)) return text;
			StringBuilder result=new StringBuilder(text);
			result[0]=char.ToUpper(result[0]);
			for (int n=1; n < result.Length; n++) {
				if (char.IsWhiteSpace(result[n-1]))
					result[n]=char.ToUpper(result[n]);
				else
					result[n]=char.ToLower(result[n]);
			}
			return result.ToString();
		}
		/// <summary>Sadala tekstu divās daļās (piemāram, izpildītāja un dziesmas nosaukumā) ar zināmu atdalītāju.</summary>
		/// <param name="title">Sadalāmais teksts.</param>
		/// <param name="description">Pirmā teksta daļa vai <c>null</c>, ja atalītājs netika atrasts.</param>
		/// <param name="separator">Atdalītājs.</param>
		/// <returns>Otrā teksta daļa vai viss teksts, ja atdalītājs netika atrasts.</returns>
		public static string SplitCaption(this string title, out string description, string separator=" - ") {
			string[] song=title.Split(new string[] { separator }, StringSplitOptions.None);
			string caption=song.Length == 2 ? song[1]:song[0];
			description=song.Length == 2 ? song[0]:null;
			return caption;
		}
		/// <summary>
		/// Piešķir <paramref name="request"/> starpniekserveri saskaņā ar programmas iestatījumiem.
		/// </summary>
		/// <remarks>Šo metodi jāizsauc vienreiz katrai <see cref="HttpWebRequest"/> instancei.</remarks>
		public static void ApplyProxy(this HttpWebRequest request) {
			if (!Settings.Default.UseSystemProxy) request.Proxy=null;
		}
		/// <param name="hexColor">Krāsas kods, piemēram, 0xFF008C.</param>
		/// <returns>Krāsa pēc tās RGB HEX koda.</returns>
		/// <remarks>Šī metode ieviesta, lai atvieglotu krāsu kopēšanu starp programmām.</remarks>
		public static Color ToColor(this Int32 hexColor) {
			return Color.FromRgb((byte)((hexColor & 0x00FF0000) >> 16), (byte)((hexColor & 0x0000FF00) >> 8), (byte)(hexColor & 0x000000FF));
		}
		/// <returns><c>enum</c> elementa cilvēkam attēlojamais nosaukums.</returns>
		public static string GetDisplayName(this Enum @enum) {
			if (@enum == null) return null;
			string name=@enum.ToString();
			FieldInfo fieldInfo=@enum.GetType().GetField(name);
			var attributes=(DisplayAttribute[])fieldInfo.GetCustomAttributes(typeof(DisplayAttribute), false);
			if (attributes.Length == 1) return attributes[0].GetName();
			return name;
		}
	}
}