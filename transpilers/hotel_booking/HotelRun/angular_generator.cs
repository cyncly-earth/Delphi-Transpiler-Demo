using System.Text;

public static class AngularGenerator
{
    public static void Generate(ScreenIR screen)
    {
        GenerateComponentTs(screen);
        GenerateHtml(screen);
    }

    static void GenerateComponentTs(ScreenIR screen)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"export class {screen.ScreenName}Component {{");

        foreach (var field in screen.Fields)
            sb.AppendLine($"  {field.Name} = '';");

        foreach (var action in screen.Actions)
        {
            sb.AppendLine($"\n  {action.Name}() {{");
            sb.AppendLine($"    // call {action.Calls}");
            sb.AppendLine("  }");
        }

        sb.AppendLine("}");

        File.WriteAllText(
            $"{screen.ScreenName.ToLower()}.component.ts",
            sb.ToString()
        );
    }

    static void GenerateHtml(ScreenIR screen)
    {
        var sb = new StringBuilder();

        foreach (var field in screen.Fields)
            sb.AppendLine($"<input [(ngModel)]=\"{field.Name}\" />");

        foreach (var action in screen.Actions)
            sb.AppendLine($"<button (click)=\"{action.Name}()\">Save</button>");

        File.WriteAllText(
            $"{screen.ScreenName.ToLower()}.component.html",
            sb.ToString()
        );
    }
}
