using System.IO;
using StackLang.Core.Exceptions;

namespace StackLang.Core.InputOutput {
	public class StreamInputManager : IInputManager {
		readonly StreamReader reader;

		public StreamInputManager(Stream stream) {
			reader = new StreamReader(stream);
		}

		public string ReadLine() {
			try {
				return reader.ReadLine();
			}
			catch {
				throw new IncompleteCodeException("Could not read from input.");
			}
		}

		public void Dispose() {
			reader.Dispose();
		}
	}

	public class StreamOutputManager : IOutputManager {
		readonly StreamWriter writer;

		public StreamOutputManager(Stream stream) {
			writer = new StreamWriter(stream);
		}

		public void WriteLine(string line) {
			try {
				writer.WriteLine(line);
			}
			catch {
				throw new IncompleteCodeException("Could not write to output.");
			}
		}

		public void Dispose() {
			writer.Dispose();
		}
	}
}