using System;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using DrDax.RadioClient;

namespace Nrcu {
	public class NrcuGuide : DescriptionListedGuide {
		private static readonly Regex guideRx=new Regex(@"\\""vremja\\"">(?'time'[012][0-9]:[0-5][0-9])<\\\/td>.+?<a[^>]+>(\\""(?'caption'[^""]+)\\""\.?(?'description'[^<]*)|(?'soloCaption'[^""][^<]+))", RegexOptions.Compiled);
		private readonly string guideUrl;
		private JavaScriptSerializer deserializer;

		internal NrcuGuide(byte number, TimeZoneInfo timezone) : base(timezone) {
			guideUrl=string.Format("http://radioukr.com.ua/getData.php?chnl=NRCU{0}&date=", number);
			Initialize();
		}

		protected override void FillGuide(DateTime date) {
			deserializer=new JavaScriptSerializer();
			using (var client=new ProperWebClient()) {
				foreach (Match match in guideRx.Matches(client.DownloadString(guideUrl+date.ToString("dd.MM.yyyy")))) {
					TimeSpan time=TimeSpan.Parse(match.Groups["time"].Value); // HH:mm
					if (match.Groups["caption"].Success) {
						string description=Deserialize(match, "description").Trim();
						AddBroadcast(date.Add(time), Deserialize(match, "caption"), description.Length == 0 ? null:description);
					} else AddBroadcast(date.Add(time), Deserialize(match, "soloCaption"), null);
				}
			}
			deserializer=null;
		}
		private string Deserialize(Match match, string groupName) {
			// Simbolu virkne ielikta pēdiņās deserializācijai.
			return deserializer.Deserialize<string>('"'+match.Groups[groupName].Value+'"');
		}
	}
}