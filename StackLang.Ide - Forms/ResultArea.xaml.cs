using System.Threading;
using System.Windows.Input;
using System.Windows.Media;
using StackLang.Core.InputOutput;

namespace StackLang.Ide {
	/// <summary>
	/// Interaction logic for ResultArea.xaml
	/// </summary>
	public partial class ResultArea : IInputManager, IOutputManager {
		volatile string inputValue;

		public ResultArea() {
			InitializeComponent();

			inputValue = null;
		}

		void OnInputKeyDown(object sender, KeyEventArgs e) {
			if (e.Key == Key.Return) {
				inputValue = InputTextBox.Text;
				InputTextBox.Clear();
			}
		}

		public void WriteLine(string line) {
			Dispatcher.Invoke(() => OutputTextBlock.Text += line + '\n');
		}

		public string ReadLine() {
			Brush currentBrush = null;
			try {
				Dispatcher.Invoke(delegate {
					currentBrush = InputTextBox.Background;
					InputTextBox.Background = Brushes.LightCoral;
					FocusManager.SetFocusedElement(this, InputTextBox);
				});
				inputValue = null;

				while (inputValue == null) {
					Thread.Sleep(20);
				}
			}
			finally {
				if (currentBrush != null) {
					Dispatcher.Invoke(() => InputTextBox.Background = currentBrush);
				}
			}
			return inputValue;
		}

		public void Dispose() {
			
		}

		public void Clear() {
			Dispatcher.Invoke(() => OutputTextBlock.Text = "");
		}
	}
}
