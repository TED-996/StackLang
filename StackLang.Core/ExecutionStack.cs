using System.Collections.Generic;

namespace StackLang.Core {
	public class ExecutionStack : Stack<IStackObject> {
		public new IStackObject Pop() {
			if (Count == 0) {
				throw new IncompleteCodeException("Stack empty, expected value.");
			}
			return base.Pop();
		}
	}
}