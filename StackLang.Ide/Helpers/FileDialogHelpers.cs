using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace StackLang.Ide.Helpers {
	public static class FileDialogHelpers {
		public static string ShowOpenFileDialog() {
			OpenFileDialog dialog = new OpenFileDialog {
				CheckFileExists = true,
				Filter = "SL Files (*.sl)|*.sl|All Files (*.*)|*.*",
				Title = "Open SL file"
			};

			if (dialog.ShowDialog() != true) {
				return null;
			}
			return dialog.FileName;
		}

		public static string ShowSaveFileDialog(string startFilename) {
			string initialDirectory = string.IsNullOrWhiteSpace(startFilename)
				? Directory.GetCurrentDirectory()
				: Path.GetDirectoryName(startFilename);

			SaveFileDialog dialog = new SaveFileDialog {
				Filter = "SL Files (*.sl)|*.sl|All Files (*.*)|*.*",
				Title = "Save SL file",
				OverwritePrompt = true
			};
			if (initialDirectory != null) {
				dialog.InitialDirectory = initialDirectory;
			}
			if (startFilename != null) {
				dialog.FileName = startFilename;
			}

			if (dialog.ShowDialog() != true) {
				return null;
			}
			return dialog.FileName;
		}

		public static bool? PromptSave(string name) {
			MessageBoxResult result = MessageBox.Show("Do you want to save " + name + " before quitting?", 
				"StackLang.Ide", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes);
			if (result == MessageBoxResult.Cancel) {
				return null;
			}
			return result == MessageBoxResult.Yes;
		}

		public static string ShowInputDialog(string startFilename) {
			string initialDirectory = string.IsNullOrWhiteSpace(startFilename)
				? Directory.GetCurrentDirectory()
				: Path.GetDirectoryName(startFilename);

			OpenFileDialog dialog = new OpenFileDialog {
				Filter = "All Files (*.*)|*.*",
				Title = "Select input file",
				CheckFileExists = true
			};
			if (initialDirectory != null) {
				dialog.InitialDirectory = initialDirectory;
			}
			if (startFilename != null) {
				dialog.FileName = startFilename;
			}

			if (dialog.ShowDialog() != true) {
				return null;
			}
			return dialog.FileName;
		}

		public static string ShowOutputDialog(string startFilename) {
			string initialDirectory = string.IsNullOrWhiteSpace(startFilename)
				? Directory.GetCurrentDirectory()
				: Path.GetDirectoryName(startFilename);

			SaveFileDialog dialog = new SaveFileDialog {
				Filter = "All Files (*.*)|*.*",
				Title = "Select output file",
				OverwritePrompt = false
			};
			if (initialDirectory != null) {
				dialog.InitialDirectory = initialDirectory;
			}
			if (startFilename != null) {
				dialog.FileName = startFilename;
			}

			if (dialog.ShowDialog() != true) {
				return null;
			}
			return dialog.FileName;
		}
	}
}