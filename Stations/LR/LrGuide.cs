using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using DrDax.RadioClient;

namespace Lr {
	public class LrGuide : PagedListedGuide {
		/// <summary>Radiokanāla numurs (1 līdz 4).</summary>
		private readonly uint number;

		internal LrGuide(uint number, TimeZoneInfo timezone) : base(timezone, new SimpleGuideMenu("Par raidījumu")) {
			this.number=number;
		}
		protected override async Task FillGuide(DateTime date) {
			bool hasNextPage=false; byte pageIdx=0; // Arhīvā ir lapošana, jo iespējams izvēlēties vairāku dienu periodu un ieslēgt visus piecus kanālus.
			do {
				string html=await client.DownloadStringTaskAsync(string.Format(
					"http://latvijasradio.lv/lv/lr/arhivs/?adv=1&d={0}&m={1}&y={2}&channel={3}&page={4}", date.Day, date.Month, date.Year, number, pageIdx));
				int idx=html.IndexOf("<table class=\"audio-archive-results\">"); // Paņem tikai vajadzīgo daļu lapas arī tāpēc, ka kopumā tā nav XML savietojama.
				hasNextPage=html.Contains("class=\"next-page\""); // Vai ir saite uz nākamo lappusi.
				html=html.Substring(idx+39, html.IndexOf("</table>", idx)-idx-39); // 39=len(<table..>)
				var doc=XDocument.Parse(new Regex("&(?!amp;)").Replace(new Regex("<\\/?([BI]|FONT[^>]*|br)>").Replace(html, string.Empty), "&amp;").Replace("</p><p>", "<br/>").Replace("&nbsp;", string.Empty)); // Gadās HTML, kuri neatbilst XML prasībām.
				foreach (var tr in doc.Root.Elements("tr")) { // Root=<tbody>
					var cells=tr.Elements("td"); var dataCell=cells.ElementAt(1).Element("div");
					var header=dataCell.Element("h3"); string caption=header.Value;
					var sb=new StringBuilder(360); string pageUrl=null;
					if (dataCell.Element("small").Value.Length != 0 && caption != dataCell.Element("small").Value) sb.AppendLine(dataCell.Element("small").Value);
					var divs=dataCell.Elements("div");
					string description=null; StringBuilder guests=null;
					foreach (XElement div in divs) {
						// Viesi mēdz būt gan pirms, gan pēc raidījuma apraksta.
						if (div.Attribute("class").Value == "guests") {
							guests=new StringBuilder(200);
							guests.AppendLine("Viesi:"); // Šāds ir arī virsraksts (h4) viesu blokam.
							foreach (var node in div.Nodes())
								if (node.NodeType == XmlNodeType.Text)
									guests.AppendLine(((XText)node).Value);
						} else { // class=descr
							foreach (var node in div.Nodes())
								if (node.NodeType == XmlNodeType.Element) {
									var p=((XElement)node);
									if (p.Name == "p")
										foreach (var e in p.Elements()) {
											switch (e.Name.LocalName) {
												case "br": sb.AppendLine(); break;
												// Sarežģitiem raidījumiem var būt pa saitei uz katru nodalījumu, bet tā kā nav zināms to sākumlaiks, raidījumam kopumā iegaumē pēdējo saiti.
												case "a": pageUrl=e.Attribute("href").Value; break;
												case "p":
													if (e.Value.Trim().Length != 0) // Dažreiz atdala ar tukšām rindkopām.
														sb.AppendLine(e.Value);
													break;
												// Retos gadījumos, kad nav raidījuma nosaukuma, paņem pirmo izcelto tekstu.
												case "strong": if (caption.Length == 0) caption=e.Value; else sb.Append(e.Value); break;
												default: sb.Append(e.Value); break;
											}
										} else if (p.Name == "br") sb.AppendLine();
									else sb.Append(p.Value);
								} else if (node.NodeType == XmlNodeType.Text)
									sb.Append(((XText)node).Value);
						}
					}
					description=sb.ToString();
					if (guests != null)
						description=string.Concat(description, description.EndsWith(Environment.NewLine) ? "":Environment.NewLine, guests.ToString());
					// Lai nerādītu tukšu pēdējo rindu, to noņem.
					if (description.EndsWith(Environment.NewLine))
						description=description.Substring(0, description.Length-Environment.NewLine.Length);
					AddBroadcast(date.Add(TimeSpan.Parse(cells.ElementAt(0).Element("time").Value.Trim())), // HH:mm
						caption.Replace("&quot;", "\""), description,
						pageUrl == null && header.Element("a") != null ? header.Element("a").Attribute("href").Value:pageUrl);
				}
				pageIdx++;
			} while (hasNextPage);
		}
	}
}