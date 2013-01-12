using System;
using System.Text;

namespace DrDax.RadioClient {
	/// <summary>
	/// Radio programma, kura pieņem,
	/// ka ICE meta dati sastāv no nosaukuma (dziesmas nosaukums) un apraksta (izpildītājs) UTF8 kodu tabulā.
	/// </summary>
	public class SimpleIcyGuide : IcyGuide {
		private readonly bool capitalize;

		/// <param name="capitalize">Vai pārveidot datus tā, lai katrs vārds sāktos ar lielo burtu. Pēc noklusējuma nepārveido.</param>
		public SimpleIcyGuide(bool capitalize=false) : base(Encoding.UTF8) {
			this.capitalize=capitalize;
		}

		protected override Broadcast GetBroadcast(string title) {
			if (title == null) return null;
			if (capitalize) title=title.ToCapitalized();
			string description, caption=title.SplitCaption(out description);
			return new Broadcast(DateTime.Now, DateTime.Now.AddHours(1), caption, description);
		}
	}
}