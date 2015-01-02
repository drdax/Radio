using System;
using DrDax.RadioClient;

namespace Retro {
	public abstract class RetroGuide : TimedGuide {
		protected readonly ProperWebClient client=new ProperWebClient(System.Text.Encoding.ASCII);
		protected const string StubCaption="Ретро FM"; // Oriģinālā teksts "Слушайте Ретро FM!", kuru liek song.js.
		// http://en.wikipedia.org/wiki/Unix_time
		protected static readonly DateTime unixEpoch=new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		public RetroGuide() : base(null) {}

		/// <returns>UNIX taimkods <paramref name="timestamp"/> klausītāja laika joslā.</returns>
		protected static DateTime GetLocalTime(string timestamp) {
			return TimeZoneInfo.ConvertTime(unixEpoch.AddSeconds(long.Parse(timestamp)), TimeZoneInfo.Local);
		}
		/// <summary>
		/// Pēc iespējas iestata pašreizējo vai iepriekšējo raidījumu.
		/// </summary>
		/// <param name="current">Pašreizējā (iestatāmā) raidījuma dati.</param>
		/// <param name="now">Dotais brīdis klausītāja laika joslā.</param>
		/// <returns><c>false</c>, ja nomainīja pašreizējo raidījumu.</returns>
		protected bool SetCurrentBroadcast(Broadcast current, DateTime now) {
			if (current.StartTime > now)
				// Ja servera laiks ir priekšā, tad pieņem ka dziesma sākas tagad.
				CurrentBroadcast=new Broadcast(now, current.EndTime, current.Caption, current.Description);
			else if (current.EndTime < now) {
				PreviousBroadcast=current;
				// Mēdz pašreizējā dziesma noskanēt, bet dati par jauno neparādīties. Tad cerīgi uzgaida, cik parasti pārbauda oriģinālais atskaņotājs.
				return true;
			} else CurrentBroadcast=current;
			return false;
		}
	}
}