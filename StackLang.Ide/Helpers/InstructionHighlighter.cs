using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Threading;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

namespace StackLang.Ide.Helpers {
	class InstructionHighlighter : DocumentColorizingTransformer {
		public bool Enabled { get; set; }
		public int Line { get; set; }
		public int Instruction { get; set; }
		public bool DimHighlight { get; set; }

		delegate void RedrawDelegate();

		RedrawDelegate redrawDelegate;

		protected override void ColorizeLine(DocumentLine line) {
			if (redrawDelegate == null) {
				redrawDelegate = CurrentContext.TextView.Redraw;
			}
			if (!Enabled || line.IsDeleted || line.LineNumber != Line) {
				return;
			}
			//There might be a better way of doing this, I'm not sure.
			int index = line.Offset;
			string text = CurrentContext.Document.Text;
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

		public void RequestRedraw() {
			if (redrawDelegate != null) {
				redrawDelegate();
			}
		}
	}

	class BreakpointHighlighter : DocumentColorizingTransformer {
		readonly ObservableCollection<int> breakpoints;

		public BreakpointHighlighter(ObservableCollection<int> newBreakpoints) {
			breakpoints = newBreakpoints;
			breakpoints.CollectionChanged += (sender, e) => RequestRedraw();
		}

		protected override void ColorizeLine(DocumentLine line) {
			if (breakpoints.Contains(line.LineNumber)) {
				ChangeLinePart(line.Offset, line.EndOffset, element => element.BackgroundBrush = Brushes.LightCoral);
			}
		}

		public void RequestRedraw() {
			if (CurrentContext != null) {
				CurrentContext.TextView.Redraw();
			}
		}
	}

}