using System;
using System.Linq;
using StackLang.Core.Collections;
using StackLang.Core.Exceptions;

namespace StackLang.Core {
	public class ExecutionContext {
		public readonly ExecutionParameters Parameters;
		public readonly InstructionLineCollection InstructionCollection;

		public bool ExecutionEnded { get; private set; }

		public ExecutionContext(InstructionLineCollection newInstructionCollection) {
			InstructionCollection = newInstructionCollection;
			Parameters = new ExecutionParameters(InstructionCollection.Select(line => line.Count).ToArray());
		}

		public ExecutionContext GetNewContext() {
			return new ExecutionContext(InstructionCollection);
		}

		public void Tick() {
			if (ExecutionEnded) {
				throw new ApplicationException("Execution already ended.");
			}

			Instruction instruction = null;

			if (Parameters.CurrentExecutionSource == ExecutionParameters.ExecutionSource.Stack && Parameters.Stack.Count == 0) {
				Parameters.CurrentExecutionSource = ExecutionParameters.ExecutionSource.Code;
			}

			if (Parameters.CurrentExecutionSource == ExecutionParameters.ExecutionSource.Stack) {
				InstructionObject instructionObject = Parameters.Stack.Pop() as InstructionObject;

				if (instructionObject != null) {
					instruction = instructionObject.Instruction;
				}
			}
			else {
				if (Parameters.CurrentLine >= InstructionCollection.Count) {
					ExecutionEnded = true;
					return;
				}
				instruction = InstructionCollection[Parameters.CurrentLine][Parameters.CurrentInstruction];
			}

			Parameters.TickStart();

			if (instruction != null) {
				if (Parameters.CurrentExecutionSource == ExecutionParameters.ExecutionSource.Stack || 
					!Parameters.InstructionEscaped || instruction.ForceExecutable) {
					try {
						instruction.Execute(Parameters);
					}
					catch (CodeException) {
						ExecutionEnded = true;
						throw;
					}
				}
				else {
					Parameters.Stack.Push(new InstructionObject(instruction));
				}
			}

			Parameters.TickEnd();
		}
	}
}