using System;
using System.IO;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Rendering;

namespace StackLang.Ide {
	/// <summary>
	/// Interaction logic for CodeTab.xaml
	/// </summary>
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

		readonly Action tabNameUpdateAction;
		readonly InstructionHighlighter highlighter;

		public CodeTab(Action newUpdateAction, IHighlightingDefinition highlightingDefinition) {
			InitializeComponent();
			Filename = null;
			Changed = false;
			tabNameUpdateAction = newUpdateAction;

			TextBox.SyntaxHighlighting = highlightingDefinition;
			highlighter = new InstructionHighlighter(TextBox);
			TextBox.TextArea.TextView.LineTransformers.Add(highlighter);
		}

		public CodeTab(string fileName, Action newUpdateAction, IHighlightingDefinition highlightingDefinition)
			: this(newUpdateAction, highlightingDefinition) {
			Filename = fileName;
			Load();
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
		}

		void Load() {
			try {
				Text = File.ReadAllText(Filename);
			}
			catch {
				throw new ApplicationException("File " + Filename + " could not be read.");
			}
			tabNameUpdateAction();
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

		void OnTextChanged(object sender, EventArgs e) {
			Changed = true;
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
				if (line.IsDeleted || line.LineNumber != Line) {
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
	}
}
