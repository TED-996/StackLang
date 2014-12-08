using System;
using System.IO;
using System.Linq;

namespace StackLang.Interpreter {
	internal static class IntepreterProgram {
		static void Main(string[] args) {
			Stream codeStream = new FileStream((args.Length == 0 ? "defaultCode.sl" : args[0]), FileMode.Open);
			Stream inputStream = null;
			Stream outputStream = null;

			try {
				string inputArg = args.FirstOrDefault(s => s.StartsWith("-i="));
				if (inputArg != null) {
					inputStream = new FileStream(inputArg.Substring(3), FileMode.Open);
				}
				string outputArg = args.FirstOrDefault(s => s.StartsWith("-o="));
				if (outputArg != null) {
					outputStream = new FileStream(outputArg.Substring(3), FileMode.Create);
				}
			}
			catch (Exception ex) {
				Console.WriteLine("Could not open file.\n" + ex);
				return;
			}

			Interpreter interpreter = new Interpreter(codeStream, inputStream, outputStream);

			interpreter.Start();

			interpreter.Dispose();

			Console.ReadKey(true);
		}
	}
}
