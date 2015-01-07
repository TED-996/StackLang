using System;

namespace StackLang.Ide.Helpers {
	public class LineEventArgs : EventArgs {
		public readonly string Line;

		public LineEventArgs(string newLine) {
			Line = newLine;
		}
	}

	public class TextChangedEventArgs : EventArgs {
		public readonly string Text;

		public TextChangedEventArgs(string newText) {
			Text = newText;
		}
	}

	public class NewSnapshotEventArgs : EventArgs {
		public readonly SnapshotWrapper SnapshotWrapper;

		public NewSnapshotEventArgs(SnapshotWrapper newWrapper) {
			SnapshotWrapper = newWrapper;
		}
	}
}