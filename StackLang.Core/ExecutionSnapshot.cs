using System.Collections.ObjectModel;
using System.Linq;

namespace StackLang.Core {
	public class ExecutionSnapshot {
		public readonly ReadOnlyCollection<string> Stack;
		public readonly IStackObject Register;
		public readonly ReadOnlyCollection<IStackObject> Memory;

		public readonly int CurrentLine;
		public readonly int CurrentInstruction;
		public readonly ExecutionParameters.ExecutionSource CurrentExecutionSource;

		public readonly bool InstructionEscaped;

		internal ExecutionSnapshot(ExecutionParameters parameters) {
			Stack = new ReadOnlyCollection<string>(parameters.Stack.Reverse().
				Select(o => o.ToString()).ToList());
			Register = parameters.Memory.Register;
			Memory = new ReadOnlyCollection<IStackObject>(parameters.Memory.MemoryArea.ToList());

			CurrentLine = parameters.CurrentLine;
			CurrentInstruction = parameters.CurrentInstruction;
			CurrentExecutionSource = parameters.CurrentExecutionSource;
			InstructionEscaped = parameters.InstructionEscaped;
		}
	}
}