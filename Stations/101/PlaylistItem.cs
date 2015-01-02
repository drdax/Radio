using System;

namespace Ru101 {
	/// <summary>Dati par iepriekš atskaņotu dziesmu.</summary>
	public class PlaylistItem {
		/// <summary>Atskaņošanas sākuma datumas un laiks.</summary>
		public DateTime StartTime { get; set; }
		/// <summary>Paredzētais atskaņošanas ilgums.</summary>
		public TimeSpan Duration { get; set; }
		/// <summary>Pilns dziesmas nosaukums ar izpildītāju kā ir.</summary>
		public string Caption { get; set; }
		/// <summary>MP3 faila pilna adrese.</summary>
		public string Url { get; set; }
	}
}