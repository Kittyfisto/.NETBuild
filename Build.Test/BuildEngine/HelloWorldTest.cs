using NUnit.Framework;

namespace Build.Test.BuildEngine
{
	[TestFixture]
	public sealed class HelloWorldTest
	{
		[Test]
		public void TestRun1()
		{
			var arguments = new Arguments
				{
					InputFile = TestPath.Get(@"TestData\CSharp\HelloWorld\HelloWorld.csproj"),
				};
			using (var engine = new Build.BuildEngine.BuildEngine(arguments))
			{
				engine.Execute();
			}
		}
	}
}