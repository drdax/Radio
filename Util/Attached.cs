using System;
using System.Windows;
using System.Windows.Controls;

namespace DrDax.RadioClient {
	/// <summary>Piekabināmās XAML īpašības.</summary>
	public class Attached {
		// http://www.scottlogic.co.uk/blog/colin/2011/01/automatically-showing-tooltips-on-a-trimmed-textblock-silverlight-wpf/
		#region TrimAndShowToolTip
		/// <summary>
		/// Kuro stilu lietot elementa <see cref="TextBlock"/> uzpeldošajam padomam, ja tā teksts pārsniedz redzamo elementa daļu.
		/// Uzpeldošais padoms tiek automātiski pielikts vajadzības gadījumā.
		/// </summary>
		public static readonly DependencyProperty TrimAndShowToolTipProperty=DependencyProperty.RegisterAttached("TrimAndShowToolTip",
				  typeof(Style), typeof(Attached), new PropertyMetadata(null, TrimAndShowToolTip_PropertyChanged));

		// Obligātās īpašības iestatīšanas/nolasīšanas metodes.
		public static Style GetTrimAndShowToolTip(DependencyObject obj) {
			return (Style)obj.GetValue(TrimAndShowToolTipProperty);
		}
		public static void SetTrimAndShowToolTip(DependencyObject obj, Style value) {
			obj.SetValue(TrimAndShowToolTipProperty, value);
		}

		private static void TrimAndShowToolTip_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			TextBlock textBlock=d as TextBlock;
			if (textBlock == null) return; // Ignorē ne teksta elementus.

			if (e.NewValue != null) {
				textBlock.TextTrimming=TextTrimming.WordEllipsis;
				//ComputeTrimAndShowToolTip(textBlock);
				textBlock.TargetUpdated+=textBlock_TargetUpdated; // Lai šis notiktu, Binding.NotifyOnTargetUpdated jābūt true.
			} else textBlock.TargetUpdated-=textBlock_TargetUpdated;
		}
		private static void textBlock_TargetUpdated(object sender, System.Windows.Data.DataTransferEventArgs e) {
			TextBlock textBlock=sender as TextBlock;
			ComputeTrimAndShowToolTip(textBlock);
		}

		/// <summary>
		/// Aprēķina vajadzību pēc uzpeldošā padoma un piešķir to teksta elementam <param name="textBlock"/>.
		/// </summary>
		private static void ComputeTrimAndShowToolTip(TextBlock textBlock) {
			textBlock.Measure(new Size(textBlock.TextWrapping == TextWrapping.Wrap ? textBlock.ActualWidth:Double.PositiveInfinity, Double.PositiveInfinity));
			double measuredDimension, actualDimension;
			if (textBlock.TextWrapping == TextWrapping.Wrap) {
				measuredDimension=textBlock.DesiredSize.Height;
				actualDimension=textBlock.ActualHeight;
			} else {
				measuredDimension=textBlock.DesiredSize.Width;
				actualDimension=textBlock.ActualWidth;
			}

			if (actualDimension < measuredDimension)
				textBlock.ToolTip=new ToolTip {
					Content=textBlock.Text, Style=GetTrimAndShowToolTip(textBlock)
				};
			else textBlock.ToolTip=null;
		}
		#endregion
	}
}