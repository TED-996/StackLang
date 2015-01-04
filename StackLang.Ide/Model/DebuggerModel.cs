using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using StackLang.Core;
using StackLang.Core.Collections;
using StackLang.Core.Exceptions;
using StackLang.Core.InputOutput;
using StackLang.Ide.Helpers;
using StackLang.Ide.ViewModel;

namespace StackLang.Ide.Model {
	public class DebuggerModel {
		public IInputManager InputManager { get; set; }
		public IOutputManager OutputManager { get; set; }

		public IList<int> Breakpoints { get; set; } 

		readonly OutputAreaModel outputAreaModel;

		public bool ExecutionRunning { get; private set; }
		public bool StepRunning {
			get {
				return runTask != null &&
					(runTask.Status == TaskStatus.Running || runTask.Status == TaskStatus.WaitingToRun ||
					   runTask.Status == TaskStatus.WaitingForActivation);
			}
		}

		volatile bool aborted;
		volatile bool paused;

		public bool InContinue { get; private set; }

		public SnapshotWrapper Snapshot { get; private set; }

		ExecutionContext context;
		Task runTask;

		readonly object executionDisposeLock = new object();

		public DebuggerModel(OutputAreaModel newOutputAreaModel) {
			outputAreaModel = newOutputAreaModel;
		}

		public void Start(EditorTabViewModel tabViewModel) {
			outputAreaModel.Clear();
			outputAreaModel.WriteLine("Building...");

			Parser parser = new Parser(tabViewModel.Text.ToMemoryStream());
			try {
				InstructionLineCollection collection = parser.Parse();
				context = new ExecutionContext(collection, InputManager, OutputManager);
			}
			catch (ParseException exception) {
				outputAreaModel.WriteLine(exception.ToString());
				return;
			}

			DebugStart += tabViewModel.OnDebugStart;

			outputAreaModel.WriteLine("Debug started.");
			ExecutionRunning = true;
			aborted = false;
			RaiseDebugStart();

			Snapshot = new SnapshotWrapper(context.Parameters.GetSnapshot());
			RaiseNewSnapshot();
		}

		public void Step() {
			runTask = new Task(Tick);
			runTask.Start();
		}

		void Tick() {
			if (!ExecutionRunning) {
				throw new ApplicationException("Step called with execution stopped.");
			}
			try {
				context.Tick();
			}
			catch (CodeException ex) {
				outputAreaModel.WriteLine(ex.ToString());
				OnExecutionEnd();
			}

			if (aborted) {
				return;
			}

			if (context.ExecutionEnded) {
				OnExecutionEnd();
				outputAreaModel.WriteLine("Debug ended.");
			}
			else {
				Application.Current.Dispatcher.Invoke(() => {
					Snapshot = new SnapshotWrapper(context.Parameters.GetSnapshot());
					RaiseNewSnapshot();
				});
			}
		}

		public void Continue() {
			paused = false;
			InContinue = true;

			runTask = new Task(() => {
				if (!ExecutionRunning) {
					throw new ApplicationException("Step called with execution stopped.");
				}
				try {
					while (!context.ExecutionEnded && !paused && !aborted) {
						context.Tick();
						if (Breakpoints.Contains(context.Parameters.CurrentLine + 1)
						    && context.Parameters.CurrentInstruction == 0) {
							outputAreaModel.WriteLine("Breakpoint hit.");
							break;
						}
					}
				}
				catch (CodeException ex) {
					outputAreaModel.WriteLine(ex.ToString());
					OnExecutionEnd();
				}

				if (aborted) {
					return;
				}

				if (ExecutionRunning && paused) {
					outputAreaModel.WriteLine("Debug paused.");
				}

				if (context.ExecutionEnded) {
					OnExecutionEnd();
					outputAreaModel.WriteLine("Debug ended.");
				}
				else {
					Application.Current.Dispatcher.Invoke(() => {
						Snapshot = new SnapshotWrapper(context.Parameters.GetSnapshot());
						RaiseNewSnapshot();
					});
				}

				InContinue = false;
			});
			runTask.Start();
		}

		void OnExecutionEnd() {
			ExecutionRunning = false;
			lock (executionDisposeLock) {
				if (context != null) {
					context.Dispose();
					context = null;
					Application.Current.Dispatcher.Invoke(RaiseDebugEnd);
				}
			}
		}

		public void Abort() {
			Task.Run(() => {
				if (StepRunning) {
					Cancel();
					if (!runTask.Wait(2000)) {
						outputAreaModel.WriteLine("Debug abort failed.");
						return;
					}
				}
				OnExecutionEnd();
				outputAreaModel.WriteLine("Debug aborted.");
			});
		}

		void Cancel() {
			aborted = true;
			ExecutionAreaModel executionAreaModel = OutputManager as ExecutionAreaModel;
			if (executionAreaModel != null && executionAreaModel.IsAwaitingInput) {
				executionAreaModel.ProvideInput("0");
			}
			//Else wait.
		}

		public void Pause() {
			paused = true;
			outputAreaModel.WriteLine("Pause requested. Waiting to finish instruction.");
		}

		public event EventHandler<NewSnapshotEventArgs> NewSnapshot;
		void RaiseNewSnapshot() {
			EventHandler<NewSnapshotEventArgs> handler = NewSnapshot;
			if (handler != null) {
				handler(this, new NewSnapshotEventArgs(Snapshot));
			}
		}

		public event EventHandler DebugStart;
		void RaiseDebugStart() {
			EventHandler handler = DebugStart;
			if (handler != null) {
				handler(this, EventArgs.Empty);
			}
		}

		public event EventHandler DebugEnd;
		void RaiseDebugEnd() {
			EventHandler handler = DebugEnd;
			if (handler != null) {
				handler(this, EventArgs.Empty);
			}
		}
	}
}