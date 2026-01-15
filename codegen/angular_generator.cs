
// angular_generator.cs
// .NET 10 console app that can:
//  1) Parse a C# object graph -> Angular-friendly TypeScript models
//  2) Create a full Angular workspace with Angular CLI (npx), then scaffold:
//     - models.ts
//     - <feature>.service.ts (HttpClient)
//     - <feature>-list.component.ts/.html (standalone)
//     - router + provideHttpClient()
//     Ready to `ng serve`.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

// ------------------------- Program -------------------------
internal static class Program
{
    public static int Main(string[] args)
    {
        var (options, error, usage) = CliOptions.Parse(args);
        if (options is null)
        {
            Console.WriteLine(usage);
            Console.WriteLine();
            Console.WriteLine($"Error: {error}");
            return 1;
        }

        try
        {
            // 1) (Optional) Create Angular workspace with Angular CLI
            if (!string.IsNullOrWhiteSpace(options.CreateAngularWorkspace))
            {
                var creator = new AngularWorkspaceCreator(options);
                creator.CreateWorkspace();
            }

            // 2) Generate TS models
            var gen = new TsGenerator(options);
            var context = gen.Run();

            // 3) (Optional) Scaffold Angular artifacts + wire up app
            if (!string.IsNullOrWhiteSpace(options.CreateAngularWorkspace) || options.ScaffoldAngular)
            {
                var scaffolder = new AngularScaffolder(options, context);
                scaffolder.WriteAngularArtifacts();

                if (!string.IsNullOrWhiteSpace(options.CreateAngularWorkspace))
                {
                    scaffolder.PatchAppConfigForHttpClient(); // provideHttpClient()
                    scaffolder.PatchRoutesForComponent();     // route -> /<feature>

                    // npm/pnpm/yarn install
                    if (!options.SkipInstall)
                    {
                        Console.WriteLine("📦 Installing packages...");
                        var ok = AngularWorkspaceCreator.RunNodeCommand(
                            options.PkgManager, new[] { "install" }, options.CreateAngularWorkspace);
                        if (!ok)
                        {
                            Console.WriteLine("⚠️ install failed. Run it later yourself.");
                        }
                    }

                    // Helpful next-steps
                    Console.WriteLine();
                    Console.WriteLine("✅ Angular workspace is ready.");
                    Console.WriteLine($"   Workspace: {Path.GetFullPath(options.CreateAngularWorkspace)}");
                    Console.WriteLine("   Start the dev server:");
                    Console.WriteLine($"     cd {options.CreateAngularWorkspace}");
                    Console.WriteLine(options.PkgManager == "npm" ? "     npm start" :
                                      options.PkgManager == "yarn" ? "     yarn start" : "     pnpm start");
                    Console.WriteLine($"   Open: http://localhost:4200/{options.RoutePath}");
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine("✅ Angular service + component scaffolded.");
                    Console.WriteLine();
                    Console.WriteLine("Next steps:");
                    Console.WriteLine("  1) Ensure HttpClient is provided (Angular 17+): provideHttpClient() in app.config.ts");
                    Console.WriteLine($"  2) Add a route to '/{options.RoutePath}' -> {scaffolder.ComponentClassName}");
                }
            }

            Console.WriteLine($"✅ Generated models: {options.OutFile}");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("❌ Generation failed:");
            Console.Error.WriteLine(ex.ToString());
            return 2;
        }
    }
}

// ------------------------- CLI -------------------------
enum OptionalStyle { Question, UnionNull }

sealed class CliOptions
{
    // Models
    public string InputPath { get; init; } = "";
    public string OutFile { get; init; } = "models.ts";
    public bool CamelCase { get; init; } = true;
    public bool DateAsDate { get; init; } = true;
    public OptionalStyle OptionalStyle { get; init; } = OptionalStyle.Question;
    public bool IncludeInternal { get; init; } = false;

    // Scaffold into existing Angular app
    public bool ScaffoldAngular { get; init; } = false;
    public string NgAppDir { get; init; } = "";        // ./src/app
    public string Feature { get; init; } = "entities";
    public string RootType { get; init; } = "";
    public string ApiBase { get; init; } = "/api";
    public string RoutePath { get; init; } = "";

    // Create a new Angular workspace
    public string CreateAngularWorkspace { get; init; } = "";  // e.g., ./my-app
    public string PkgManager { get; init; } = "npm";            // npm|pnpm|yarn
    public bool SkipInstall { get; init; } = false;

    public static (CliOptions? opts, string? error, string usage) Parse(string[] args)
    {
        var usage =
@"Usage:

  # Models only
  dotnet run angular_generator.cs -- --input <file-or-folder> --out <outfile.ts>
                                  [--camel|--no-camel]
                                  [--date-as-date|--date-as-string]
                                  [--optional-style=question|union]
                                  [--include-internal]

  # Create FULL Angular app + models + service + component + route
  dotnet run angular_generator.cs -- \
    --input ./run/input/Models.cs \
    --create-angular-workspace ./my-angular-app \
    --out ./my-angular-app/src/app/models/models.ts \
    --feature orders --root-type Order --api-base /api \
    [--pkg-manager npm|pnpm|yarn] [--skip-install]

  # Scaffold INTO existing Angular app
  dotnet run angular_generator.cs -- \
    --input ./run/input/Models.cs \
    --out ./src/app/models/models.ts \
    --scaffold-angular --ng-app-dir ./src/app \
    --feature orders --root-type Order --api-base /api --route-path orders
";

        string input = "";
        string output = "models.ts";
        bool camel = true, dateAsDate = true;
        OptionalStyle opt = OptionalStyle.Question;
        bool includeInternal = false;

        bool scaffold = false;
        string ngAppDir = "";
        string feature = "entities";
        string rootType = "";
        string apiBase = "/api";
        string routePath = "";

        string createWs = "";
        string pkgMgr = "npm";
        bool skipInstall = false;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                // Models
                case "--input": input = args[++i]; break;
                case "--out": output = args[++i]; break;
                case "--camel": camel = true; break;
                case "--no-camel": camel = false; break;
                case "--date-as-date": dateAsDate = true; break;
                case "--date-as-string": dateAsDate = false; break;
                case "--optional-style=question": opt = OptionalStyle.Question; break;
                case "--optional-style=union": opt = OptionalStyle.UnionNull; break;
                case "--include-internal": includeInternal = true; break;

                // Existing app scaffolding
                case "--scaffold-angular": scaffold = true; break;
                case "--ng-app-dir": ngAppDir = args[++i]; break;
                case "--feature": feature = args[++i]; break;
                case "--root-type": rootType = args[++i]; break;
                case "--api-base": apiBase = args[++i]; break;
                case "--route-path": routePath = args[++i]; break;

                // New workspace
                case "--create-angular-workspace": createWs = args[++i]; break;
                case "--pkg-manager": pkgMgr = args[++i]; break;
                case "--skip-install": skipInstall = true; break;
            }
        }

        if (string.IsNullOrWhiteSpace(input))
            return (null, "Missing --input <file-or-folder>", usage);

        if (!string.IsNullOrWhiteSpace(createWs))
        {
            if (string.IsNullOrWhiteSpace(routePath)) routePath = feature;
            ngAppDir = Path.Combine(createWs, "src", "app");
            // When creating a new workspace, ScaffoldAngular is implied
            scaffold = true;
            // If user didn't specify models --out, they should. Usage shows recommended path.
        }
        else if (scaffold && string.IsNullOrWhiteSpace(ngAppDir))
        {
            return (null, "When using --scaffold-angular, provide --ng-app-dir (e.g., ./src/app)", usage);
        }

        return (new CliOptions
        {
            InputPath = input,
            OutFile = output,
            CamelCase = camel,
            DateAsDate = dateAsDate,
            OptionalStyle = opt,
            IncludeInternal = includeInternal,
            ScaffoldAngular = scaffold,
            NgAppDir = ngAppDir,
            Feature = feature,
            RootType = rootType,
            ApiBase = apiBase,
            RoutePath = string.IsNullOrWhiteSpace(routePath) ? feature : routePath,
            CreateAngularWorkspace = createWs,
            PkgManager = pkgMgr,
            SkipInstall = skipInstall
        }, null, usage);
    }
}

// ------------------------- Model generation -------------------------
sealed class GeneratorContext
{
    public CSharpCompilation Compilation { get; }
    public IReadOnlyList<INamedTypeSymbol> Declarations { get; }
    public string ModelsOutFile { get; }
    public GeneratorContext(CSharpCompilation c, IReadOnlyList<INamedTypeSymbol> d, string outFile)
    {
        Compilation = c; Declarations = d; ModelsOutFile = outFile;
    }
}

sealed class TsGenerator
{
    private readonly CliOptions _o;
    public TsGenerator(CliOptions o) => _o = o;

    public GeneratorContext Run()
    {
        var files = new List<string>();
        if (File.Exists(_o.InputPath)) files.Add(_o.InputPath);
        else if (Directory.Exists(_o.InputPath))
            files.AddRange(Directory.EnumerateFiles(_o.InputPath, "*.cs", SearchOption.AllDirectories));
        else
            throw new FileNotFoundException($"Input not found: {_o.InputPath}");

        if (files.Count == 0) throw new InvalidOperationException("No .cs files found in input path.");

        var trees = files.Select(f => CSharpSyntaxTree.ParseText(File.ReadAllText(f), path: f)).ToList();

        var refs = GetTrustedPlatformReferences();
        var compilation = CSharpCompilation.Create(
            "InputGraph", trees, refs, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var modelByTree = trees.ToDictionary(t => t, t => compilation.GetSemanticModel(t));

        var decls = new List<INamedTypeSymbol>();
        foreach (var tree in trees)
        {
            var model = modelByTree[tree];
            var root = tree.GetRoot();
            foreach (var node in root.DescendantNodes().OfType<BaseTypeDeclarationSyntax>())
            {
                var sym = model.GetDeclaredSymbol(node) as INamedTypeSymbol;
                if (sym == null) continue;
                if (!_o.IncludeInternal && sym.DeclaredAccessibility != Accessibility.Public) continue;
                if (sym.Name.StartsWith("<", StringComparison.Ordinal)) continue; // synthesized
                decls.Add(sym);
            }
        }

        var sb = new StringBuilder();
        sb.AppendLine("// Auto-generated: C# object graph → Angular TypeScript models");
        sb.AppendLine();
        // Helpful minimal shims for C# patterns that don't map 1:1 to TS
        sb.AppendLine("// Compatibility shims");
        sb.AppendLine("export interface IEquatable<T> {}\nexport interface ValueType {}\n");

        foreach (var e in decls.Where(d => d.TypeKind == TypeKind.Enum).OrderBy(d => d.Name))
            AppendEnum(sb, e);

        foreach (var t in decls.Where(d => d.TypeKind != TypeKind.Enum).OrderBy(d => d.Name))
            AppendType(sb, t, compilation);

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(_o.OutFile)) ?? ".");
        File.WriteAllText(_o.OutFile, sb.ToString());

        return new GeneratorContext(compilation, decls, Path.GetFullPath(_o.OutFile));
    }

    private static IEnumerable<MetadataReference> GetTrustedPlatformReferences()
    {
        var tpa = (AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string) ?? string.Empty;
        return tpa.Split(Path.PathSeparator)
              .Where(p => p.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
              .Select(p => MetadataReference.CreateFromFile(p));
    }

    private void AppendEnum(StringBuilder sb, INamedTypeSymbol enumSym)
    {
        sb.AppendLine($"export enum {GetTypeName(enumSym)} {{");
        foreach (var field in enumSym.GetMembers().OfType<IFieldSymbol>())
        {
            if (field.Name == "value__") continue;
            string line = $"  {field.Name}";
            if (field.HasConstantValue && field.ConstantValue is IConvertible)
            {
                var v = Convert.ToString(field.ConstantValue, System.Globalization.CultureInfo.InvariantCulture);
                line += $" = {v}";
            }
            sb.AppendLine(line + ",");
        }
        sb.AppendLine("}");
        sb.AppendLine();
    }

    private void AppendType(StringBuilder sb, INamedTypeSymbol typeSym, Compilation compilation)
    {
        var bases = new List<string>();
        if (typeSym.BaseType is { SpecialType: not SpecialType.System_Object })
            bases.Add(GetTypeRef(typeSym.BaseType, compilation));
        bases.AddRange(typeSym.Interfaces.Select(i => GetTypeRef(i, compilation)));

        var name = GetTypeName(typeSym);
        var typeParams = typeSym.TypeParameters.Length > 0
            ? "<" + string.Join(", ", typeSym.TypeParameters.Select(tp => tp.Name)) + ">"
            : string.Empty;

        var extendsClause = bases.Count > 0 ? $" extends {string.Join(", ", bases.Distinct())}" : "";
        sb.AppendLine($"export interface {name}{typeParams}{extendsClause} {{");

        foreach (var member in typeSym.GetMembers().OfType<IPropertySymbol>())
        {
            if (member.IsIndexer) continue;
            if (member.DeclaredAccessibility != Accessibility.Public) continue;
            if (ShouldIgnore(member)) continue;

            var tsName = ResolvePropertyName(member);
            var tsType = GetTsType(member.Type, compilation);
            var optional = IsOptional(member);

            var optSuffix = optional && _o.OptionalStyle == OptionalStyle.Question ? "?" : "";
            if (optional && _o.OptionalStyle == OptionalStyle.UnionNull) tsType = $"{tsType} | null";

            sb.AppendLine($"  {tsName}{optSuffix}: {tsType};");
        }

        sb.AppendLine("}");
        sb.AppendLine();
    }

    private static bool ShouldIgnore(IPropertySymbol prop) =>
        prop.GetAttributes().Any(a =>
            a.AttributeClass?.ToDisplayString() switch
            {
                "System.Text.Json.Serialization.JsonIgnoreAttribute" => true,
                "Newtonsoft.Json.JsonIgnoreAttribute" => true,
                "System.Runtime.Serialization.IgnoreDataMemberAttribute" => true,
                _ => false
            });

    private string ResolvePropertyName(IPropertySymbol prop)
    {
        string name = prop.Name;

        foreach (var a in prop.GetAttributes())
        {
            var full = a.AttributeClass?.ToDisplayString();
            if (full == "System.Text.Json.Serialization.JsonPropertyNameAttribute" &&
                a.ConstructorArguments.Length == 1)
            {
                name = a.ConstructorArguments[0].Value?.ToString() ?? name;
            }
            else if (full == "Newtonsoft.Json.JsonPropertyAttribute")
            {
                if (a.ConstructorArguments.Length > 0 &&
                    a.ConstructorArguments[0].Value is string s1 && !string.IsNullOrWhiteSpace(s1))
                    name = s1;
                else
                {
                    var named = a.NamedArguments.FirstOrDefault(kv => kv.Key == "PropertyName");
                    if (named.Value.Value is string s2 && !string.IsNullOrWhiteSpace(s2))
                        name = s2;
                }
            }
            else if (full == "System.Runtime.Serialization.DataMemberAttribute")
            {
                var named = a.NamedArguments.FirstOrDefault(kv => kv.Key == "Name");
                if (named.Value.Value is string s3 && !string.IsNullOrWhiteSpace(s3))
                    name = s3;
            }
        }

        return _o.CamelCase ? ToCamelCase(name) : name;
    }

    private bool IsOptional(IPropertySymbol prop)
    {
        var t = prop.Type;
        bool isNullableRef = t.IsReferenceType && prop.NullableAnnotation == NullableAnnotation.Annotated;
        bool isNullableValue = t.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;
        bool isRequired = prop.IsRequired;
        if (isNullableRef || isNullableValue) return true;
        if (isRequired) return false;
        return false;
    }

    private string GetTsType(ITypeSymbol type, Compilation compilation)
    {
        // Handle generic type parameters (e.g., T) safely
        if (type is ITypeParameterSymbol tparam)
        {
            return string.IsNullOrEmpty(tparam.Name) ? "any" : tparam.Name;
        }

        if (type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T &&
            type is INamedTypeSymbol nt && nt.TypeArguments.Length == 1)
            return GetTsType(nt.TypeArguments[0], compilation);

        if (type is IArrayTypeSymbol ats)
            return $"{GetTsType(ats.ElementType, compilation)}[]";

        if (type is INamedTypeSymbol namedTuple && namedTuple.IsTupleType)
        {
            var elems = namedTuple.TupleElements;
            return "[" + string.Join(", ", elems.Select(e => GetTsType(e.Type, compilation))) + "]";
        }

        if (TryGetSingleTypeArgument(type, out var elem, KnownCollectionType))
            return $"{GetTsType(elem!, compilation)}[]";

        if (TryGetDictionary(type, out var key, out var value))
            return $"{{ [key: {GetDictionaryKeyTs(key!)}]: {GetTsType(value!, compilation)} }}";

        if (IsKeyValuePair(type, out var k, out var v))
            return $"{{ key: {GetTsType(k!, compilation)}; value: {GetTsType(v!, compilation)} }}";

        if (GetBuiltin(type, out var mapped)) return mapped!;

        if (type is INamedTypeSymbol named && named.IsGenericType)
        {
            var nm = GetTypeName(named);
            var args = string.Join(", ", named.TypeArguments.Select(a => GetTsType(a, compilation)));
            return $"{nm}<{args}>";
        }

        if (type is INamedTypeSymbol namedType)
            return GetTypeName(namedType);

        return type.Name ?? "any";
    }

    private static bool KnownCollectionType(INamedTypeSymbol named)
    {
        var def = named.OriginalDefinition.ToDisplayString();
        return def is
            "System.Collections.Generic.IEnumerable<T>" or
            "System.Collections.Generic.ICollection<T>" or
            "System.Collections.Generic.IReadOnlyCollection<T>" or
            "System.Collections.Generic.IReadOnlyList<T>" or
            "System.Collections.Generic.IList<T>" or
            "System.Collections.Generic.List<T>" or
            "System.Collections.Generic.HashSet<T>";
    }

    private static bool TryGetSingleTypeArgument(ITypeSymbol type, out ITypeSymbol? element, Func<INamedTypeSymbol, bool> predicate)
    {
        element = null;
        if (type is INamedTypeSymbol named)
        {
            if (named.IsGenericType && named.TypeArguments.Length == 1 && predicate(named))
            {
                element = named.TypeArguments[0];
                return true;
            }
            foreach (var i in named.AllInterfaces)
            {
                if (i.IsGenericType && i.TypeArguments.Length == 1 && predicate(i))
                {
                    element = i.TypeArguments[0];
                    return true;
                }
            }
        }
        return false;
    }

    private static bool TryGetDictionary(ITypeSymbol type, out ITypeSymbol? key, out ITypeSymbol? value)
    {
        key = value = null;
        if (type is INamedTypeSymbol named)
        {
            bool isDict(INamedTypeSymbol n) =>
                n.OriginalDefinition.ToDisplayString() is
                    "System.Collections.Generic.Dictionary<TKey, TValue>" or
                    "System.Collections.Generic.IDictionary<TKey, TValue>" or
                    "System.Collections.Generic.IReadOnlyDictionary<TKey, TValue>";

            if (named.IsGenericType && named.TypeArguments.Length == 2 && isDict(named))
            {
                key = named.TypeArguments[0];
                value = named.TypeArguments[1];
                return true;
            }
            foreach (var i in named.AllInterfaces)
            {
                if (i.IsGenericType && i.TypeArguments.Length == 2 && isDict(i))
                {
                    key = i.TypeArguments[0];
                    value = i.TypeArguments[1];
                    return true;
                }
            }
        }
        return false;
    }

    private static bool IsKeyValuePair(ITypeSymbol type, out ITypeSymbol? key, out ITypeSymbol? value)
    {
        key = value = null;
        if (type is INamedTypeSymbol n &&
            n.OriginalDefinition.ToDisplayString() == "System.Collections.Generic.KeyValuePair<TKey, TValue>" &&
            n.IsGenericType && n.TypeArguments.Length == 2)
        {
            key = n.TypeArguments[0];
            value = n.TypeArguments[1];
            return true;
        }
        return false;
    }

    private static string GetDictionaryKeyTs(ITypeSymbol key) =>
        key.SpecialType switch
        {
            SpecialType.System_String => "string",
            SpecialType.System_Int32 or SpecialType.System_Int64 or
            SpecialType.System_UInt32 or SpecialType.System_UInt64 => "number",
            _ => "string"
        };

    private bool GetBuiltin(ITypeSymbol type, out string? ts)
    {
        ts = null;

        if (type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T &&
            type is INamedTypeSymbol nt && nt.TypeArguments.Length == 1)
            type = nt.TypeArguments[0];

        switch (type.SpecialType)
        {
            case SpecialType.System_Boolean: ts = "boolean"; return true;
            case SpecialType.System_String: ts = "string"; return true;
            case SpecialType.System_Char: ts = "string"; return true;

            case SpecialType.System_SByte:
            case SpecialType.System_Byte:
            case SpecialType.System_Int16:
            case SpecialType.System_UInt16:
            case SpecialType.System_Int32:
            case SpecialType.System_UInt32:
            case SpecialType.System_Int64:
            case SpecialType.System_UInt64:
            case SpecialType.System_Single:
            case SpecialType.System_Double:
            case SpecialType.System_Decimal:
                ts = "number"; return true;
        }

        var full = type.ToDisplayString();

        if (full is "System.DateTime" or "System.DateOnly")
        {
            ts = _o.DateAsDate ? "Date" : "string";
            return true;
        }
        if (full is "System.DateTimeOffset") { ts = "string"; return true; }
        if (full is "System.TimeSpan" or "System.TimeOnly") { ts = "string"; return true; }
        if (full is "System.Guid") { ts = "string"; return true; }
        if (full is "System.Byte[]") { ts = "string"; return true; }

        if (type.TypeKind == TypeKind.Enum) { ts = GetTypeName((INamedTypeSymbol)type); return true; }

        return false;
    }

    private static string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        if (name.Length == 1) return name.ToLowerInvariant();
        if (char.IsUpper(name[0]) && name.Length > 1 && char.IsUpper(name[1]))
            return char.ToLowerInvariant(name[0]) + name[1..];
        return char.ToLowerInvariant(name[0]) + name[1..];
    }

    private static string GetTypeName(INamedTypeSymbol t)
    {
        var n = t.Name;
        var i = n.IndexOf('`');
        return i >= 0 ? n[..i] : n;
    }

    private static string GetTypeRef(ITypeSymbol type, Compilation compilation)
    {
        if (type is INamedTypeSymbol n && n.IsGenericType)
        {
            var name = GetTypeName(n);
            var args = string.Join(", ", n.TypeArguments.Select(a => a.Name));
            return $"{name}<{args}>";
        }
        return type.Name;
    }
}

// ------------------------- Angular CLI workspace creation -------------------------
sealed class AngularWorkspaceCreator
{
    private readonly CliOptions _o;
    public AngularWorkspaceCreator(CliOptions o) => _o = o;

    public void CreateWorkspace()
    {
        var wsDir = _o.CreateAngularWorkspace;
        var full = Path.GetFullPath(wsDir);
        if (Directory.Exists(full) && Directory.EnumerateFileSystemEntries(full).Any())
            throw new InvalidOperationException($"Target workspace directory is not empty: {full}");

        Directory.CreateDirectory(full);
        var appName = Path.GetFileName(full.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        var parent = Directory.GetParent(full)!.FullName;

        Console.WriteLine("🧱 Creating Angular workspace (Angular CLI)...");
        // ng new <appName> --routing --style=scss --standalone --skip-install --skip-git
        var args = new List<string> {
            "-y", "@angular/cli@latest", "new", appName,
            "--routing", "--style", "scss", "--standalone", "--skip-install", "--skip-git"
        };

        var ok = RunNodeCommand("npx", args.ToArray(), parent);
        if (!ok)
            throw new InvalidOperationException("Angular CLI 'ng new' failed. Ensure Node/npm are installed.");

        // Add a "start": "ng serve" script if missing
        EnsureStartScript(full);
    }

    public static bool RunNodeCommand(string file, string[] args, string workingDirectory)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = file,
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            foreach (var a in args) psi.ArgumentList.Add(a);

            using var p = Process.Start(psi)!;
            p.OutputDataReceived += (_, e) => { if (e.Data != null) Console.WriteLine(e.Data); };
            p.ErrorDataReceived += (_, e) => { if (e.Data != null) Console.Error.WriteLine(e.Data); };
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();
            p.WaitForExit();
            return p.ExitCode == 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return false;
        }
    }

    private static void EnsureStartScript(string workspaceDir)
    {
        var pkgJson = Path.Combine(workspaceDir, "package.json");
        if (!File.Exists(pkgJson)) return;

        var text = File.ReadAllText(pkgJson);
        if (!text.Contains("\"start\"", StringComparison.Ordinal))
        {
            text = text.Replace("\"scripts\": {", "\"scripts\": {\n    \"start\": \"ng serve\",");
            File.WriteAllText(pkgJson, text);
        }
    }
}

// ------------------------- Angular scaffolding (service+component+patching) -------------------------
sealed class AngularScaffolder
{
    private readonly CliOptions _o;
    private readonly GeneratorContext _ctx;

    public AngularScaffolder(CliOptions o, GeneratorContext ctx) { _o = o; _ctx = ctx; }

    public string ComponentClassName => $"{ToPascal(_o.Feature)}ListComponent";
    public string ServiceClassName => $"{ToPascal(_o.Feature)}Service";
    public string ComponentDir => Path.Combine(_o.NgAppDir, "components", $"{_o.Feature}-list");
    public string ServiceDir => Path.Combine(_o.NgAppDir, "services");
    public string ComponentTsPath => Path.Combine(ComponentDir, $"{_o.Feature}-list.component.ts");
    public string ComponentHtmlPath => Path.Combine(ComponentDir, $"{_o.Feature}-list.component.html");
    public string ServiceTsPath => Path.Combine(ServiceDir, $"{_o.Feature}.service.ts");

    public void WriteAngularArtifacts()
    {
        Directory.CreateDirectory(ComponentDir);
        Directory.CreateDirectory(ServiceDir);

        var rootType = ResolveRootType();
        if (rootType == null) throw new InvalidOperationException("Provide --root-type <TypeName>.");

        var columns = PickScalarPropertyNames(rootType, max: 5);
        var modelName = TsName(rootType.Name);

        var modelsRelFromService = RelativeImportToModels(ServiceDir);
        var modelsRelFromComponent = RelativeImportToModels(ComponentDir);

        File.WriteAllText(ServiceTsPath, ServiceTemplate(ServiceClassName, modelName, _o.ApiBase, _o.Feature, modelsRelFromService));
        File.WriteAllText(ComponentTsPath, ComponentTemplate(ComponentClassName, ServiceClassName, modelName, modelsRelFromComponent, columns));
        File.WriteAllText(ComponentHtmlPath, ComponentHtml(columns));
    }

    public void PatchAppConfigForHttpClient()
    {
        var configFile = Path.Combine(_o.CreateAngularWorkspace, "src", "app", "app.config.ts");
        if (!File.Exists(configFile))
        {
            var routesImport = "import { provideRouter } from '@angular/router';\nimport { routes } from './app.routes';";
            var httpImport = "import { provideHttpClient } from '@angular/common/http';";
            var content = $"{routesImport}\n{httpImport}\n\n" +
                          "export const appConfig = {\n  providers: [provideRouter(routes), provideHttpClient()]\n};\n";
            File.WriteAllText(configFile, content);
            return;
        }

        var text = File.ReadAllText(configFile);
        if (!text.Contains("provideHttpClient", StringComparison.Ordinal))
        {
            text = "import { provideHttpClient } from '@angular/common/http';\n" + text;
            text = text.Replace("providers: [", "providers: [ provideHttpClient(), ");
            File.WriteAllText(configFile, text);
        }
    }

    public void PatchRoutesForComponent()
    {
        var routesFile = Path.Combine(_o.CreateAngularWorkspace, "src", "app", "app.routes.ts");
        var route = $"{{ path: '{_o.RoutePath}', loadComponent: () => import('{RelativeImportForComponent()}').then(m => m.{ComponentClassName}) }}";

        if (!File.Exists(routesFile))
        {
            var content = "import { Routes } from '@angular/router';\n\n" +
                          $"export const routes: Routes = [\n  {route},\n  {{ path: '', redirectTo: '{_o.RoutePath}', pathMatch: 'full' }}\n];\n";
            File.WriteAllText(routesFile, content);
            return;
        }

        var existing = File.ReadAllText(routesFile);
        if (!existing.Contains(ComponentClassName, StringComparison.Ordinal))
        {
            var content = "import { Routes } from '@angular/router';\n\n" +
                          $"export const routes: Routes = [\n  {route},\n  {{ path: '', redirectTo: '{_o.RoutePath}', pathMatch: 'full' }}\n];\n";
            File.WriteAllText(routesFile, content);
        }
    }

    private INamedTypeSymbol? ResolveRootType()
    {
        if (!string.IsNullOrWhiteSpace(_o.RootType))
        {
            var rt = _ctx.Declarations.FirstOrDefault(t => t.Name == _o.RootType && t.TypeKind != TypeKind.Enum);
            if (rt != null) return rt;
        }
        return _ctx.Declarations.FirstOrDefault(t => t.TypeKind != TypeKind.Enum);
    }

    private static string TsName(string csharpName) =>
        csharpName.Contains('`') ? csharpName.Substring(0, csharpName.IndexOf('`')) : csharpName;

    private string RelativeImportToModels(string fromDir)
    {
        var noExt = Path.Combine(Path.GetDirectoryName(_ctx.ModelsOutFile) ?? ".", Path.GetFileNameWithoutExtension(_ctx.ModelsOutFile));
        var rel = Path.GetRelativePath(fromDir, noExt).Replace('\\', '/');
        if (!rel.StartsWith(".", StringComparison.Ordinal)) rel = "./" + rel;
        return rel;
    }

    private string RelativeImportForComponent()
    {
        var compNoExt = Path.Combine(Path.GetDirectoryName(ComponentTsPath) ?? ".", Path.GetFileNameWithoutExtension(ComponentTsPath))
                        .Replace('\\', '/');
        var rel = Path.GetRelativePath(Path.Combine(_o.CreateAngularWorkspace, "src", "app"), compNoExt).Replace('\\', '/');
        if (!rel.StartsWith(".", StringComparison.Ordinal)) rel = "./" + rel;
        return rel;
    }

    private static IReadOnlyList<string> PickScalarPropertyNames(INamedTypeSymbol root, int max)
    {
        bool IsScalar(ITypeSymbol t)
        {
            if (t.TypeKind == TypeKind.Enum) return true;
            if (t.SpecialType == SpecialType.System_String ||
                t.SpecialType == SpecialType.System_Boolean ||
                t.SpecialType == SpecialType.System_Char ||
                t.SpecialType == SpecialType.System_Int16 || t.SpecialType == SpecialType.System_UInt16 ||
                t.SpecialType == SpecialType.System_Int32 || t.SpecialType == SpecialType.System_UInt32 ||
                t.SpecialType == SpecialType.System_Int64 || t.SpecialType == SpecialType.System_UInt64 ||
                t.SpecialType == SpecialType.System_Single || t.SpecialType == SpecialType.System_Double ||
                t.SpecialType == SpecialType.System_Decimal)
                return true;

            var full = t.ToDisplayString();
            if (full is "System.DateTime" or "System.DateOnly" or "System.DateTimeOffset" or "System.Guid")
                return true;

            return false;
        }

        var props = root.GetMembers().OfType<IPropertySymbol>()
            .Where(p => p.DeclaredAccessibility == Accessibility.Public && !p.IsIndexer && IsScalar(p.Type))
            .Select(p => p.Name).Take(max).ToList();

        if (!props.Contains("Id", StringComparer.Ordinal) && root.GetMembers("Id").OfType<IPropertySymbol>().Any())
            props.Insert(0, "Id");

        return props;
    }

        private static string ServiceTemplate(string className, string modelName, string apiBase, string feature, string importFromModels) => $@"import {{ Injectable, inject }} from '@angular/core';
import {{ HttpClient }} from '@angular/common/http';
import {{ Observable }} from 'rxjs';
import {{ {modelName} }} from '{importFromModels}';

@Injectable({{ providedIn: 'root' }})
export class {className} {{
    private http = inject(HttpClient);
    private readonly base = '{apiBase}';

    getAll(): Observable<{modelName}[]> {{
        return this.http.get<{modelName}[]>(`${{this.base}}/{feature}`);
    }}

    getById(id: string): Observable<{modelName}> {{
        return this.http.get<{modelName}>(`${{this.base}}/{feature}/${{id}}`);
    }}

    create(entity: {modelName}): Observable<{modelName}> {{
        return this.http.post<{modelName}>(`${{this.base}}/{feature}`, entity);
    }}
}}";

    private static string ComponentTemplate(string className, string serviceClass, string modelName, string importFromModels, IReadOnlyList<string> columns)
    {
        var colsTs = string.Join(", ", columns.Select(c => $"'{ToCamel(c)}'"));
        var serviceKebab = ToKebab(serviceClass.Replace("Service", ""));
        var classKebab = ToKebab(className.Replace("Component", ""));
                return $@"import {{ Component, inject }} from '@angular/core';
import {{ CommonModule }} from '@angular/common';
import {{ {serviceClass} }} from '../../services/{serviceKebab}.service';
import {{ {modelName} }} from '{importFromModels}';

@Component({{
    selector: 'app-{classKebab}',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './{classKebab}.component.html',
}})
export class {className} {{
    private service = inject({serviceClass});
    items: {modelName}[] = [];
    loading = true;
    error: string | null = null;

    readonly columns: string[] = [{colsTs}];

    ngOnInit(): void {{
        this.service.getAll().subscribe({{
            next: data => {{
                this.items = data ?? [];
                this.loading = false;
            }},
            error: err => {{
                this.error = err?.message ?? 'Failed to load data';
                this.loading = false;
            }}
        }});
    }}
}}";
    }

    private static string ComponentHtml(IReadOnlyList<string> columns)
    {
        if (columns.Count == 0)
        {
            return @"<section class=""feature"">
  <h2>List</h2>
  <div *ngIf=""loading"">Loading…</div>
  <div *ngIf=""error"" class=""error"">{{ error }}</div>
  <pre *ngIf=""!loading && !error"">{{ items | json }}</pre>
</section>";
        }

        var th = string.Join(Environment.NewLine, columns.Select(c => $"        <th>{ToTitle(c)}</th>"));
        var td = string.Join(Environment.NewLine, columns.Select(c => $"        <td>{{{{ item.{ToCamel(c)} }}}}</td>"));

        return $@"<section class=""feature"">
  <h2>List</h2>

  <div *ngIf=""loading"" class=""loading"">Loading…</div>
  <div *ngIf=""error"" class=""error"">{{{{ error }}}}</div>

  <table *ngIf=""!loading && !error && items?.length"">
    <thead>
      <tr>
{th}
      </tr>
    </thead>
    <tbody>
      <tr *ngFor=""let item of items"">
{td}
      </tr>
    </tbody>
  </table>

  <p *ngIf=""!loading && !error && (!items || items.length === 0)"">
    No data found.
  </p>
</section>";
    }

    private static string ToKebab(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return s;
        var sb = new StringBuilder(s.Length * 2);
        for (int i = 0; i < s.Length; i++)
        {
            var ch = s[i];
            if (char.IsUpper(ch))
            {
                if (i > 0) sb.Append('-');
                sb.Append(char.ToLowerInvariant(ch));
            }
            else sb.Append(ch);
        }
        return sb.ToString();
    }

    private static string ToCamel(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        return char.ToLowerInvariant(name[0]) + (name.Length > 1 ? name.Substring(1) : "");
    }

    private static string ToTitle(string name)
    {
        var camel = ToCamel(name);
        var spaced = new StringBuilder();
        foreach (var ch in camel)
        {
            if (char.IsUpper(ch)) { spaced.Append(' '); spaced.Append(ch); }
            else spaced.Append(ch);
        }
        var s = spaced.ToString().Trim();
        return string.IsNullOrEmpty(s) ? "" : char.ToUpperInvariant(s[0]) + (s.Length > 1 ? s.Substring(1) : "");
    }

    private static string ToPascal(string s) =>
        string.IsNullOrWhiteSpace(s) ? s : char.ToUpperInvariant(s[0]) + (s.Length > 1 ? s.Substring(1) : "");
}