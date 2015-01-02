using System;

namespace DrDax.RadioClient {
	/// <summary>Radio raidījums.</summary>
	public class Broadcast {
		private readonly DateTime startTime;
		private readonly DateTime endTime;
		private readonly string caption;
		private readonly string description;

		/// <summary>Sākumlaiks lietotāja datora laika joslā.</summary>
		public DateTime StartTime { get { return startTime; } }
		/// <summary>Beigu laiks lietotāja datora laika joslā.</summary>
		public DateTime EndTime { get { return endTime; } }
		/// <summary>Raidījuma nosaukums. Nedrīkst būt tukšs vai <c>null</c>.</summary>
		/// <remarks>Var būt dziesmas nosaukums muzikas kanālā.</remarks>
		public string Caption { get { return caption; } }
		/// <summary>Raidījum apraksts. Drīkst būt <c>null</c>.</summary>
		/// <remarks>Piemēram, var būt dziesmas izpildītājs muzikas kanālā.</remarks>
		public string Description { get { return description; } }

		public Broadcast(DateTime startTime, DateTime endTime, string caption, string description=null) {
			if (string.IsNullOrEmpty(caption)) throw new ArgumentNullException("caption");
			this.startTime=startTime; this.endTime=endTime;
			this.caption=caption; this.description=description;
		}
		#if DEBUG
		public override string ToString() {
			return string.Concat(startTime.ToString("d.HH:mm:ss"), ", ", endTime.ToString("d.HH:mm:ss"), ", ", caption, ", ", description);
		}
		#endif
	}
}