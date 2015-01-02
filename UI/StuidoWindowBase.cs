using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace DrDax.RadioClient {
	/// <summary>Studijas attēla translācijas logs, kurš nav atkarīgs no tiešraides tehnoloģijas.</summary>
	public abstract class StudioWindowBase : ProperWindow {
		protected StudioWindowBase(string title) {
			this.Background=Brushes.Black;
			this.SizeToContent=SizeToContent.WidthAndHeight; // Atļauj logam iegūt video izmērus. Vienkāršs veids, kā pašiem nepārrēķināt pēc apmales.
			this.Loaded+=Window_Loaded;
			this.Closing+=Window_Closing;
			this.Title=title;
			this.MouseDoubleClick+=Window_MouseDoubleClick;
		}

		protected abstract void OnLoaded();
		protected abstract void OnClosing();

		private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
			this.WindowState=this.WindowState==WindowState.Maximized ? WindowState.Normal:WindowState.Maximized;
		}
		private void Window_Loaded(object sender, RoutedEventArgs e) {
			this.Loaded-=Window_Loaded;
			OnLoaded();
			// Ļauj mainīt loga izmēru kopā ar video.
			this.SizeToContent=SizeToContent.Manual;
			this.MinWidth=this.ActualWidth; this.MinHeight=this.ActualHeight;
		}
		private void Window_Closing(object sender, CancelEventArgs e) {
			this.Closing-=Window_Closing;
			this.MouseDoubleClick-=Window_MouseDoubleClick;
			OnClosing();
			window=null;
		}
		/// <summary>Atvērtā loga instance.</summary>
		protected static StudioWindowBase window;
	}
}