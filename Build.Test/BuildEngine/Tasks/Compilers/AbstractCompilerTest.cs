using System;
using System.Diagnostics.Contracts;
using System.IO;
using Build.BuildEngine;
using NUnit.Framework;

namespace Build.Test.BuildEngine.Tasks.Compilers
{
	public abstract class AbstractCompilerTest
	{
		private AssemblyResolver _assemblyResolver;
		private Build.ExpressionEngine.ExpressionEngine _expressionEngine;
		private TestLogger _logger;

		protected TestLogger Logger
		{
			get { return _logger; }
		}

		public Build.ExpressionEngine.ExpressionEngine ExpressionEngine
		{
			get { return _expressionEngine; }
		}

		public AssemblyResolver AssemblyResolver
		{
			get { return _assemblyResolver; }
		}

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			_expressionEngine = new Build.ExpressionEngine.ExpressionEngine();
			_assemblyResolver = new AssemblyResolver(_expressionEngine);
			_logger = new TestLogger();
		}

		public sealed class TestLogger
			: ILogger
		{
			public void WriteLine(Verbosity verbosity, string format, params object[] arguments)
			{
				Console.WriteLine(format, arguments);
			}

			public void WriteMultiLine(Verbosity verbosity, string message)
			{
				var lines = message.Split(new[] {"\n", "\r\n"}, StringSplitOptions.RemoveEmptyEntries);
				foreach (var line in lines)
				{
					Console.WriteLine(line);
				}
			}
		}

		[Pure]
		protected bool FileExists(string relativeFilePath)
		{
			var path = TestPath.Get(relativeFilePath);
			return File.Exists(path);
		}

		protected int Run(string relativeFileName, out string output)
		{
			var fullPath = TestPath.Get(relativeFileName);
			var workingDirectory = Path.GetDirectory(fullPath);

			Console.WriteLine("Executing '{0}'", fullPath);

			int exitCode = ProcessEx.Run(fullPath,
			                             workingDirectory,
			                             new ArgumentBuilder(),
			                             out output);
			return exitCode;
		}

		protected void Clean(string relativeFolderName)
		{
			if (relativeFolderName.Contains(".."))
				throw new Exception();

			var folderPath = Path.Normalize(TestPath.Get(relativeFolderName));
			if (!Directory.Exists(folderPath))
				return;

			Console.WriteLine("Cleaning '{0}'", folderPath);
			var files = Directory.EnumerateFiles(folderPath, "*.*");
			foreach (var file in files)
			{
				File.Delete(file);
			}
		}
	}
}