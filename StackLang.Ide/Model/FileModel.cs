using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using StackLang.Ide.Annotations;
using StackLang.Ide.Helpers;
using StackLang.Ide.ViewModel;

namespace StackLang.Ide.Model {
	public sealed class FileModel : INotifyPropertyChanged {
		string _filename;

		public string Filename {
			get { return _filename; }
			set {
				if (_filename == value) {
					return;
				}
				_filename = value;
				RaisePropertyChanged();
				RaisePropertyChanged("DisplayName");
			}
		}

		public string DisplayName {
			get {
				return Filename == null
					? "Untitled"
					: Path.GetFileName(Filename);
			}
		}

		public string Text { get; private set; }

		public FileModel() {
			Filename = null;
			Text = "";
		}

		public FileModel(string newFilename, string newText) {
			Filename = newFilename;
			Text = newText;
		}

		public void Save(string newFilename = null) {
			if (newFilename != null) {
				Filename = newFilename;
			}

			try {
				File.WriteAllText(Filename, Text);
			}
			catch (Exception ex) {
				throw new FileException(Filename, "Could not save to file.", ex);
			}
		}

		public void OnEditorTabTextChanged(object s, TextChangedEventArgs e) {
			Text = e.Text;
		}

		public event PropertyChangedEventHandler PropertyChanged;
		[NotifyPropertyChangedInvocator]
		void RaisePropertyChanged([CallerMemberName] string propertyName = null) {
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) {
				handler(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		public static FileModel LoadFromFile(string filename) {
			string text;
			try {
				text = File.ReadAllText(filename);
			}
			catch (Exception ex) {
				throw new FileException(filename, "Could not read from file.", ex);
			}
			return new FileModel(filename, text);
		}
	}
}