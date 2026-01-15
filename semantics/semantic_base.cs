using System.Collections.Generic;
using Transpiler.AST;

namespace Transpiler.Semantics
{
	public interface ISemanticAnalyzer
	{
		SemanticContext Analyze(AstUnit unit);
	}

	public abstract class SemanticAnalyzerBase : ISemanticAnalyzer
	{
		public virtual SemanticContext Analyze(AstUnit unit)
		{
			var ctx = new SemanticContext
			{
				OriginalAst = unit,
				Backend = new BackendModel(),
				Frontend = new FrontendModel(),
				TypeRegistry = new Dictionary<string, TypeMetadata>(),
				SymbolTable = new Dictionary<string, SymbolInfo>()
			};

			BuildTypeRegistry(unit, ctx);
			BuildSymbols(unit, ctx);
			Transform(unit, ctx);

			return ctx;
		}

		protected virtual void BuildTypeRegistry(AstUnit unit, SemanticContext ctx) { }
		protected virtual void BuildSymbols(AstUnit unit, SemanticContext ctx) { }
		protected virtual void Transform(AstUnit unit, SemanticContext ctx) { }
	}

	// Semantic context produced by analyzers
	public class SemanticContext
	{
		public AstUnit OriginalAst { get; set; } = new();
		public BackendModel Backend { get; set; } = new();
		public FrontendModel Frontend { get; set; } = new();
		public Dictionary<string, TypeMetadata> TypeRegistry { get; set; } = new();
		public Dictionary<string, SymbolInfo> SymbolTable { get; set; } = new();
		public List<string> Diagnostics { get; set; } = new();
	}

	// Minimal metadata / IR shapes used by generators
	public class BackendModel
	{
		public List<EntityDefinition> Entities { get; set; } = new();
		public List<ControllerDefinition> Controllers { get; set; } = new();
		public List<ServiceDefinition> Services { get; set; } = new();
	}

	public class FrontendModel
	{
		public List<ModelDefinition> Models { get; set; } = new();
		public List<ComponentDefinition> Components { get; set; } = new();
		public List<ServiceDefinition> Services { get; set; } = new();
	}

	public class EntityDefinition
	{
		public string Name { get; set; } = string.Empty;
		public List<FieldDefinition> Fields { get; set; } = new();
	}

	public class FieldDefinition
	{
		public string Name { get; set; } = string.Empty;
		public string TypeName { get; set; } = string.Empty;
	}

	public class ControllerDefinition
	{
		public string Name { get; set; } = string.Empty;
		public List<OperationDefinition> Operations { get; set; } = new();
	}

	public class ServiceDefinition
	{
		public string Name { get; set; } = string.Empty;
		public List<OperationDefinition> Operations { get; set; } = new();
	}

	public class OperationDefinition
	{
		public string Name { get; set; } = string.Empty;
		public List<ParameterDefinition> Parameters { get; set; } = new();
		public string ReturnType { get; set; } = string.Empty;
	}

	public class ParameterDefinition
	{
		public string Name { get; set; } = string.Empty;
		public string TypeName { get; set; } = string.Empty;
	}

	public class ModelDefinition
	{
		public string Name { get; set; } = string.Empty;
		public List<FieldDefinition> Fields { get; set; } = new();
	}

	public class ComponentDefinition
	{
		public string Name { get; set; } = string.Empty;
		public List<string> UsedModels { get; set; } = new();
	}

	// helpers
	public class TypeMetadata
	{
		public string Name { get; set; } = string.Empty;
	}

	public class SymbolInfo
	{
		public string Name { get; set; } = string.Empty;
		public string Kind { get; set; } = string.Empty;
	}
}
