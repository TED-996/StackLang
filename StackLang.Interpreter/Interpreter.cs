using System;
using System.IO;
using StackLang.Core;
using StackLang.Core.Exceptions;
using StackLang.Core.InputOutput;

namespace StackLang.Interpreter {
	public class Interpreter : IDisposable {
		ExecutionContext executionContext;

		readonly Parser parser;
		readonly IInputManager inputManager;
		readonly IOutputManager outputManager;


		public Interpreter(Stream codeSource, Stream inputStream = null, Stream outputStream = null) {
			parser = new Parser(codeSource);
			inputManager = inputStream == null ? (IInputManager) new ConsoleInputManager() : new StreamInputManager(inputStream);
			outputManager = outputStream == null ? (IOutputManager) new ConsoleOutputManager() :
				new StreamOutputManager(outputStream);
		}

		public void Start() {
			try {
				executionContext = new ExecutionContext(parser.Parse(), inputManager, outputManager);
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

		public void Dispose() {
			executionContext.Dispose();
		}
	}
}