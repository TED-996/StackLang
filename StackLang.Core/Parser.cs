using System.IO;
using StackLang.Core.Collections;
using StackLang.Core.Exceptions;

namespace StackLang.Core {
    public class Parser {
	    readonly Stream stream;

	    public Parser(Stream newStream) {
		    stream = newStream;
	    }

	    public InstructionLineCollection Parse() {
		    try {
			    return InstructionLineCollection.BuildCollectionFromLines(stream.ReadAllLines());
		    }
		    catch (IncompleteParseException ex) {
			    throw ex.GetParseException();
		    }
	    }
    }
}
