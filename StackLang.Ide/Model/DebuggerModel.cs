using System;
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

		readonly OutputAreaModel outputAreaModel;

		public bool ExecutionRunning { get; private set; }
		public bool StepRunning {
			get {
				return stepTask != null &&
					(stepTask.Status == TaskStatus.Running || stepTask.Status == TaskStatus.WaitingToRun ||
					   stepTask.Status == TaskStatus.WaitingForActivation);
			}
		}

		volatile bool aborted;

		public SnapshotWrapper Snapshot { get; private set; }

		ExecutionContext context;
		Task stepTask;

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
			stepTask = new Task(Tick);
			stepTask.Start();
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
					if (!stepTask.Wait(2000)) {
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
			if (executionAreaModel != null) {
				executionAreaModel.ProvideInput("0");
			}
			//Else wait.
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