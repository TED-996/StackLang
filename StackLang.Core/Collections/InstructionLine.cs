using System.Collections.Generic;
using System.Collections.ObjectModel;
using StackLang.Core.Exceptions;

namespace StackLang.Core.Collections {
	public class InstructionLine : ReadOnlyCollection<Instruction> {
		public readonly ReadOnlyCollection<string> InstructionStrings;
		
		InstructionLine(IList<Instruction> list, IList<string> newInstructionStrings) : base(list) {
			InstructionStrings = new ReadOnlyCollection<string>(newInstructionStrings);
		}

		internal static InstructionLine BuildLineFromInstructions(IEnumerable<string> instructions, int lineIndex) {
			List<Instruction> list = new List<Instruction>();
			List<string> instructionStrings = new List<string>();
			int index = 0;
			bool commented = false;
			foreach (var instruction in instructions) {
				instructionStrings.Add(instruction);

				if (instruction.StartsWith(":")) {
					commented = true;
				}
				if (commented) {
					continue;
				}

				try {
					list.Add(Instruction.GetInstructionFromString(instruction, lineIndex));
					index++;
				}
				catch (IncompleteParseException ex) {
					throw new IncompleteParseException(ex, null, index);
				}
			}

			return new InstructionLine(list, instructionStrings);
		}
	}
}