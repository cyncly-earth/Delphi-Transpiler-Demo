using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Transpiler.AST;

namespace Transpiler.Semantics
{
	// Booking domain semantic analyzer that maps AstUnit -> SemanticContext
	public class BookingSemanticAnalyzer : SemanticAnalyzerBase
	{
		protected override void BuildTypeRegistry(AstUnit unit, SemanticContext ctx)
		{
			// register simple types found in type declarations and classes
			foreach (var t in unit.InterfaceSection.Types.Concat(unit.ImplementationSection.Types))
			{
				if (!string.IsNullOrEmpty(t.Name) && !ctx.TypeRegistry.ContainsKey(t.Name))
				{
					ctx.TypeRegistry[t.Name] = new TypeMetadata { Name = t.TypeKind ?? t.Name };
				}
			}
		}

		protected override void BuildSymbols(AstUnit unit, SemanticContext ctx)
		{
			// register top-level symbols (classes, procedures)
			foreach (var c in unit.InterfaceSection.Classes.Concat(unit.ImplementationSection.Classes))
			{
				if (!string.IsNullOrEmpty(c.Name))
					ctx.SymbolTable[c.Name] = new SymbolInfo { Name = c.Name, Kind = "class" };
			}

			foreach (var p in unit.InterfaceSection.Procedures.Concat(unit.ImplementationSection.Procedures))
			{
				if (!string.IsNullOrEmpty(p.Name))
					ctx.SymbolTable[p.Name] = new SymbolInfo { Name = p.Name, Kind = "procedure" };
			}
		}

		protected override void Transform(AstUnit unit, SemanticContext ctx)
		{
			// Map classes -> Entities / Models
			foreach (var c in unit.InterfaceSection.Classes.Concat(unit.ImplementationSection.Classes))
			{
				var entity = new EntityDefinition { Name = c.Name };
				foreach (var f in c.Fields)
				{
					foreach (var n in f.Names)
					{
						entity.Fields.Add(new FieldDefinition { Name = n, TypeName = f.TypeName });
					}
				}
				ctx.Backend.Entities.Add(entity);
				ctx.Frontend.Models.Add(new ModelDefinition { Name = c.Name, Fields = entity.Fields.Select(x => new FieldDefinition { Name = x.Name, TypeName = x.TypeName }).ToList() });
			}

			// Map procedures -> Controllers / Services
			var combinedProcedures = unit.InterfaceSection.Procedures.Concat(unit.ImplementationSection.Procedures);
			if (combinedProcedures.Any())
			{
				var controller = new ControllerDefinition { Name = unit.Name + "Controller" };
				var service = new ServiceDefinition { Name = unit.Name + "Service" };

				foreach (var p in combinedProcedures)
				{
					var op = new OperationDefinition
					{
						Name = p.Name,
						ReturnType = string.IsNullOrEmpty(p.ReturnType) ? "void" : p.ReturnType,
						Parameters = p.Parameters.SelectMany(par => par.Names.Select(n => new ParameterDefinition { Name = n, TypeName = par.TypeName })).ToList()
					};

					controller.Operations.Add(op);
					service.Operations.Add(op);
					ctx.Frontend.Services.Add(new ServiceDefinition { Name = unit.Name + "ClientService", Operations = new List<OperationDefinition> { op } });
				}

				ctx.Backend.Controllers.Add(controller);
				ctx.Backend.Services.Add(service);
			}
		}
	}

	// Runner utility: load .ast files (AstSerializer) and produce aggregated SemanticContext
	public static class BookingSemanticRunner
	{
		private static readonly JsonSerializerOptions Options = new()
		{
			WriteIndented = true,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
		};

		public static SemanticContext AnalyzeDirectory(string astDirectory)
		{
			if (!Directory.Exists(astDirectory))
				throw new DirectoryNotFoundException(astDirectory);

			var analyzer = new BookingSemanticAnalyzer();
			var aggregated = new SemanticContext { Backend = new BackendModel(), Frontend = new FrontendModel(), TypeRegistry = new Dictionary<string, TypeMetadata>(), SymbolTable = new Dictionary<string, SymbolInfo>(), Diagnostics = new List<string>() };

			var files = Directory.GetFiles(astDirectory, "*.ast");
			foreach (var f in files)
			{
				try
				{
					var unit = AstSerializer.Load(f);
					var ctx = analyzer.Analyze(unit);

					// merge backend entities
					aggregated.Backend.Entities.AddRange(ctx.Backend.Entities);
					aggregated.Backend.Controllers.AddRange(ctx.Backend.Controllers);
					aggregated.Backend.Services.AddRange(ctx.Backend.Services);

					// merge frontend
					aggregated.Frontend.Models.AddRange(ctx.Frontend.Models);
					aggregated.Frontend.Components.AddRange(ctx.Frontend.Components);
					aggregated.Frontend.Services.AddRange(ctx.Frontend.Services);

					// merge registries & symbols
					foreach (var kv in ctx.TypeRegistry) aggregated.TypeRegistry[kv.Key] = kv.Value;
					foreach (var kv in ctx.SymbolTable) aggregated.SymbolTable[kv.Key] = kv.Value;
					if (ctx.Diagnostics?.Count > 0) aggregated.Diagnostics.AddRange(ctx.Diagnostics);
				}
				catch (Exception ex)
				{
					aggregated.Diagnostics.Add($"Failed to analyze {f}: {ex.Message}");
				}
			}

			return aggregated;
		}

		public static void SaveSemantic(SemanticContext ctx, string path)
		{
			var json = JsonSerializer.Serialize(ctx, Options);
			Directory.CreateDirectory(Path.GetDirectoryName(path) ?? "");
			File.WriteAllText(path, json);
		}
	}
}
