using System;

namespace StackLang.Core.Exceptions {
	public class CodeException : Exception {
		readonly ExecutionParameters executionParameters;

		internal CodeException(string message, ExecutionParameters newExecutionParameters)
			: base(message) {
			executionParameters = newExecutionParameters;
		}

		internal CodeException(IncompleteCodeException incompleteException, ExecutionParameters newExecutionParameters)
			: this(incompleteException.Message, newExecutionParameters) {
		}

		public override string ToString() {
			return "Error: " + Message + '\n' +
			       "On line " + (executionParameters.CurrentLine + 1) + ", instruction " + (executionParameters.CurrentInstruction + 1);
		}
	}

	public class IncompleteCodeException : ApplicationException {
		internal IncompleteCodeException(string message)
			: base(message) {
		}

		public override string ToString()
		{
			return "Error: " + Message + '\n' + "Contact the developer, more information should be available.";
		}
	}
}