using System;
using DelphiTranspiler.Semantics;

namespace ClientManagementTranspiler
{
    class Program
    {
        static void Main(string[] args)
        {
            // Determine which IR to log (default: "all")
            string irType = args.Length > 0 ? args[0] : "all";

            // Build semantic IR for the client management feature
            var builder = new SemanticIrBuilder();
            var (uiJson, entityJson, backendJson) = builder.BuildSemanticIr(irType);

            // Log the requested IR(s)
            irType = irType.ToLowerInvariant();

            if (irType == "all" || irType == "ui")
            {
                Console.WriteLine("===== UI IR =====");
                Console.WriteLine(uiJson);
                Console.WriteLine();
            }

            if (irType == "all" || irType == "entity")
            {
                Console.WriteLine("===== ENTITY MODEL IR =====");
                Console.WriteLine(entityJson);
                Console.WriteLine();
            }

            if (irType == "all" || irType == "backend")
            {
                Console.WriteLine("===== BACKEND IR =====");
                Console.WriteLine(backendJson);
                Console.WriteLine();
            }
        }
    }
}
