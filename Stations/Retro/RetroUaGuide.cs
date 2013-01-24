﻿using System;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Xml;
using System.Xml.Linq;
using DrDax.RadioClient;

namespace Retro {
	/// <summary>Kijevas Ретро FM skanošās dziesmas.</summary>
	public class RetroUaGuide : RetroGuide {
		protected override void UpdateBroadcasts() {
			XElement json=XElement.Load(JsonReaderWriterFactory.CreateJsonReader(
					client.DownloadData("http://retro.ua/on_air/onair.json"),
					XmlDictionaryReaderQuotas.Max));
			/* Pašreizējā dziesma detaļās un pēdējās trīs skanējušās.
			 {"playing":{"dbid":116952,"db":false,
				 "xml":{"artist":"ELTON JOHN","name":"SACRIFICE"},
				 "src":"\/media\/default\/img\/no_img_30x30.png","duration":"00:04:53","start_ts":1357164024,"end_ts":1357164317,"start":"2013-01-03 00:00:24","end":"2013-01-03 00:05:17"},
			  "playlist":[
				 {"id":"15147","artist":{"id":false,"name":"ELTON JOHN"},"song":{"id":false,"name":"SACRIFICE"},"start_ts":1357164024,"duration":293},
				 {"id":"15146","artist":{"id":false,"name":"ANJELIKA VARUM"},"song":{"id":false,"name":"NASH SOSED"},"start_ts":1357163641,"duration":178},
				 {"id":"15145","artist":{"id":false,"name":"CORONA"},"song":{"id":false,"name":"BABY,BABY"},"start_ts":1357163412,"duration":223}],
			  "djs":[{"name":"\u0416\u043e\u0440\u0436 \u0412\u0435\u043b\u044e\u0440\u043e\u0432","src":"\/thumb\/player_160_115\/staff\/fb\/5c\/50f428344b418_2.png","link":"\/staff\/di-dzhei\/2"}],
			  "date":"2013-01-03 00:02:01","ts":1357164121}
			 */
			System.Diagnostics.Debug.WriteLine(json.ToString());
			XElement item;
			if (CurrentBroadcast == null) {
				item=json.Element("playlist").Elements("item").ElementAt(1);
				DateTime start=GetLocalTime(item.Element("start_ts").Value);
				PreviousBroadcast=new Broadcast(start, start.AddSeconds(short.Parse(item.Element("duration").Value)),
					item.Element("song").Element("name").Value.ToCapitalized(), item.Element("artist").Element("name").Value.ToCapitalized());
			} else if (CurrentBroadcast.Caption != StubCaption) PreviousBroadcast=CurrentBroadcast;
			item=json.Element("playing");
			DateTime now=DateTime.Now;
			if (item == null || SetCurrentBroadcast(new Broadcast(
				GetLocalTime(item.Element("start_ts").Value), GetLocalTime(item.Element("end_ts").Value),
				item.Element("xml").Element("name").Value.ToCapitalized(), item.Element("xml").Element("artist").Value.ToCapitalized()), now)) {
				XElement dj=json.Element("djs").Elements().FirstOrDefault();
				// Ukrainas uztvērējs pārbauda raidījumus reizi minūtē, bet šeit aizkave 30 sekundes tāpat kā Maskavas kanālam.
				CurrentBroadcast=new Broadcast(now, now.AddSeconds(30), dj != null ? dj.Element("name").Value:StubCaption);
			}
		}
		// Saprotamais laiks ir Kijevas laika joslā un lai to lieku reizi nepārveidotu, tiek lietots Unix laiks.
		//private DateTime ParseTime(string time) { return DateTime.ParseExact(time, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture); }
	}
}