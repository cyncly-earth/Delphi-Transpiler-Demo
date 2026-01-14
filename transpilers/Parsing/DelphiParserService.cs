using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System.IO;

namespace DelphiTranspilerDemo.Parsing
{
    public class DelphiParserService
    {
        public IParseTree ParseFile(string filePath)
        {
            var inputStream = CharStreams.fromPath(filePath);
            var lexer = new DelphiLexer(inputStream);
            var tokens = new CommonTokenStream(lexer);
            var parser = new DelphiParser(tokens);

            parser.BuildParseTree = true;

            return parser.file(); // entry rule
        }
    }
}