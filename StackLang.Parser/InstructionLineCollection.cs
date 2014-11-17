using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace StackLang.Parser {
	public class InstructionLineCollection : ReadOnlyCollection<InstructionLine> {
		InstructionLineCollection(IList<InstructionLine> list) : base(list) {
		}

		internal static InstructionLineCollection BuildCollectionFromLines(IEnumerable<string> lines) {
			return new InstructionLineCollection(lines.Select(
				line => InstructionLine.BuildLineFromInstructions(GetTokensFromLine(line))).ToList());
		}

		static IEnumerable<string> GetTokensFromLine(string line) {
			return line.Split(new[] {' ', '\t'}, StringSplitOptions.RemoveEmptyEntries); //TODO: Replace with yield
		}
	}
}