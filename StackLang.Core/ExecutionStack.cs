using System.Collections.Generic;
using System.Linq;
using StackLang.Core.Exceptions;

namespace StackLang.Core {
	public class ExecutionStack : Stack<IStackObject> {
		internal ExecutionStack() {
		}

		ExecutionStack(IEnumerable<IStackObject> source) : base(source.Reverse()) {
		}

		internal new IStackObject Pop() {
			if (Count == 0) {
				throw new IncompleteCodeException("Stack empty, expected value.");
			}
			return base.Pop();
		}

		internal ExecutionStack Clone() {
			return new ExecutionStack(this);
		}
	}
}