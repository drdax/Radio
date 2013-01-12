using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace DrDax.RadioClient {
	/// <summary>Radio programmas saskarnes krāsas.</summary>
	/// <remarks><c>sealed</c>, lai stacijas nemēģinātu ietektmēt izskatu.</remarks>
	public sealed class Brand : INotifyPropertyChanged {
		private static readonly Brush inactiveCaptionBackground=(Brush)Application.Current.Resources["InactiveCaptionBackground"];
		private readonly SolidColorBrush textForeground;
		private readonly SolidColorBrush statusForeground;
		private readonly SolidColorBrush captionForeground;
		private readonly Brush captionBackground;
		private readonly Brush logoBackground;
		private readonly Brush guideBackground;
		private bool focused=true;

		/// <summary>
		/// Vai logotipa fons tiks lietots arī kā raidījumu saraksta fons.
		/// </summary>
		public readonly bool HasSharedBackground;
		/// <summary>Raidījumu saraksta teksta krāsa.</summary>
		public SolidColorBrush TextForeground { get { return textForeground; } }
		/// <summary>Atskaņošanas ilguma un skaļuma rādītāju krāsa.</summary>
		public SolidColorBrush StatusForeground { get { return statusForeground; } }
		/// <summary>Virsraksta un minimizēšanas/aizvēršanas pogu krāsa.</summary>
		public SolidColorBrush CaptionForeground {
			get { return focused?captionForeground:SystemColors.InactiveCaptionTextBrush; }
		}
		/// <summary>Loga virsraksta fons.</summary>
		public Brush CaptionBackground {
			get { return focused?captionBackground:inactiveCaptionBackground; }
		}
		/// <summary>Kanāla logotipa un skaļuma regulatora fons.</summary>
		public Brush LogoBackground { get { return logoBackground; } }
		/// <summary>Raidījumu saraksta fons.</summary>
		public Brush GuideBackground { get { return guideBackground; } }
		/// <summary>
		/// Vai programmas logs ir fokusā. Iestata, lai loga virsraksts izskatītos atbilstoši stāvoklim.
		/// </summary>
		internal bool Focused {
			set {
				focused=value;
				if (PropertyChanged != null) {
					PropertyChanged(this, new PropertyChangedEventArgs("CaptionForeground"));
					PropertyChanged(this, new PropertyChangedEventArgs("CaptionBackground"));
				}
			}
		}
	
		/// <summary>Noformējums ar īpašām fona otām.</summary>
		/// <remarks>
		/// Lai lietotu vienu bildi visai kopējai ekrāna daļai,
		/// to jāpadod kā <paramref name="logoBackground"/>, bet <paramref name="guideBackground"/> jābūt <c>null</c>.
		/// </remarks>
		public Brand(Color textColor, Color statusColor, Color captionTextColor,
			Brush captionBackground, Brush logoBackground, Brush guideBackground=null) {
			if (textColor == null || statusColor == null || captionBackground == null) throw new ArgumentNullException("Kāda krāsa nav norādīta");
			if (captionBackground == null || logoBackground == null) throw new ArgumentNullException("Kāda ota nav norādīta");

			if (guideBackground == null) HasSharedBackground=true;
			else {
				guideBackground.Freeze(); HasSharedBackground=false;
			}
			textForeground=new SolidColorBrush(textColor); statusForeground=new SolidColorBrush(statusColor);
			captionForeground=new SolidColorBrush(captionTextColor);
			this.captionBackground=captionBackground; this.logoBackground=logoBackground; this.guideBackground=guideBackground;
			// Iesaldē otas labākai veiktspējai. To animācija pēc šī soļa nav iespējama.
			textForeground.Freeze(); statusForeground.Freeze(); captionForeground.Freeze();
			captionBackground.Freeze(); logoBackground.Freeze();
		}
		/// <summary>Noformējums ar blīvām krāsām, izņemot gradienta virsrakstu.</summary>
		public Brand(Color textColor, Color progressColor, Color captionTextColor,
			Color captionBackgroundStartColor, Color captionBackgroundEndColor,
			Color logoBackgroundColor, Color guideBackgroundColor) :
			this(textColor, progressColor, captionTextColor,
				new LinearGradientBrush(captionBackgroundStartColor, captionBackgroundEndColor, 90),
				new SolidColorBrush(logoBackgroundColor),
				guideBackgroundColor == logoBackgroundColor ? null:new SolidColorBrush(guideBackgroundColor)) {}
		/// <summary>Noformējums ar blīvām krāsām.</summary>
		public Brand(Color textColor, Color progressColor, Color captionTextColor,
			Color captionBackgroundColor, Color logoBackgroundColor, Color guideBackgroundColor) :
			this(textColor, progressColor, captionTextColor,
				new SolidColorBrush(captionBackgroundColor), new SolidColorBrush(logoBackgroundColor),
				guideBackgroundColor == logoBackgroundColor ? null:new SolidColorBrush(guideBackgroundColor)) {}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}