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
			if (InnerException != null) {
				return string.Format("Error handling file {0} : {1}\nAdditional info: {2}",
					Filename, Message, InnerException.Message);
			}
			return string.Format("Error handling file {0} : {1}", Filename, Message);
		}
	}
}