using System.IO;
using System.Linq;

namespace StackLang.Parser
{
    public class Parser {
	    readonly Stream stream;

	    public Parser(Stream newStream) {
		    stream = newStream;
	    }

	    public InstructionLineCollection Parse() {
		    return InstructionLineCollection.BuildCollectionFromLines(stream.ReadAllLines().ToArray());
	    } 
    }
}
