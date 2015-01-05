using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using StackLang.Ide.Annotations;
using StackLang.Ide.Helpers;

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

		byte[] lastHash;

		public FileModel(string newFilename = null, string newText = "") {
			Filename = newFilename;
			Text = newText;

			SetFileHash();
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
			SetFileHash();
		}

		void SetFileHash() {
			if (Filename == null) {
				lastHash = null;
				return;
			}
			HashAlgorithm hash = HashAlgorithm.Create();
			using (FileStream stream = new FileStream(Filename, FileMode.Open, FileAccess.Read)) {
				lastHash = hash.ComputeHash(stream);
			}
		}

		public bool IsFileChanged() {
			byte[] oldHash = lastHash;
			SetFileHash();

			//We do this to eliminate nulls, but stil be able to check their equality.
			return !(oldHash.NullToEmptyCollection()).SequenceEqual(lastHash.NullToEmptyCollection());
		}

		public void OnEditorTabTextChanged(object s, TextChangedEventArgs e) {
			Text = e.Text;
		}

		public void Reload() {
			if (Filename == null) {
				throw new ApplicationException("Attempted reload from null.");
			}
			try {
				Text = File.ReadAllText(Filename);
			}
			catch (Exception ex) {
				throw new FileException(Filename, "Could not reload from file.", ex);
			}
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