using System.IO;
using Build.BuildEngine;
using NUnit.Framework;

namespace Build.Test.TaskEngine
{
	[TestFixture]
	public sealed class TaskEngineAcceptanceTest
	{
		private IFileSystem _fileSystem;
		private Build.ExpressionEngine.ExpressionEngine _expressionEngine;
		private Build.TaskEngine.TaskEngine _engine;
		private BuildLog _buildLog;
		private BuildEnvironment _environment;

		[SetUp]
		public void SetUp()
		{
			_buildLog = new BuildLog(new Arguments(), new MemoryStream());
			_fileSystem = new FileSystem();
			_expressionEngine = new Build.ExpressionEngine.ExpressionEngine(_fileSystem);
			_engine = new Build.TaskEngine.TaskEngine(_expressionEngine, _fileSystem);
			_environment = new BuildEnvironment();
		}

		#region EmbeddedResource

		#endregion

	}
}