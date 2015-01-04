using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using StackLang.Core;
using StackLang.Ide.ViewModel;

namespace StackLang.Ide.Helpers {
	public class BoolToBrushConverter : IValueConverter {
		public Brush TrueBrush { get; set; }
		public Brush FalseBrush { get; set; }

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			return System.Convert.ToBoolean(value) ? TrueBrush : FalseBrush;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			return Equals(value, TrueBrush);
		}
	}

	public class InstructionEscapedToStringConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			return "Escaping: " + (bool) value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}

	public class NullToBoolConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			return value != null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}

	public class NullToGridLengthConverter : IValueConverter {
		GridLength lastLength;
		bool lastValueExtended;
		GridLength extendedLength = new GridLength(1, GridUnitType.Star);

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value == null) {
				if (lastValueExtended) {
					extendedLength = lastLength;
				}
				lastLength = new GridLength(0, GridUnitType.Star);
				lastValueExtended = false;
			}
			else {
				lastLength = extendedLength;
				lastValueExtended = true;
			}
			return lastLength;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}

	public class NullToVisibilityConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			return value == null ? Visibility.Collapsed : Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}

	public class BoolToHiddenVisibilityConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			return (bool) value
				? Visibility.Visible
				: Visibility.Hidden;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}

	public class StringListToStackItemListConverter : IMultiValueConverter {
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
			if (values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue) {
				return Enumerable.Empty<StackItemViewModel>();
			}
			IList<string> inputCollection = (IList<string>)values[0];
			bool useHighlight = ((ExecutionParameters.ExecutionSource) values[1] == 
				ExecutionParameters.ExecutionSource.Stack);
			return inputCollection.Select((s, i) => new StackItemViewModel(s, useHighlight &&
				i == inputCollection.Count - 1));
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}