using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using DrDax.RadioClient;

namespace Euronews {
	public class EuronewsGuide : PollingGuide {
		/// <summary>
		/// Raidījumu saraksta izgūšanas intervāls sekundēs.
		/// </summary>
		private const int TimerTimeout=10; // Oriģinālais Euronews uztvērējs pārbauda ik pēc 10 sekundēm.

		/// <summary>Nepārtrauktas lielo burtu virknes noteikšanai.</summary>
		/// <remarks>Tā tiek pierakstīti sadaļu nosaukumi, piemēram, NEWS vai SPORT.</remarks>
		private readonly Regex allCapsRx=new Regex("^[A-Z]+$", RegexOptions.Compiled);
		private readonly TimeZoneInfo timezone;
		private readonly string guideUrl;

		internal EuronewsGuide(uint number, TimeZoneInfo timezone) : base(TimerTimeout, null) {
			// Sākotnējās izgūšanas laikam vajadzētu palīdzēt novērst nepareizo raidījumu secību.
			guideUrl=string.Format("http://www.euronewsradio.com/winradio/prog{0}.xml?{1:yyyyMMddHHmmss}",
				number == 1 ? string.Empty:number.ToString(), TimeZoneInfo.ConvertTime(DateTime.UtcNow, timezone));
			this.timezone=timezone;
		}

		protected override async Task UpdateBroadcasts() {
			// Raidījumi ir sakārtoti pēc sākumlaika dilstošā secībā.
			try {
				XElement doc=XDocument.Parse(await client.DownloadStringTaskAsync(guideUrl)).Root;
				Broadcast current=GetBroadcast(doc.Element("morceau"));
				if (CurrentBroadcast == null || current.StartTime != CurrentBroadcast.StartTime) {
					PreviousBroadcast=CurrentBroadcast ?? GetBroadcast(doc.Elements("morceau").ElementAt(1));
					CurrentBroadcast=current;
				}
			} catch {
				// Retos gadījumos var trāpīties bojāts XML.
				CurrentBroadcast=null;
			}
		}
		private Broadcast GetBroadcast(XElement xml) {
			DateTime startTime=TimeZoneInfo.ConvertTime(
				DateTime.ParseExact(xml.Element("date_prog").Value, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
				timezone, TimeZoneInfo.Local);
			string caption=xml.Element("chanson").Value;
			return new Broadcast(startTime, startTime.AddSeconds(TimerTimeout), allCapsRx.IsMatch(caption) ? caption.ToCapitalized():caption);
		}
	}
}