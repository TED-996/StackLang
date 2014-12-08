using System.IO;

namespace StackLang.Core.InputOutput {
	public class StreamInputManager : IInputManager {
		readonly StreamReader reader;

		public StreamInputManager(Stream stream) {
			reader = new StreamReader(stream);
		}

		public string ReadLine() {
			return reader.ReadLine();
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
			writer.WriteLine(line);
		}

		public void Dispose() {
			writer.Dispose();
		}
	}
}