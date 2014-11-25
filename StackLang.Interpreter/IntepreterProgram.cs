using System;
using System.IO;

namespace StackLang.Interpreter {
	static class IntepreterProgram {
		static void Main(string[] args) {
			Stream stream = new FileStream((args.Length == 0 ? "defaultCode.sl" : args[0]), FileMode.Open);

			new Interpreter(stream).Start();

			Console.ReadKey(true);
		}
	}
}

