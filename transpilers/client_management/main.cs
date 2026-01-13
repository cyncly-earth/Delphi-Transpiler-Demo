using System;
using System.Linq;

namespace ClientManagementTranspiler
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine(
                    "Usage: ClientManagementTranspiler <ast_file1> <ast_file2> ...");
                return;
            }

            try
            {
                Console.WriteLine("Starting semantic enrichment for feature:");
                foreach (var file in args)
                {
                    Console.WriteLine($"  - {file}");
                }

                var runner = new SemanticEnrichmentRunner();

                // 🔑 IMPORTANT: process ALL files together
                runner.ProcessFeature(args.ToList());

                Console.WriteLine("Semantic enrichment completed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fatal error: {ex.Message}");
            }
        }
    }
}
