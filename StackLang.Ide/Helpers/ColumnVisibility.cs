using System.Windows;
using System.Windows.Controls;

namespace StackLang.Ide.Helpers {
	public class ColumnVisibleDefinition : ColumnDefinition {
		public static readonly DependencyProperty VisibleProperty = DependencyProperty.Register(
			"Visible", typeof (bool), typeof (ColumnVisibleDefinition), new PropertyMetadata(true, 
				OnVisibleChanged));

		public bool Visible {
			get { return (bool) GetValue(VisibleProperty); }
			set { SetValue(VisibleProperty, value); }
		}

		public static void SetVisible(DependencyObject obj, bool newVisible) {
			obj.SetValue(VisibleProperty, newVisible);
		}

		public static bool GetVisible(DependencyObject obj) {
			return (bool) obj.GetValue(VisibleProperty);
		}

		static void OnVisibleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
			obj.CoerceValue(WidthProperty);
		}

		static ColumnVisibleDefinition()  {
			ColumnDefinition.WidthProperty.OverrideMetadata(typeof(ColumnVisibleDefinition),
				new FrameworkPropertyMetadata(new GridLength(1, GridUnitType.Star), null, CoerceWidth));
		}

		static object CoerceWidth(DependencyObject obj, object basevalue) {
			return (((ColumnVisibleDefinition) obj).Visible ? basevalue : new GridLength(0));
		}
	}
}