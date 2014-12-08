using System;

namespace StackLang.Core.InputOutput {
	public class ConsoleInputManager : IInputManager {
		public string ReadLine() {
			Console.ForegroundColor = ConsoleColor.Red;
			Console.Write("<<: ");
			Console.ForegroundColor = ConsoleColor.White;

			return Console.ReadLine();
		}


		public void Dispose() {
		}
	}

	public class ConsoleOutputManager : IOutputManager {
		public void WriteLine(string line) {
			Console.WriteLine(line);
		}

		public void Dispose() {
		}
	}
}