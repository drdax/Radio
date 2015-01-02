using System;
using DrDax.RadioClient;

namespace Nrcu {
	public class NrcuBroadcast : Broadcast {
		public readonly int Id;
		public readonly int PresenterId;
		public NrcuBroadcast(int id, DateTime startTime, DateTime endTime, string caption, string description, int presenterId)
			: base(startTime, endTime, caption, description) {
				Id=id; PresenterId=presenterId;
		}
	}
}