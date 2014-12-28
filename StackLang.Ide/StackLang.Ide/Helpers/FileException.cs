using System;

namespace StackLang.Ide.Helpers {
	public class FileException : Exception {
		public readonly string Filename;

		public FileException(string newFilename, string newMessage, Exception innerException)
			: base(newMessage, innerException) {
			Filename = newFilename;
		}

		public FileException(string newFilename, string newMessage)
			: base(newMessage) {
			Filename = newFilename;
		}

		public override string ToString() {
			return string.Format("Error handling file {0}\nAdditional info:\n{1}", Filename, base.ToString());
		}
	}
}