using System;
using Build.BuildEngine;
using Build.Parser;
using FluentAssertions;
using NUnit.Framework;

namespace Build.Test.BuildEngine.CompilerTest
{
	[TestFixture]
	public sealed class HelloWorldTest
		: AbstractCompilerTest
	{
		[Test]
		[Description("Verifies that compiling a simple c# console application works")]
		public void TestCompile1()
		{
			var filepath = TestPath.Get(@"TestData\CSharp\HelloWorld\HelloWorld.csproj");
			var environment = new BuildEnvironment();
			var project = ExpressionEngine.Evaluate(CSharpProjectParser.Instance.Parse(filepath), environment);

			Clean(@"TestData\CSharp\HelloWorld\bin\Debug\");

			var compiler = new CSharpProjectCompiler(AssemblyResolver, project, environment);
			int exitCode = compiler.Run();
			Console.WriteLine();
			Console.WriteLine(compiler.Output);
			exitCode.Should().Be(0);

			FileExists(@"TestData\CSharp\HelloWorld\bin\Debug\HelloWorld.exe").Should().BeTrue();
			FileExists(@"TestData\CSharp\HelloWorld\bin\Debug\HelloWorld.pdb").Should().BeTrue();

			string output;
			exitCode = Run(@"TestData\CSharp\HelloWorld\bin\Debug\HelloWorld.exe", out output);
			exitCode.Should().Be(0);
			output.Should().Be("Hello World!");
		}
	}
}