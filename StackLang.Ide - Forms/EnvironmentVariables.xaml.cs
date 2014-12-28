using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace StackLang.Ide {
	public partial class EnvironmentVariables {
		CodeTab currentTab;

		string InputFilename {
			get { return currentTab.InputFilename; }
			set { currentTab.InputFilename = value.Trim(); }
		}

		string OutputFilename {
			get { return currentTab.OutputFilename; }
			set { currentTab.OutputFilename = value.Trim(); }
		}

		public EnvironmentVariables() {
			InitializeComponent();
		}

		public void SetTab(CodeTab newTab) {
			currentTab = newTab;
			if (currentTab == null) {
				InFileTextBox.IsEnabled = false;
				OutFileTextBox.IsEnabled = false;
			}
			else {
				InFileTextBox.IsEnabled = true;
				OutFileTextBox.IsEnabled = true;
				InFileTextBox.Text = InputFilename;
				OutFileTextBox.Text = OutputFilename;
			}

		}

		void OnInFileChanged(object sender, TextChangedEventArgs e) {
			InputFilename = InFileTextBox.Text;
		}

		void OnOutFileChanged(object sender, TextChangedEventArgs e) {
			OutputFilename = OutFileTextBox.Text;
		}

		void OnInputBrowseClick(object sender, RoutedEventArgs e) {
			string directory = currentTab.Filename == null
				? Directory.GetCurrentDirectory()
				: Path.GetDirectoryName(currentTab.Filename);
			OpenFileDialog dialog = new OpenFileDialog {CheckFileExists = true};
			if (directory != null) {
				dialog.InitialDirectory = directory;
			}

			bool? result = dialog.ShowDialog();
			if (result == true) {
				InFileTextBox.Text = dialog.FileName;
			}
		}

		void OnOutputBrowseClick(object sender, RoutedEventArgs e) {
			string directory = currentTab.Filename == null
				? Directory.GetCurrentDirectory()
				: Path.GetDirectoryName(currentTab.Filename);
			SaveFileDialog dialog = new SaveFileDialog {OverwritePrompt = true};
			if (directory != null) {
				dialog.InitialDirectory = directory;
			}

			bool? result = dialog.ShowDialog();
			if (result == true) {
				OutFileTextBox.Text = dialog.FileName;
			}
		}
	}
}
