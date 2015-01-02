using System;

namespace DrDax.RadioClient {
	public class ChannelNotFoundException : Exception {
		public ChannelNotFoundException(uint channelNumber) :
			base(string.Format("Kanāls ar numuru {0} nav atrasts", channelNumber))
		{}

		public ChannelNotFoundException(string channelId) :
			base(string.Format("Kanāls \"{0}\" nav atrasts", channelId))
		{}
	}
}