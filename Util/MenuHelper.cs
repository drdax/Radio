using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;

namespace DrDax.RadioClient {
	/// <summary>Nodrošina metodes loga izvēlnes maiņai.</summary>
	internal static class MenuHelper {
		/// <summary>Raidījumu saraksta komandu pirmais identifikators.</summary>
		public const int GuideIdOffset=10;
		/// <summary>Kanāla komandu pirmais identifikators.</summary>
		public const int ChannelIdOffset=15;

		#region Loga izvēlnes komandu identifikatori
		public const int GuideCommandId=1;
		public const int ClockCommandId=2;
		public const int LockCommandId=3;
		public const int ProxyCommandId=4;
		public const int AboutCommandId=5;
		/// <summary>SC_SIZE</summary>
		public const int ResizeCommandId=0xF000;
		/// <summary>SC_MAXIMIZE</summary>
		public const int MaximizeCommandId=0xF030;
		/// <summary>SC_RESTORE</summary>
		public const int RestoreCommandId=0xF120;
		/// <summary>Atdalītājs virs Aizvērt.</summary>
		public const int SeparatorCommandId=0;
		#endregion

		public static IntPtr SystemMenuHandle=IntPtr.Zero;
		public static IntPtr SettingsMenuHandle;
		/// <summary>
		/// Ielādētās izvēlnes punktu ikonas. Atslēga ir resursu plūsmas adrese, vertība ir atmiņas adrese.
		/// </summary>
		private static Dictionary<Uri, IntPtr> icons;

		public static void Delete(uint commandId) {
			DeleteMenu(SystemMenuHandle, commandId, MenuFlag.ByCommand);
		}
		/// <summary>Aizvāc izmēra maiņas loga izvēlnes punktus.</summary>
		/// <remarks>Tie ir atslēgti, ja izvēlni atver ar tastatūru, bet kļūst pieejami, ja ar ekrāna pogu.</remarks>
		public static void DeleteSystemSizeCommands(IntPtr menuHandle, bool allowMinimize) {
			DeleteMenu(menuHandle, 0, MenuFlag.ByCommand); // Atdalītājs virs Aizvērt.
			DeleteMenu(menuHandle, 0xF000, MenuFlag.ByCommand); // SC_SIZE
			DeleteMenu(menuHandle, 0xF030, MenuFlag.ByCommand); // SC_MAXIMIZE
			DeleteMenu(menuHandle, 0xF120, MenuFlag.ByCommand); // SC_RESTORE
			if (allowMinimize) return;
			DeleteMenu(menuHandle, 0xF020, MenuFlag.ByCommand); // SC_MINIMIZE
		}
		public static void SetIsChecked(uint commandId, bool isChecked) {
			CheckMenuItem(SettingsMenuHandle, commandId, isChecked ? MenuFlag.Checked:MenuFlag.Unchecked); // MenuFlag.ByCommand
		}
		public static void SetIsEnabledGuide(uint commandIdx, bool isEnabled) {
			EnableMenuItem(SystemMenuHandle, GuideIdOffset+commandIdx, isEnabled ? MenuFlag.Enabled:MenuFlag.Disabled); // MenuFlag.ByCommand
		}
		public static void SetItemIcon(IntPtr menuHandle, uint itemdPosition, Uri resourceUri) {
			IntPtr iconHandle;
			if (icons == null) icons=new Dictionary<Uri, IntPtr>(3);
			if (!icons.TryGetValue(resourceUri, out iconHandle)) {
				var streamInfo=RadioApp.GetResourceStream(resourceUri);
				var bitmap=new Bitmap(streamInfo.Stream);
				streamInfo.Stream.Dispose();
				iconHandle=bitmap.GetHbitmap(Color.Black); // Caurspīdīgums strādā tikai, ja šeit iestata melno krāsu.
				bitmap.Dispose();
				icons.Add(resourceUri, iconHandle);
			}
			SetMenuItemBitmaps(menuHandle, itemdPosition, MenuFlag.ByPosition, iconHandle, IntPtr.Zero);
		}
		[DllImport("user32.dll")]
		public static extern IntPtr GetSystemMenu(IntPtr hwnd, bool revert);
		[DllImport("user32", EntryPoint="InsertMenuW", CharSet=CharSet.Unicode)]
		public static extern bool InsertMenu(IntPtr menuHandle, uint itemId, MenuFlag flags, IntPtr commandId, string caption);
		[DllImport("user32.dll")]
		private static extern bool DeleteMenu(IntPtr menuHandle, uint itemId, MenuFlag flags);
		[DllImport("user32.dll")]
		public static extern IntPtr CreatePopupMenu();
		[DllImport("user32.dll")]
		private static extern bool CheckMenuItem(IntPtr menuHandle, uint itemdId, MenuFlag flags);
		[DllImport("user32.dll")]
		private static extern bool EnableMenuItem(IntPtr menuHandle, uint itemdId, MenuFlag flags);
		[DllImport("user32.dll")]
		private static extern bool SetMenuItemBitmaps(IntPtr menuHandle, uint itemdId, MenuFlag flags, IntPtr uncheckedBitmapHandle, IntPtr checkedBitmapHandle);
	}
	public enum MenuFlag {
		Checked  =0x008,
		Unchecked=0x000,
		Enabled  =0x000,
		Disabled =0x002,
		Grayed   =0x001, // Faktiski tas pats Disabled.

		/// <summary>Apakšizvēlne. Izvēlnes spals jāpadod kā ID parametrs.</summary>
		Popup    =0x010,
		/// <summary>Teksta elements.</summary>
		String   =0x000,
		BarBreak =0x020,
		Break    =0x040,
		/// <summary>Atdalošā līnija.</summary>
		Separator=0x800,

		/// <summary>Vietu izvēlnē nosaka pēc kārtas numuru sākot ar nulli.</summary>
		ByPosition=0x400,
		/// <summary>Izvēlnes elementu nosaka pēc tā [komandas] identifikatora.</summary>
		ByCommand= 0x000
	}
}