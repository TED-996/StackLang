using System;

namespace StackLang.Ide.Helpers {
	public class LineEventArgs : EventArgs {
		public readonly string Line;

		public LineEventArgs(string newLine) {
			Line = newLine;
		}
	}
}