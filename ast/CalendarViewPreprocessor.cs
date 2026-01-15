using System.Text.RegularExpressions;

namespace Transpiler.AST;

public static class CalendarViewPreprocessor
{
    /// <summary>
    /// Preprocesses CalendarView.pas to handle const array declarations that cause parse errors.
    /// Moves const declarations from interface to implementation section.
    /// </summary>
    public static string Preprocess(string source)
    {
        // Find the const section between interface and implementation
        var constPattern = @"(?<before>procedure\s+\w+\s*;)\s*const\s+(?<constDecls>.*?)(?<after>implementation)";
        var match = Regex.Match(source, constPattern, RegexOptions.Singleline);
        
        if (!match.Success)
        {
            return source; // No problematic const section found
        }

        var before = match.Groups["before"].Value;
        var constDecls = match.Groups["constDecls"].Value;
        var after = match.Groups["after"].Value;

        // Move const declarations to after implementation keyword
        var processed = source.Replace(match.Value, 
            $"{before}\n\n{after}\n\nconst\n{constDecls}");

        return processed;
    }
}
