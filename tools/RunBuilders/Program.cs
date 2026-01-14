using System;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Running AST builders (in-memory + JSON output)");

        try
        {
            var ci = CalendarItemAstBuilder.Build();
            Console.WriteLine($"CalendarItem: classes={ci.Classes.Count}, procs={ci.Procedures.Count}");
            Console.WriteLine("CalendarItem JSON: " + System.Text.Json.JsonSerializer.Serialize(ci));
            CalendarItemAstBuilder.Run();

            var cv = CalendarViewAstBuilder.Build();
            Console.WriteLine($"CalendarView: classes={cv.Classes.Count}, procs={cv.Procedures.Count}");
            Console.WriteLine("CalendarView JSON: " + System.Text.Json.JsonSerializer.Serialize(cv));
            CalendarViewAstBuilder.Run();

            var cc = CalendarControllerAstBuilder.Build();
            Console.WriteLine($"CalendarController: classes={cc.Classes.Count}, procs={cc.Procedures.Count}");
            Console.WriteLine("CalendarController JSON: " + System.Text.Json.JsonSerializer.Serialize(cc));
            CalendarControllerAstBuilder.Run();

            Console.WriteLine("Builders completed. JSON files in result/ast_output/");
        }
        catch (Exception ex)
        {
            Console.WriteLine("[ERROR] " + ex);
        }
    }
}
