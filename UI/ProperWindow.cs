using System;
using System.Collections.Generic;
using System.Windows;

namespace DrDax.RadioClient {
	/// <summary>Logs, kurš tiks aizvērts, mainoties kanālam vai aizverot programmu.</summary>
	/// <remarks>Visos gadījumos ir jāizmanto šīs klases mantieniekus.</remarks>
	public abstract class ProperWindow : Window {
		public ProperWindow() {
			if (windows == null) windows=new HashSet<ProperWindow>();
			windows.Add(this);
			this.Closed+=ProperWindow_Closed;
		}

		/// <summary>Aizver visas izveidotās instances.</summary>
		public static void CloseAll() {
			if (windows == null || windows.Count == 0) return;
			foreach (var window in windows)
				window.ForceClose();
			windows.Clear();
		}

		private void ForceClose() {
			this.Closed-=ProperWindow_Closed; // Neļauj izmainīt windows kolekciju, kad pār to skrien cikls.
			this.Close();
		}
		private void ProperWindow_Closed(object sender, EventArgs e) {
			this.Closed-=ProperWindow_Closed;
			windows.Remove(this);
		}
		/// <summary>Izveidoto (ne obligāti atvērto) logu kopa.</summary>
		private static HashSet<ProperWindow> windows;
	}
}