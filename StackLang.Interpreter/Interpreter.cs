using System;
using System.IO;
using StackLang.Core;
using StackLang.Core.Exceptions;

namespace StackLang.Interpreter {
	public class Interpreter {
		readonly Stream codeSource;
		ExecutionContext executionContext;

		public Interpreter(Stream newCodeSource) {
			codeSource = newCodeSource;
		}

		public void Start() {
			try {
				executionContext = new ExecutionContext(new Parser(codeSource).Parse());
			}
			catch (ParseException ex) {
				Console.WriteLine(ex);
				return;
			}

			while (!executionContext.ExecutionEnded) {
				try {
					executionContext.Tick();
				}
				catch (CodeException ex) {
					Console.WriteLine(ex);
					return;
				}
			}

			Console.WriteLine("Execution ended.");
		}
	}
}