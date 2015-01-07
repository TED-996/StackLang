using System.Threading;
using StackLang.Core;
using StackLang.Core.Collections;
using StackLang.Core.Exceptions;
using StackLang.Core.InputOutput;
using StackLang.Ide.Helpers;
using ExecutionContext = StackLang.Core.ExecutionContext;

namespace StackLang.Ide.Model {
	public class InterpreterModel {
		public IInputManager InputManager { get; set; }
		public IOutputManager OutputManager { get; set; }

		readonly OutputAreaModel outputAreaModel;

		public bool ExecutionRunning {
			get { return executionThread != null && executionThread.IsAlive; }
		}

		ExecutionContext context;
		Thread executionThread;

		volatile bool abortRequested;

		public InterpreterModel(OutputAreaModel newOutputAreaModel) {
			outputAreaModel = newOutputAreaModel;
		}

		public void Run(string code) {
			outputAreaModel.Clear();
			outputAreaModel.WriteLine("Building...");

			Parser parser = new Parser(code.ToMemoryStream());
			try {
				InstructionLineCollection collection = parser.Parse();
				context = new ExecutionContext(collection, InputManager, OutputManager);
			}
			catch (ParseException exception) {
				outputAreaModel.WriteLine(exception.ToString());
				return;
			}
			
			executionThread = new Thread(InterpreterThreadStart) {IsBackground = true};
			executionThread.Start();
		}

		void InterpreterThreadStart() {
			outputAreaModel.WriteLine("Execution started.");
			try {
				while (!context.ExecutionEnded) {
					context.Tick();
					if (abortRequested) {
						outputAreaModel.WriteLine("Execution aborted.");
						return;
					}
				}
			}
			catch (CodeException ex) {
				outputAreaModel.WriteLine(ex.ToString());
			}
			finally {
				context.Dispose();
			}
			outputAreaModel.WriteLine("Execution ended.");
			executionThread = null;
		}

		public void Abort() {
			if (!ExecutionRunning) {
				return;
			}
			outputAreaModel.WriteLine("Abort requested.");
			abortRequested = true;

			ExecutionAreaModel executionAreaModel = OutputManager as ExecutionAreaModel;
			if (executionAreaModel != null && executionAreaModel.IsAwaitingInput) {
				executionAreaModel.ProvideInput("0");
			}
			//Else wait.
		}
	}
}