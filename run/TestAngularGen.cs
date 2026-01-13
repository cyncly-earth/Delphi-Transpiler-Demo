using CodeGen;

class TestAngularGen
{
    static void Main()
    {
        var generator = new AngularGenerator();

        generator.Generate("run/input/test_personview.json");

        Console.WriteLine("Angular codegen test completed.");
    }
}
