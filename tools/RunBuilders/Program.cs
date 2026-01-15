using System;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Running AST builders (in-memory + JSON output)");

        try
        {
            Console.WriteLine("\n=== Building CalendarItem ===");
            var ci = CalendarItemAstBuilder.Build();
            Console.WriteLine($"CalendarItem: Interface procs={ci.InterfaceSection.Procedures.Count}, Implementation procs={ci.ImplementationSection.Procedures.Count}");
            CalendarItemAstBuilder.Run();
            Console.WriteLine("✓ CalendarItem.ast generated");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] CalendarItem failed: {ex.Message}");
        }

        try
        {
            Console.WriteLine("\n=== Building CalendarController ===");
            var cc = CalendarControllerAstBuilder.Build();
            Console.WriteLine($"Interface:");
            Console.WriteLine($"  Uses: {string.Join(", ", cc.InterfaceSection.Uses)}");
            Console.WriteLine($"  Procedures: {cc.InterfaceSection.Procedures.Count}");
            foreach (var proc in cc.InterfaceSection.Procedures)
            {
                Console.WriteLine($"    - {proc.ProcedureType} {proc.Name}({proc.Parameters.Count} params)");
            }
            Console.WriteLine($"Implementation:");
            Console.WriteLine($"  Uses: {string.Join(", ", cc.ImplementationSection.Uses)}");
            Console.WriteLine($"  Procedures: {cc.ImplementationSection.Procedures.Count}");
            foreach (var proc in cc.ImplementationSection.Procedures)
            {
                Console.WriteLine($"    - {proc.ProcedureType} {proc.Name}({proc.Parameters.Count} params, {proc.LocalVariables.Count} local vars, {proc.Body.Count} statements)");
            }
            CalendarControllerAstBuilder.Run();
            Console.WriteLine("✓ CalendarController.ast generated");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] CalendarController failed: {ex.Message}");
        }

        try
        {
            Console.WriteLine("\n=== Building CalendarView ===");
            var cv = CalendarViewAstBuilder.Build();
            Console.WriteLine($"CalendarView: Interface procs={cv.InterfaceSection.Procedures.Count}, Implementation procs={cv.ImplementationSection.Procedures.Count}");
            CalendarViewAstBuilder.Run();
            Console.WriteLine("✓ CalendarView.ast generated");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] CalendarView failed: {ex.Message}");
            Console.WriteLine("Note: CalendarView has complex const declarations that may not parse correctly with current grammar.");
        }

        Console.WriteLine("\n=== Builders completed ===");
    }
}
