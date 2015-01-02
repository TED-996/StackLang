using System.Collections.ObjectModel;
using StackLang.Core;

namespace StackLang.Ide.Helpers {
	public class SnapshotWrapper {
		readonly ExecutionSnapshot snapshot;

		public ReadOnlyCollection<string> Stack { get { return snapshot.Stack; } }
		public IStackObject Register { get { return snapshot.Register; } }
		public ReadOnlyCollection<IStackObject> Memory { get { return snapshot.Memory; } }

		public int CurrentLine { get { return snapshot.CurrentLine; } }
		public int CurrentInstruction { get { return snapshot.CurrentInstruction; } }
		public ExecutionParameters.ExecutionSource CurrentExecutionSource { get { return snapshot.CurrentExecutionSource; } }

		public bool InstructionEscaped { get { return snapshot.InstructionEscaped; } }

		public SnapshotWrapper(ExecutionSnapshot newSnapshot) {
			snapshot = newSnapshot;
		}
	}
}