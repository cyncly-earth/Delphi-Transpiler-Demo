using System;
using Transpiler.AST;

using Transpiler.AST;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("--- Starting Pod 1: Delphi Parser ---");
        
        var processor = new AstProcessor();
        processor.Run();

        Console.WriteLine("--- Pod 1 Complete. Check 'output' folder. ---");
    }
}
// class Program
// {
//     static void Main(string[] args)
//     {
//         Console.WriteLine("--- Starting Pod 1: Delphi Parser ---");

//         // 1. Parse Command Line Arguments (from Pipeline)
//         string inputDir = null;
//         string outputDir = null;

//         for (int i = 0; i < args.Length; i++)
//         {
//             if (args[i] == "--input" && i + 1 < args.Length) inputDir = args[i + 1];
//             if (args[i] == "--output" && i + 1 < args.Length) outputDir = args[i + 1];
//         }

//         var processor = new AstProcessor();

//         // 2. Execute
//         if (!string.IsNullOrEmpty(inputDir) && !string.IsNullOrEmpty(outputDir))
//         {
//             // PIPELINE MODE: Use the paths provided by the runner
//             processor.RunExplicit(inputDir, outputDir);
//         }
//         else
//         {
//             // STANDALONE MODE: Auto-detect folders (good for debugging)
//             processor.Run();
//         }

//         Console.WriteLine("--- Pod 1 Complete ---");
//     }
//}