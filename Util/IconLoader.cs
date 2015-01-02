using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace DrDax.RadioClient {
	public static class IconLoader {
		/// <param name="assemblyPath">Stacijas bibliotēkas faila ceļš.</param>
		/// <param name="index">Ikona kārtas nummurs sākot ar nulli un ņemot vērā tās nosaukumu pēc alfabēta resursu failā.</param>
		/// <returns>Izkasīto ikonu WPF saprotamā formātā.</returns>
		public static BitmapSource LoadAssemblyIcon(string assemblyPath, int index) {
			IntPtr hLib=LoadLibraryEx(assemblyPath, IntPtr.Zero, 0x2 /*LOAD_LIBRARY_AS_DATAFILE*/);

			// Atrod ikonas resursa nosaukumu pēc kārtas numura.
			// http://pinvoke.net/default.aspx/kernel32/EnumResourceNames.html piemērs vienkāršots, lai tikai izgūtu vienu nosaukumu.
			int n=0; string resourceName=null; // Nosaukumam noteikti jāsastāv no lieliem burtiem, drīkst būt cipari.
			if (!EnumResourceNamesWithID(hLib, 14 /*RT_GROUP_ICON*/,
				(IntPtr hModule, IntPtr lpszType, IntPtr lpszName, IntPtr lParam) => {
					if (n == index) {
						resourceName=lpszName.ToInt64() <= ushort.MaxValue ? "#"+lpszName.ToString():Marshal.PtrToStringUni(lpszName);
						return false; // Beigt pārskatīšanu.
					}
					n++;
					return true;
				}, IntPtr.Zero))
				if (Marshal.GetLastWin32Error() != 15106 && Marshal.GetLastWin32Error() != 0) // ERROR_RESOURCE_ENUM_USER_STOP (Vista vai vēlāk) vai ERROR_SUCCESS (XP)
					throw new Exception("Stacija nesatur nevienu ikonu: "+Marshal.GetLastWin32Error());

			// Ielādē ikonu un atbrīvo resursus.
			IntPtr hIcon=LoadImage(hLib, resourceName, ImageType.Icon, 16, 16, 0 /*LR_DEFAULTCOLOR*/);
			BitmapSource icon=Imaging.CreateBitmapSourceFromHIcon(hIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
			DestroyIcon(hIcon); FreeLibrary(hLib);
			return icon;
		}

		[DllImport("kernel32.dll", SetLastError=true)]
		private static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, uint dwFlags);
		[DllImport("kernel32.dll", SetLastError=true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool FreeLibrary(IntPtr hModule);
		[DllImport("user32.dll", SetLastError=true, CharSet=CharSet.Auto)]
		private static extern IntPtr LoadImage(IntPtr hLib, string name, ImageType type, int desiredWidth, int desiredHeight, uint fuLoad);
		[DllImport("user32.dll", SetLastError=true)]
		private static extern bool DestroyIcon(IntPtr hIcon);
		[DllImport("kernel32.dll", EntryPoint="EnumResourceNamesW", CharSet=CharSet.Unicode, SetLastError=true)]
		private static extern bool EnumResourceNamesWithID(IntPtr hModule, uint lpszType, EnumResNameDelegate lpEnumFunc, IntPtr lParam);
		/// <summary>
		/// Resursu pārskatīšanas metode. Neizmantojam Func<>, jo to nedrīkst lietot extern metodēs.
		/// </summary>
		private delegate bool EnumResNameDelegate(IntPtr hModule, IntPtr lpszType, IntPtr lpszName, IntPtr lParam);
	}
	public enum ImageType : uint {
		Bitmap=0,
		Icon=1,
		Cursor=2
	}
}