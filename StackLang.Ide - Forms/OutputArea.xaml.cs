
using System.Windows;

namespace StackLang.Ide {
	/// <summary>
	/// Interaction logic for OutputArea.xaml
	/// </summary>
	public partial class OutputArea {
		public OutputArea() {
			InitializeComponent();
		}

		public void Clear() {
			Dispatcher.Invoke(() => OutputTextBlock.Text = "");
		}

		public void WriteLine(string line) {
			Dispatcher.Invoke(() => OutputTextBlock.Text += line + '\n');
		}

		void OnClearClick(object sender, RoutedEventArgs e) {
			Clear();
		}
	}
}
