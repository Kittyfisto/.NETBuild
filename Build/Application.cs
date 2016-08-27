using System;
using System.Diagnostics;
using Build.BuildEngine;
using Build.Parser;

namespace Build
{
	public sealed class Application : IDisposable
	{
		public static int Main(string[] args)
		{
			using (var application = new Application())
			{
				return application.Run();
			}
		}

		private int Run()
		{
			var sw = new Stopwatch();
			sw.Start();

			var expressionEngine = new ExpressionEngine.ExpressionEngine();
			var assembyResolver = new AssemblyResolver(expressionEngine);

			var loader = CSharpProjectParser.Instance;
			const string fname = @"C:\Snapshots\Dashboard\Build\Build.csproj";
			var project = loader.Parse(fname);
			var environment = new BuildEnvironment();
			var evaluated = expressionEngine.Evaluate(project, environment);

			var compiler = new CSharpProjectCompiler(assembyResolver, evaluated, environment);
			var exitCode = compiler.Run();
			Console.WriteLine(compiler.Output);

			sw.Stop();
			Console.WriteLine("Project took: {0}ms", sw.ElapsedMilliseconds);

			return exitCode;
		}

		public void Dispose()
		{
			
		}
	}
}