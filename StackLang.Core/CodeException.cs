using System;

namespace StackLang.Core {
	public class CodeException : ApplicationException {
		readonly ExecutionParameters executionParameters;

		public CodeException(string message, ExecutionParameters newExecutionParameters) : base(message) {
			executionParameters = newExecutionParameters;
		}

		public CodeException(IncompleteCodeException incompleteException, ExecutionParameters newExecutionParameters)
			: this(incompleteException.Message, newExecutionParameters) {
		}

		public override string ToString() {
			return "Error: " + Message + '\n' +
			       "On line" + executionParameters.CurrentLine + ", instruction " + executionParameters.CurrentInstruction;
		}
	}

	public class IncompleteCodeException : ApplicationException {
		public IncompleteCodeException(string message)
			: base(message) {
		}

		public override string ToString()
		{
			return "Error: " + Message + '\n' + "Contact the developer, more information should be available.";
		}
	}
}