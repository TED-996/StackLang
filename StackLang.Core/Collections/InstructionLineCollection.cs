using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using StackLang.Core.Exceptions;

namespace StackLang.Core.Collections {
	public class InstructionLineCollection : ReadOnlyCollection<InstructionLine> {
		InstructionLineCollection(IList<InstructionLine> list) : base(list) {
		}

		internal static InstructionLineCollection BuildCollectionFromLines(IEnumerable<string> lines) {
			List<InstructionLine> list = new List<InstructionLine>();
			int index = 0;
			foreach (string line in lines) {
				try {
					InstructionLine instructionLine = InstructionLine.BuildLineFromInstructions(GetTokensFromLine(line));
					if (instructionLine.Count == 0) {
						continue;
					}
					list.Add(instructionLine);
					index++;
				}
				catch (IncompleteParseException ex) {
					throw new IncompleteParseException(ex, index, null);
				}
			}
			return new InstructionLineCollection(list);
		}

		static IEnumerable<string> GetTokensFromLine(string line) {
			return line.Split(new[] {' ', '\t'}, StringSplitOptions.RemoveEmptyEntries); //TODO: Replace with yield
		}
	}
}