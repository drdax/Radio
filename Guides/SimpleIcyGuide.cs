using System;
using System.Text;
using System.Threading.Tasks;

namespace DrDax.RadioClient {
	/// <summary>
	/// Radio programma, kura pieņem,
	/// ka ICE meta dati sastāv no nosaukuma (dziesmas nosaukums) un apraksta (izpildītājs) UTF8 kodu tabulā.
	/// </summary>
	public class SimpleIcyGuide : IcyGuide {
		private readonly bool capitalize;

		/// <param name="capitalize">Vai pārveidot datus tā, lai katrs vārds sāktos ar lielo burtu. Pēc noklusējuma nepārveido.</param>
		public SimpleIcyGuide(bool capitalize=false, Menu<Guide> menu=null) : base(Encoding.UTF8, menu) {
			this.capitalize=capitalize;
		}

		protected override Task<Broadcast> GetBroadcast(string title) {
			if (title == null) return NullTaskBroadcast;
			if (capitalize) title=title.ToCapitalized();
			string description, caption=title.SplitCaption(out description);
			return Task.FromResult(new Broadcast(DateTime.Now, DateTime.Now.AddMilliseconds(Channel.DefaultTimeout), caption, description));
		}
	}
}