using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using StackLang.Core;

namespace StackLang.Parser {
	public class InstructionLine : ReadOnlyCollection<Instruction> {
		InstructionLine(IList<Instruction> list) : base(list) {
		}

		internal static InstructionLine BuildLineFromInstructions(IEnumerable<string> instructions) {
			return new InstructionLine(instructions.Select(Instruction.GetInstructionFromString).ToList());
		}
	}
}