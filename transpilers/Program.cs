using System;
using System.IO;
using DelphiTranspilerDemo.Parsing;

namespace DelphiTranspilerDemo
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            var inputs = new[]
            {
                "run/input/classPerson.pas",
                "run/input/PersonController.pas",
                "run/input/PersonView.pas"
            };

            var ParserService = new DelphiParserService();
            foreach(var inputPath in inputs)
            {
                if (!File.Exists(inputPath))
                {
                    Console.WriteLine($"[WARN] Missing: {inputPath}");
                    continue;
                }

                Console.WriteLine($"Parsing: {inputPath}");

                var parseTree = ParserService.ParseFile(inputPath);

                // TEMPORARY: debug only
                Console.WriteLine(parseTree.ToStringTree());
            }
            Console.WriteLine("Done parsing all modules.");
        }
    }
}
