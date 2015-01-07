using System;
using StackLang.Ide.Helpers;

namespace StackLang.Ide.Model {
	public class OutputAreaModel {
		public void WriteLine(string line) {
			EventHandler<LineEventArgs> handler = WriteLineRequest;
			if (handler != null) {
				handler(this, new LineEventArgs(line));
			}
		}

		public void Clear() {
			EventHandler handler = ClearRequest;
			if (handler != null) {
				handler(this, EventArgs.Empty);
			}
		}

		public event EventHandler<LineEventArgs> WriteLineRequest;
		public event EventHandler ClearRequest;
	}
}