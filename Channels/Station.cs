using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;

namespace DrDax.RadioClient {
	/// <summary>Radio stacija ar vienu vai vairākiem raidošiem kanāliem.</summary>
	public abstract class Station {
		/// <summary>Ielādēto kanālu logotipu kopija atmiņā.</summary>
		private static readonly Dictionary<string, BitmapImage> images=new Dictionary<string, BitmapImage>();
		/// <summary>
		/// Vai stacijas kanāliem ir jāsniedz informācija par skanošajiem raidījumiem.
		/// </summary>
		/// <remarks>
		/// Šis lauks ieviests, jo <see cref="Properties.Settings"/> nav pieejams stacijās.
		/// Visām stacijām ir jāievēro šī lauka vērtība.
		/// </remarks>
		public static readonly bool UseGuide=Properties.Settings.Default.UseGuide;
		/// <summary>Stacijas laika josla, ja tāda ir.</summary>
		protected readonly TimeZoneInfo timezone;
		/// <summary>Ceļš līdz failam ar Windows resursiem, kura ikonas apzīme stacijas kanālus.
		/// Ikona attēlošanai līdzās kanāla nosaukumam.</summary>
		public readonly string IconPath;
		/// <summary>Visu stacijas kanālu numuri (atslēga) un nosaukumi (vērtība).</summary>
		public readonly StationChannelList Channels;
		/// <summary>
		/// Atgriež kanāla instanci pēc tā numura. Drīkst izraisīt <see cref="ChannelNotFoundException"/> izņēmumu, ja kanāla numurs nepareizs.
		/// </summary>
		/// <param name="number">Kanāla numurs stacijas ietvaros.</param>
		public abstract Channel GetChannel(byte number);

		protected Station(StationChannelList channels, string timezoneName=null) {
			Channels=channels;
			IconPath=this.GetType().Assembly.Location;
			if (timezoneName != null) timezone=TimeZoneInfo.FindSystemTimeZoneById(timezoneName);
		}

		/// <summary>Atgriež attēlu no stacijas pakotnes. Izmanto logotipu izgūšanai.</summary>
		protected BitmapImage GetResourceImage(string imageName) {
			BitmapImage image;
			if (images.TryGetValue(imageName, out image)) return image;
			image=new BitmapImage();
			image.BeginInit();
			image.CacheOption=BitmapCacheOption.OnLoad;
			image.UriSource=new Uri(string.Format("pack://application:,,,/{0};component/{1}", this.GetType().Assembly.GetName().Name, imageName));
			image.EndInit();
			image.Freeze();
			images.Add(imageName, image);
			return image;
		}
	}
}