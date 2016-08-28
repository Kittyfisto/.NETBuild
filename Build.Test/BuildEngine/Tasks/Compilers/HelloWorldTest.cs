using System;
using Build.BuildEngine;
using Build.BuildEngine.Tasks.Compilers;
using Build.Parser;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Build.Test.BuildEngine.Tasks.Compilers
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

			Console.WriteLine();
			var logger = new Mock<ILogger>();
			logger.Setup(x => x.WriteMultiLine(It.IsAny<Verbosity>(), It.IsAny<string>()))
			      .Callback((Verbosity unused, string message) =>
				      {
					      Console.WriteLine(message);
				      });
			var compiler = new CSharpProjectCompiler(AssemblyResolver, logger.Object, project, environment);
			new Action(compiler.Run).ShouldNotThrow();

			FileExists(@"TestData\CSharp\HelloWorld\bin\Debug\HelloWorld.exe").Should().BeTrue();
			FileExists(@"TestData\CSharp\HelloWorld\bin\Debug\HelloWorld.pdb").Should().BeTrue();

			string output;
			var exitCode = Run(@"TestData\CSharp\HelloWorld\bin\Debug\HelloWorld.exe", out output);
			exitCode.Should().Be(0);
			output.Should().Be("Hello World!");
		}
	}
}