using System;
using System.ComponentModel;

namespace DrDax.RadioClient {
	/// <summary>Programmas iestatījumi.</summary>
	internal sealed partial class Settings {
		/// <summary>Vai programmas iestatījumi ir mainījušies.</summary>
		public bool HasChanges=false;
		/// <summary>
		/// Tiek izsaukts, kad noamainās iestatījums "Lietot sistēmas starpniekserveri". Parametrs ir jaunā vērtība.
		/// </summary>
		public event Action<bool> ProxyChanged;

		protected override void OnPropertyChanged(object sender, PropertyChangedEventArgs e) {
			if (e.PropertyName == "UseProxy" && ProxyChanged != null) ProxyChanged(UseSystemProxy);
			HasChanges=true; // Pat ja jaunā iestatījumu vērtība sakrīt ar veco.
			base.OnPropertyChanged(sender, e);
		}
	}
}