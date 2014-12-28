using System;
using System.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using StackLang.Ide.Model;

namespace StackLang.Ide.ViewModel {
	public sealed class EditorTabViewModel : ViewModelBase {
		readonly FileModel fileModel;

		public string Name {
			get { return fileModel.DisplayName + (TextModified ? " *" : ""); }
		}

		public bool FileUntitled {
			get { return fileModel.Filename == null; }
		}

		bool _textModified;

		public bool TextModified {
			get { return _textModified; }
			set {
				if (_textModified == value) {
					return;
				}
				_textModified = value;
				RaisePropertyChanged();

				RaisePropertyChanged("Name");
			}
		}

		TextDocument _document = new TextDocument();
		public TextDocument Document {
			get { return _document; }
			set {
				if (_document == value) {
					return;
				}
				_document = value;
				RaisePropertyChanged();
			}
		}

		IHighlightingDefinition _highlighting = HighlightingManager.Instance.HighlightingDefinitions.First(
				definition => definition.Name == "C#");
		public IHighlightingDefinition Highlighting {
			get { return _highlighting; }
			set {
				if (_highlighting == value) {
					return;
				}
				_highlighting = value;
				RaisePropertyChanged();
			}
		}

		RelayCommand textChangedCommand;
		public RelayCommand TextChangedCommand {
			get {
				return textChangedCommand
				       ?? (textChangedCommand = new RelayCommand(OnTextChanged));
			}
		}

		RelayCommand removeCommand;
		public RelayCommand RemoveCommand {
			get {
				return removeCommand
				       ?? (removeCommand = new RelayCommand(OnRequestRemove));
			}
		}

		public event EventHandler RequestRemove;
		void OnRequestRemove() {
			EventHandler ev = RequestRemove;
			if (ev != null) {
				ev(this, EventArgs.Empty);
			}
		}

		public EditorTabViewModel() {
			fileModel = new FileModel();
			TextChanged += fileModel.OnEditorTabTextChanged;
		}

		EditorTabViewModel(FileModel newFileModel) {
			fileModel = newFileModel;
			TextChanged += fileModel.OnEditorTabTextChanged;

			Document.Text = fileModel.Text;
			TextModified = false;
		}

		public void Save(string filename) {
			if (FileUntitled && filename == null) {
				throw new InvalidOperationException("Cannot save to an untitled file.");
			}
			fileModel.Save(filename);
			TextModified = false;

			RaisePropertyChanged("FileUntitled");
		}

		void OnTextChanged() {
			RaiseTextChanged();
		}

		EventHandler<TextChangedEventArgs> TextChanged;
		void RaiseTextChanged() {
			EventHandler<TextChangedEventArgs> ev = TextChanged;
			if (ev != null) {
				ev(this, new TextChangedEventArgs(Document.Text));
			}
		}

		public static EditorTabViewModel LoadFromFile(string filename) {
			return new EditorTabViewModel(FileModel.LoadFromFile(filename));
		}
	}

	public class TextChangedEventArgs : EventArgs {
		public readonly string Text;

		public TextChangedEventArgs(string newText) {
			Text = newText;
		}
	}
}