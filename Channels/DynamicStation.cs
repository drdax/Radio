using System;

namespace DrDax.RadioClient {
	/// <summary>Raidstacija, kuras kanālu sarakstu var mainīt lietotājs radio darbības laikā.</summary>
	public abstract class DynamicStation : Station {
		/// <summary>Jāizsauc, kad dinamiskais kanālu saraksts mainījās un to jāparāda saskarnē.</summary>
		public Action ChannelsChanged;
		public DynamicStation(string timezone) : base(new StationChannelList(), timezone) {}
		/// <summary>Dzēš lietotāja izvēlēto kanālu no redzamā saraksta.</summary>
		/// <param name="number">Kanāla ID stacijas ietvaros.</param>
		public abstract void RemoveChannel(uint number);
	}
}