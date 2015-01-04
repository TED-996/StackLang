using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using GalaSoft.MvvmLight.CommandWpf;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Rendering;
using StackLang.Core;
using StackLang.Ide.Helpers;
using StackLang.Ide.Model;
using StackLang.Ide.MVVMEnhancements;

namespace StackLang.Ide.ViewModel {
	public sealed class EditorTabViewModel : ViewModelBaseEnhanced {
		readonly FileModel fileModel;

		public readonly IoSettingsModel IoSettingsModel;

		public readonly ObservableCollection<int> Breakpoints;
		readonly InstructionHighlighter highlighter;

		public string Name {
			get { return fileModel.DisplayName + (TextModified ? " *" : ""); }
		}

		bool _textModified;
		bool TextModified {
			get { return _textModified; }
			set {
				if (_textModified == value) {
					return;
				}
				_textModified = value;

				RaisePropertyChanged("Name");
			}
		}

		string text = "";
		public string Text {
			get { return text; }
			set {
				if (text == value) {
					return;
				}
				text = value;
				RaisePropertyChanged();
			}
		}

		IHighlightingDefinition _highlightingDefinition;
		public IHighlightingDefinition HighlightingDefinition {
			get { return _highlightingDefinition; }
			set {
				if (_highlightingDefinition == value) {
					return;
				}
				_highlightingDefinition = value;
				RaisePropertyChanged();
			}
		}

		List<DocumentColorizingTransformer> _transformers;
		public List<DocumentColorizingTransformer> Transformers {
			get { return _transformers; }
			set {
				if (_transformers == value) {
					return;
				}
				_transformers = value;
				RaisePropertyChanged();
			}
		}

		bool _inDebug;
		public bool InDebug {
			get { return _inDebug; }
			set {
				if (_inDebug == value) {
					return;
				}
				_inDebug = value;
				RaisePropertyChanged();
			}
		}

		bool _breakpointsVisible;
		public bool BreakpointsVisible {
			get { return _breakpointsVisible; }
			set {
				if (_breakpointsVisible == value) {
					return;
				}
				_breakpointsVisible = value;
				RaisePropertyChanged();
			}
		}

		string _breakpointText;
		public string BreakpointText {
			get { return _breakpointText; }
			set {
				if (_breakpointText == value) {
					return;
				}
				_breakpointText = value;
				RaisePropertyChanged();
			}
		}

		public RelayCommand TextChangedCommand {
			get { return GetRelayCommand(OnTextChanged); }
		}
		public RelayCommand RemoveCommand {
			get {
				return GetRelayCommand(RaiseRequestRemove);
			}
		}
		public RelayCommand ToggleBreakpointCommand {
			get {
				return GetRelayCommand(ToggleBreakpoint, () => BreakpointsVisible && !string.IsNullOrEmpty(BreakpointText));
			}
		}
		public RelayCommand ToggleBreakpointsVisibleCommand {
			get { return GetRelayCommand(() => { BreakpointsVisible = !BreakpointsVisible; }); }
		}

		public EditorTabViewModel(IHighlightingDefinition newSyntaxDefinition) : this(newSyntaxDefinition, new FileModel()) {
			TextChanged += fileModel.OnEditorTabTextChanged;
			fileModel.PropertyChanged += OnFileModelPropertyChanged;
		}

		EditorTabViewModel(IHighlightingDefinition newSyntaxDefinition, FileModel newFileModel) {
			fileModel = newFileModel;
			TextChanged += fileModel.OnEditorTabTextChanged;
			fileModel.PropertyChanged += OnFileModelPropertyChanged;

			Text = fileModel.Text;
			TextModified = false;

			HighlightingDefinition = newSyntaxDefinition;

			IoSettingsModel = new IoSettingsModel {CodeFileName = fileModel.Filename};

			highlighter = new InstructionHighlighter();
			Breakpoints = new ObservableCollection<int>();
			Transformers = new List<DocumentColorizingTransformer> {
				new BreakpointHighlighter(Breakpoints),
				highlighter
			};
		}

		void OnFileModelPropertyChanged(object s, PropertyChangedEventArgs e) {
			if (e.PropertyName == "Filename" || e.PropertyName == "DisplayName") {
				RaisePropertyChanged("Name");
				IoSettingsModel.CodeFileName = fileModel.Filename;
			}
		}

		public bool Save(bool saveAs = false) {
			string filename = fileModel.Filename;

			if (saveAs || filename == null) {
				filename = FileDialogHelpers.ShowSaveFileDialog(filename);
				if (filename == null) {
					return false;
				}
			}

			fileModel.Save(filename);

			TextModified = false;
			return true;
		}

		void ToggleBreakpoint() {
			int value = int.Parse(BreakpointText);
			BreakpointText = "";

			if (value <= 0 || value > Text.Count(c => c == '\n') + 1) {
				return;
			}

			if (!Breakpoints.Remove(value)) {
				Breakpoints.Add(value);
			}
		}

		public void OnDebugStart(object sender, EventArgs e) {
			DebuggerModel debuggerModel = (DebuggerModel) sender;
			debuggerModel.DebugStart -= OnDebugStart;
			debuggerModel.NewSnapshot += OnNewSnapshot;
			debuggerModel.DebugEnd += OnDebugEnd;

			InDebug = true;
		}

		void OnNewSnapshot(object sender, NewSnapshotEventArgs e) {
			SnapshotWrapper snapshot = e.SnapshotWrapper;
			highlighter.Enabled = true;
			highlighter.Line = snapshot.CurrentLine + 1;
			highlighter.Instruction = snapshot.CurrentInstruction;
			highlighter.DimHighlight = snapshot.CurrentExecutionSource == ExecutionParameters.ExecutionSource.Stack;

			highlighter.RequestRedraw();
		}

		void OnDebugEnd(object sender, EventArgs e) {
			highlighter.Enabled = false;
			InDebug = false;

			highlighter.RequestRedraw();

			DebuggerModel debuggerModel = (DebuggerModel)sender;
			debuggerModel.NewSnapshot -= OnNewSnapshot;
			debuggerModel.DebugEnd -= OnDebugEnd;

		}

		public event EventHandler RequestRemove;
		public void RaiseRequestRemove() {
			if (TextModified) {
				bool? result = FileDialogHelpers.PromptSave(Name);
				if (result == null) {
					return;
				}
				if (result == true) {
					Save();
				}
			}
			EventHandler ev = RequestRemove;
			if (ev != null) {
				ev(this, EventArgs.Empty);
			}
		}

		void OnTextChanged() {
			TextModified = true;

			foreach (int breakpoint in Breakpoints.Where(b => b > Text.Count(c => c == '\n') + 1).ToList()) {
				Breakpoints.Remove(breakpoint);
			}

			RaiseTextChanged();
		}

		EventHandler<TextChangedEventArgs> TextChanged;
		void RaiseTextChanged() {
			EventHandler<TextChangedEventArgs> ev = TextChanged;
			if (ev != null) {
				ev(this, new TextChangedEventArgs(Text));
			}
		}

		public static EditorTabViewModel Open(IHighlightingDefinition syntaxDefinition) {
			string filename = FileDialogHelpers.ShowOpenFileDialog();
			if (filename == null) {
				return null;
			}
			return new EditorTabViewModel(syntaxDefinition, FileModel.LoadFromFile(filename));
		}
	}
}