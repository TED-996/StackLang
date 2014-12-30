using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using StackLang.Core.InputOutput;
using StackLang.Ide.Helpers;

namespace StackLang.Ide.Model {
	public class IoSettingsModel : INotifyPropertyChanged {
		public string CodeFileName { get; set; }

		string _inputFilename = "";
		public string InputFilename {
			get { return _inputFilename; }
			set {
				if (_inputFilename == value) {
					return;
				}
				_inputFilename = value;
				RaisePropertyChanged();
			}
		}

		public string InputStartName {
			get { return string.IsNullOrWhiteSpace(InputFilename) ? CodeFileName : InputFilename; }
		}

		string _outputFilename = "";
		public string OutputFilename {
			get { return _outputFilename; }
			set {
				if (_outputFilename == value) {
					return;
				}
				_outputFilename = value;
				RaisePropertyChanged();
			}
		}

		public string OutputStartName {
			get { return string.IsNullOrWhiteSpace(OutputFilename) ? CodeFileName : OutputFilename; }
		}

		public IInputManager GetInputManager(IInputManager defaultManager) {
			if (string.IsNullOrWhiteSpace(InputFilename)) {
				return defaultManager;
			}

			string filename = InputFilename;
			if (!Path.IsPathRooted(filename)) {
				string currentDirectory = (CodeFileName == null
					? Directory.GetCurrentDirectory()
					: Path.GetDirectoryName(CodeFileName))
				    ?? Directory.GetCurrentDirectory();
				filename = Path.Combine(currentDirectory, filename);
			}
			try {
				return new StreamInputManager(new FileStream(filename, FileMode.Open, FileAccess.Read));
			}
			catch (Exception ex) {
				throw new FileException(filename, "Could not open input file.", ex);
			}
		}

		public IOutputManager GetOutputManager(IOutputManager defaultManager) {
			if (string.IsNullOrWhiteSpace(OutputFilename)) {
				return defaultManager;
			}

			string filename = OutputFilename;
			if (!Path.IsPathRooted(filename)) {
				string currentDirectory = (CodeFileName == null
					? Directory.GetCurrentDirectory()
					: Path.GetDirectoryName(CodeFileName))
					?? Directory.GetCurrentDirectory();
				filename = Path.Combine(currentDirectory, filename);
			}
			try {
				return new StreamOutputManager(new FileStream(filename, FileMode.Create, FileAccess.Write));
			}
			catch (Exception ex) {
				throw new FileException(OutputFilename, "Could not open output file.", ex);
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null) {
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) {
				handler(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}