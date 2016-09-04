using NUnit.Framework;

namespace Build.Test.BuildEngine
{
	[TestFixture]
	public sealed class ReferencesLibraryTest
		: AbstractBuildEngineTest
	{
		protected override string[] ProjectDirectories
		{
			get
			{
				return new[]
					{
						"Library",
						"ReferencesLibrary"
					};
			}
		}

		protected override string[] ExpectedOutputFiles
		{
			get
			{
				return new[]
					{
						@"ReferencesLibrary\bin\Debug\ReferencesLibrary.exe",
						@"ReferencesLibrary\bin\Debug\ReferencesLibrary.pdb",
						@"ReferencesLibrary\bin\Debug\ReferencesLibrary.exe.config",
						@"ReferencesLibrary\bin\Debug\Library.dll",
						@"ReferencesLibrary\bin\Debug\Library.pdb"
					};
			}
		}

		protected override string ProjectFilePath
		{
			get { return @"ReferencesLibrary\ReferencesLibrary.csproj"; }
		}

		protected override void PostBuildChecks()
		{
			
		}
	}
}