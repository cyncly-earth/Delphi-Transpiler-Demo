using System;

class Program
{
	static void Main()
	{
		// 1. Parse + AST
		var ast = BookingAstBuilder.Build();

		// 2. Semantic → IR
		ScreenIR ir = BookingSemantic.Analyze(ast);

		// 3. Generate Angular
		AngularGenerator.Generate(ir);

		Console.WriteLine("Angular code generated!");
	}
}

