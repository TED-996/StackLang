using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Input;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Rendering;

namespace StackLang.Ide {
	public partial class CodeTab {
		public string Filename;

		public bool Changed { get; private set; }

		public string Text {
			get { return TextBox.Text; }
			private set { TextBox.Text = value; }
		}

		public bool ReadOnly {
			get { return TextBox.IsReadOnly; }
			set { TextBox.IsReadOnly = value; }
		}

		public string TabName {
			get { return (Filename == null ? "Untitled" : Path.GetFileName(Filename)) + (Changed ? "*" : ""); }
		}

		public string InputFilename { get; set; }
		public string OutputFilename { get; set; }

		readonly List<int> breakpoints;

		readonly Action tabNameUpdateAction;
		readonly InstructionHighlighter highlighter;

		public static readonly RoutedCommand BreakpointToggle = new RoutedCommand();

		public CodeTab(Action newUpdateAction, IHighlightingDefinition highlightingDefinition) {
			InitializeComponent();
			Filename = null;
			Changed = false;
			tabNameUpdateAction = newUpdateAction;

			breakpoints = new List<int>();

			TextBox.SyntaxHighlighting = highlightingDefinition;
			highlighter = new InstructionHighlighter(TextBox);
			TextBox.TextArea.TextView.LineTransformers.Add(new BreakpointHighlighter(breakpoints));
			TextBox.TextArea.TextView.LineTransformers.Add(highlighter);

		}
		
		public CodeTab(string fileName, Action newUpdateAction, IHighlightingDefinition highlightingDefinition)
			: this(newUpdateAction, highlightingDefinition) {
			Filename = fileName;
			Load();
		}

		void Load() {
			try {
				Text = File.ReadAllText(Filename);
			}
			catch {
				throw new ApplicationException("File " + Filename + " could not be read.");
			}
			Changed = false;
			tabNameUpdateAction();
		}

		void OnTextChanged(object sender, EventArgs e) {
			Changed = true;
			tabNameUpdateAction();

			if (breakpoints.RemoveAll(b => b > TextBox.LineCount) != 0) {
				TextBox.TextArea.TextView.Redraw();
			}
		}

		void OnBreakpointTextInputPreview(object sender, TextCompositionEventArgs e) {
			int value;
			if (!string.IsNullOrEmpty(BreakpointTextBox.Text) && !int.TryParse(BreakpointTextBox.Text, out value)) {
				e.Handled = false;
			}
		}

		void OnBreakpointToggle(object sender, ExecutedRoutedEventArgs e) {
			int breakpoint;
			if (!int.TryParse(BreakpointTextBox.Text, out breakpoint)) {
				return;
			}
			if (breakpoint > 0 && breakpoint <= TextBox.LineCount) {
				if (!breakpoints.Remove(breakpoint)) {
					breakpoints.Add(breakpoint);
				}
				TextBox.TextArea.TextView.Redraw();
			}
			BreakpointTextBox.Clear();
		}

		public bool IsBreakpoint(int currentLine) {
			return breakpoints.Contains(currentLine + 1);
		}

		public void SetHighlight(int line, int instruction, bool dim) {
			highlighter.Enabled = true;
			highlighter.Line = line;
			highlighter.Instruction = instruction;
			highlighter.DimHighlight = dim;

			TextBox.TextArea.TextView.Redraw();
		}

		public void SetHighlghtEnabled(bool enabled) {
			highlighter.Enabled = enabled;

			TextBox.TextArea.TextView.Redraw();
		}

		public void SaveAs(string filename) {
			Filename = filename;
			
			Save();
		}

		public void Save() {
			try {
				File.WriteAllText(Filename, Text);
			}
			catch {
				throw new ApplicationException("Could not save to file " + Filename + ".");
			}

			Changed = false;
			tabNameUpdateAction();
		}

		class InstructionHighlighter : DocumentColorizingTransformer {
			public bool Enabled { get; set; }
			public int Line { get; set; }
			public int Instruction { get; set; }
			public bool DimHighlight { get; set; }

			readonly TextEditor textBox;

			public InstructionHighlighter(TextEditor newTextBox) {
				textBox = newTextBox;
			}

			protected override void ColorizeLine(DocumentLine line) {
				if (!Enabled || line.IsDeleted || line.LineNumber != Line) {
					return;
				}
				//There might be a better way of doing this, I'm not sure.
				int index = line.Offset;
				string text = textBox.Text;
				while (char.IsWhiteSpace(text[index])) {
					index++;
				}

				int instructionsLeft = Instruction;
				while (instructionsLeft != 0) {
					index++;
					if (char.IsWhiteSpace(text[index]) && !char.IsWhiteSpace(text[index - 1])) {
						//An instruction just ended
						instructionsLeft--;
					}
				}

				//Skipping the whitespace
				while (char.IsWhiteSpace(text[index])) {
					index++;
				}
				int startIndex = index;

				while (index < line.EndOffset && !char.IsWhiteSpace(text[index])) {
					index++;
				}

				ChangeLinePart(startIndex, index, element => {
					element.BackgroundBrush = DimHighlight ? Brushes.LightYellow : Brushes.Yellow;
				});
			}
		}

		class BreakpointHighlighter : DocumentColorizingTransformer {
			readonly List<int> breakpoints;

			public BreakpointHighlighter(List<int> newBreakpoints) {
				breakpoints = newBreakpoints;
			}

			protected override void ColorizeLine(DocumentLine line) {
				if (breakpoints.Contains(line.LineNumber)) {
					ChangeLinePart(line.Offset, line.EndOffset, element => element.BackgroundBrush = Brushes.LightCoral);
				}
			}
		}
	}
}
