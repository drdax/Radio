using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pieci {
	/// <summary>Dati par iepriekš atskaņotu dziesmu.</summary>
	public class PlaylistItem {
		/// <summary>Atskaņošanas sākuma datumas un laiks.</summary>
		public DateTime StartTime { get; set; }
		/// <summary>Paredzētais atskaņošanas ilgums.</summary>
		public TimeSpan Duration { get; set; }
		/// <summary>Izpildītāja vārds.</summary>
		public string Artist { get; set; }
		/// <summary>Dziesmas nosaukums.</summary>
		public string Caption { get; set; }
		/// <summary>Apraksts, piemēram, koncerts, kurā skanēja dziesma.</summary>
		public string Description { get; set; }
	}
}