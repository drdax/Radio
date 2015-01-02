using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace DrDax.RadioClient {
	/// <summary>Noklusējuma programmu noskaidrošanas metodes.</summary>
	public static class DefaultProgram {
		/// <param name="extension">Faila paplašinājums. Drīkst būt ar vai bez punkta sākumā.</param>
		/// <returns>Pilns ceļš līdz programmai, kura spēj atvērt failu ar paplašinājumu <paramref name="extension"/>. <c>null</c>, ja nespēja atrast.</returns>
		// http://www.pinvoke.net/default.aspx/shlwapi.AssocQueryString
		public static string GetForFileExtension(string extension) {
			if (string.IsNullOrWhiteSpace(extension)) throw new ArgumentNullException("extension");
			if (extension[0] != '.') extension='.'+extension;

			uint strLen=0; // Atgriezto simbolu skaits.
			// Noskaidro atgriezto datu apjomu.
			try {
				AssocQueryString(AssocF.Verify, AssocStr.Executable, extension, null, null, ref strLen);
			} catch { return null; }
			if (strLen == 0) return null;

			// Sagatavo atmiņu.
			StringBuilder appPath=new StringBuilder((int)strLen);
			// Noskaidro programmas ceļu.
			AssocQueryString(AssocF.Verify, AssocStr.Executable, extension, null, appPath, ref strLen);
			return appPath.ToString();
		}
		public static void OpenFile(string filePath) {
			var programPath=GetForFileExtension(Path.GetExtension(filePath));
			if (programPath != null)
				Process.Start(programPath, filePath);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void OpenPage(string url) {
			Process.Start(url); // Varbūt kādreiz būs interesantāks veids, kā atvērt noklusēto pārlūku.
		}

		[DllImport("Shlwapi.dll", SetLastError=true, CharSet=CharSet.Auto)]
		static extern uint AssocQueryString(AssocF flags, AssocStr str, string pszAssoc, string pszExtra, [Out] StringBuilder pszOut, [In][Out] ref uint pcchOut);
		[Flags]
		private enum AssocF {
			Init_NoRemapCLSID=0x1,
			Init_ByExeName=0x2,
			Open_ByExeName=0x2,
			Init_DefaultToStar=0x4,
			Init_DefaultToFolder=0x8,
			NoUserSettings=0x10,
			NoTruncate=0x20,
			Verify=0x40,
			RemapRunDll=0x80,
			NoFixUps=0x100,
			IgnoreBaseClass=0x200
		}
		private enum AssocStr {
			Command=1,
			Executable,
			FriendlyDocName,
			FriendlyAppName,
			NoOpen,
			ShellNewValue,
			DDECommand,
			DDEIfExec,
			DDEApplication,
			DDETopic,
			INFOTIP,
			QUICKTIP,
			TILEINFO,
			CONTENTTYPE,
			DEFAULTICON,
			SHELLEXTENSION,
			DROPTARGET,
			DELEGATEEXECUTE,
			SUPPORTED_URI_PROTOCOLS,
			MAX
		}
	}
}