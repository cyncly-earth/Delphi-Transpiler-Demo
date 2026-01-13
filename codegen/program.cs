using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Program
{
    static void Main()
    {
        // Create output directory
        if (!Directory.Exists("output"))
        {
            Directory.CreateDirectory("output");
        }

        var screen = new ScreenConfig
        {
            Screen = "Person",
            Fields = new List<string>
            {
                "FirstName",
                "LastName",
                "Email"
            },
            Action = new ActionConfig
            {
                Name = "savePerson",
                Calls = "save"
            }
        };

        var generator = new AngularGenerator();
        generator.Generate(screen);

        Console.WriteLine("Angular code generation test completed.");
    }
}

public class ScreenConfig
{
    public string Screen { get; set; }
    public List<string> Fields { get; set; }
    public ActionConfig Action { get; set; }
}

public class ActionConfig
{
    public string Name { get; set; }
    public string Calls { get; set; }}