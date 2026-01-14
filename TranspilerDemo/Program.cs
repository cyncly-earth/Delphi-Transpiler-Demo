using System;
using System.IO;

class Program
{
    static void Main()
    {
        var inputPath = "run/input/ui-ir-demo.json";
        var outputPath = "run/output/angular";

        Directory.CreateDirectory(outputPath);

        var uiIrJson = File.ReadAllText(inputPath);//change file.readalltext(input) to semantic.function()

        var generator = new AngularGenerator();
        generator.Generate(uiIrJson, outputPath);

        Console.WriteLine("Angular code generated successfully!");
    }
}

// var json = File.ReadAllText("ui-ir.json");

// var generator = new AngularGenerator();
// generator.Generate(json, "./output/angular");
// var uiIrJson = File.ReadAllText("run/input/ui-ir-demo.json");

// var angularGen = new AngularGenerator();
// angularGen.Generate(uiIrJson, "run/output/angular");
