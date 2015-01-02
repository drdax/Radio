using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DrDax.RadioClient {
	/// <summary>Attēlo atskaņošanas signāla līmeni.</summary>
	public partial class PlaybackStateDisplay : UserControl {
		public PlaybackState State {
			get { return (PlaybackState)GetValue(StateProperty); }
			set { SetValue(StateProperty, value); }
		}
		public static readonly DependencyProperty StateProperty=DependencyProperty.Register("State", typeof(PlaybackState), typeof(PlaybackStateDisplay));
		public PlaybackStateDisplay() {
			InitializeComponent();
		}
	}

	public class PlaybackStateToOpacityConverter : IValueConverter {
		/// <summary>Pārveido signāla līmeni indikatora caurspīdīgumā.</summary>
		/// <param name="value">(<see cref="PlaybackState"/>) signāla līmenis.</param>
		/// <param name="targetType">double</param>
		/// <param name="parameter">Signāla līmenis, kuru attēlo indikators. <see cref="PlaybackState"/> kā skaitlis.</param>
		/// <returns>Caurspīdīgums, kurš atbilst ieslēgtam (1), vai izslēgtam signāla indikatoram (0.3).</returns>
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			byte state=(byte)value, targetState=byte.Parse((string)parameter); // XAML saistīšanas gadījumā parameter ir string.
			return state < targetState ? 0.3d:1d;
		}
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}