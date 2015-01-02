using System.Collections.Generic;

namespace Ru101 {
	public class ChannelGroup {
		public string Caption { get { return caption; } }
		public int ChannelCount { get { return channelCount; } }

		public ChannelGroup(short id, string caption, int channelCount) {
			this.Id=id;
			this.caption=caption; this.channelCount=channelCount;
		}

		// Ar zīmi, jo -1 ir personālo staciju identifikators.
		public readonly short Id;
		private readonly string caption;
		private readonly int channelCount;
		public List<ChannelItem> Channels;
	}
}