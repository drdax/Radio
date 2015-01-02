using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;

namespace DrDax.RadioClient {
	/// <summary>Radio stacija ar vienu vai vairākiem raidošiem kanāliem.</summary>
	public abstract class Station {
		/// <summary>Ielādēto kanālu logotipu kopija atmiņā.</summary>
		private static readonly Dictionary<string, BitmapSource> images=new Dictionary<string, BitmapSource>();
		/// <summary>Stacijas bibliotēkas nosaukums bez paplašinājuma.</summary>
		private string assemblyName;
		protected bool hasSettingsChanges=false;
		/// <summary>Stacijas laika josla, ja tāda ir.</summary>
		protected readonly TimeZoneInfo timezone;
		/// <summary>Ceļš līdz failam ar Windows resursiem, kura ikonas apzīme stacijas kanālus.
		/// Ikona attēlošanai līdzās kanāla nosaukumam.</summary>
		public readonly string IconPath;
		/// <summary>Visu stacijas kanālu numuri (atslēga) un nosaukumi (vērtība).</summary>
		public readonly StationChannelList Channels;
		/// <param name="number">Kanāla numurs stacijas ietvaros.</param>
		/// <returns>Kanāls, kuru nosaka pēc numura <paramref name="number"/>.
		/// Drīkst izraisīt <see cref="ChannelNotFoundException"/> izņēmumu, ja kanāla numurs nepareizs.</returns>
		public abstract Channel GetChannel(uint number);
		/// <returns>Raidījumu saraksts kanālam <paramref name="channelNumber"/>. Drīkst atgriezt <c>null</c>, ja kanālam nav raidījumu saraksta.</returns>
		public abstract Guide GetGuide(uint channelNumber);
		/// <summary>
		/// Vai stacijas iestatījumi, kuri glabājas programmas failā, ir mainījušies.
		/// </summary>
		public bool HasSettingsChanges { get { return hasSettingsChanges; } }

		protected Station(StationChannelList channels, string timezoneName=null) {
			Channels=channels;
			var assembly=this.GetType().Assembly;
			IconPath=assembly.Location; assemblyName=assembly.GetName().Name;
			if (timezoneName != null) timezone=TimeZoneInfo.FindSystemTimeZoneById(timezoneName);
		}

		/// <summary>
		/// Saglabā stacijas iestātījumus. Jāimplementē, ja stacija ir sava iestatījumu klase.
		/// </summary>
		public virtual void SaveSettings() {}
		/// <summary>
		/// Atgriež pilnu kanāla <paramref name="channelNumber"/> mājaslapas adresi.
		/// </summary>
		/// <returns></returns>
		public abstract string GetHomepage(uint channelNumber);
		/// <summary>Atgriež attēlu no stacijas pakotnes. Izmanto logotipu izgūšanai.</summary>
		protected BitmapSource GetResourceImage(string imageName) {
			BitmapImage image=GetCachedImage(imageName) as BitmapImage;
			if (image == null) {
				image=new BitmapImage();
				image.BeginInit();
				image.CacheOption=BitmapCacheOption.OnLoad;
				image.UriSource=new Uri(string.Format("pack://application:,,,/{0};component/{1}", assemblyName, imageName));
				image.EndInit();
				image.Freeze();
				images.Add(assemblyName+imageName, image);
			}
			return image;
		}
		/// <returns>Saglabātu attēlu vai <c>null</c>, ja <paramref name="imageName"/> nepazīstams.</returns>
		protected BitmapSource GetCachedImage(string imageName) {
			BitmapSource image;
			if (images.TryGetValue(assemblyName+imageName, out image)) return image;
			return null;
		}
		/// <summary>Saglabā attēlu <paramref name="image"/> zem nosaukuma <paramref name="imageName"/> atkārtotai izgūšanai.</summary>
		protected void CacheImage(string imageName, BitmapSource image) {
			images.Add(assemblyName+imageName, image);
		}
	}
}