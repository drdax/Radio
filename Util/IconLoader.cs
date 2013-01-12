using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace DrDax.RadioClient {
	public static class IconLoader {
		public static BitmapSource LoadAssemblyIcon(string assemblyPath, int index) {
			IntPtr hLib=LoadLibraryEx(assemblyPath, IntPtr.Zero, 0x2 /*LOAD_LIBRARY_AS_DATAFILE*/);

			// Atrod ikonas resursa nosaukumu pēc kārtas numura.
			// http://pinvoke.net/default.aspx/kernel32/EnumResourceNames.html piemērs vienkāršots, lai tikai izgūtu vienu nosaukumu.
			int n=0; string resourceName=null;
			if (!EnumResourceNamesWithID(hLib, 14 /*RT_GROUP_ICON*/,
				(IntPtr hModule, IntPtr lpszType, IntPtr lpszName, IntPtr lParam) => {
					if (n == index) {
						resourceName=((uint)lpszName) <= ushort.MaxValue ? "#"+lpszName.ToString():Marshal.PtrToStringUni(lpszName);
						return false; // Beigt pārskatīšanu.
					}
					n++;
					return true;
				}, IntPtr.Zero))
				if (Marshal.GetLastWin32Error() != 15106 && Marshal.GetLastWin32Error() != 0) // ERROR_RESOURCE_ENUM_USER_STOP (Vista vai vēlāk) vai ERROR_SUCCESS (XP)
					throw new Exception("Stacija nesatur nevienu ikonu: "+Marshal.GetLastWin32Error());

			// Ielādē ikonu un atbrīvo resursus.
			IntPtr hIcon=LoadImage(hLib, resourceName, 1 /*IMAGE_ICON*/, 16, 16, 0 /*LR_DEFAULTCOLOR*/);
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
		private static extern IntPtr LoadImage(IntPtr hinst, string lpszName, uint uType, int cxDesired, int cyDesired, uint fuLoad);
		[DllImport("user32.dll", SetLastError=true)]
		private static extern bool DestroyIcon(IntPtr hIcon);
		[DllImport("kernel32.dll", EntryPoint="EnumResourceNamesW", CharSet=CharSet.Unicode, SetLastError=true)]
		static extern bool EnumResourceNamesWithID(IntPtr hModule, uint lpszType, EnumResNameDelegate lpEnumFunc, IntPtr lParam);
		/// <summary>
		/// Resursu pārskatīšanas metode. Nelietojam Func<>, jo to nedrīkst lietot extern metodēs.
		/// </summary>
		private delegate bool EnumResNameDelegate(IntPtr hModule, IntPtr lpszType, IntPtr lpszName, IntPtr lParam);
	}
}