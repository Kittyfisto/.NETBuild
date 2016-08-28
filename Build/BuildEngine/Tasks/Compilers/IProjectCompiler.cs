using System.Collections.Generic;

namespace Build.BuildEngine.Tasks.Compilers
{
	public interface IProjectCompiler
	{
		/// <summary>
		///     The complete file path of the primary outputfile, for example
		///     "C:\Snapshots\.NETBuild\Build\bin\Debug\Build.exe"
		/// </summary>
		string OutputFilePath { get; }

		/// <summary>
		///     The complete file paths of all additional output files, for example
		///     "C:\Snapshots\.NETBuild\Build\bin\Debug\Build.pdb"
		/// </summary>
		IEnumerable<string> AdditionalOutputFiles { get; }

		/// <summary>
		/// Compiles the project.
		/// </summary>
		void Run();
	}
}