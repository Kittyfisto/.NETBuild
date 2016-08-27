using System.Collections.Generic;
using Build.DomainModel;

namespace Build.Watchdog
{
	/// <summary>
	///     Is responsible for creating in-memory-representations of files in a sandbox.
	/// </summary>
	public sealed class SandboxLoader
	{
		private readonly Dictionary<Filetype, IFileStore> _files;
		private readonly CSharpProjectStore _cSharpProjects;
		private readonly SolutionStore _solutions;
		private bool _isSandboxDirty;
		private Sandbox _lastSandbox;

		public SandboxLoader()
		{
			_cSharpProjects = new CSharpProjectStore();
			_solutions = new SolutionStore();
			_files = new Dictionary<Filetype, IFileStore>
				{
					{Filetype.Project, _cSharpProjects},
					{Filetype.Solution, _solutions}
				};
			_isSandboxDirty = true;
		}

		private static bool TryGetExtension(string filename, out string extension)
		{
			extension = Path.GetExtension(filename);
			if (extension == null)
				return false;

			extension = extension.ToLowerInvariant();
			return true;
		}

		private static Filetype GetFileType(string filename)
		{
			string extension;
			if (!TryGetExtension(filename, out extension))
			{
				return Filetype.Unknown;
			}

			switch (extension)
			{
				case ".csproj":
					return Filetype.Project;

				case "*.sln":
					return Filetype.Solution;

				default:
					return Filetype.Unknown;
			}
		}

		public void CreateOrUpdate(string filename)
		{
			Filetype type = GetFileType(filename);
			IFileStore store;
			if (_files.TryGetValue(type, out store))
			{
				store.CreateOrUpdate(filename);
				_isSandboxDirty = true;
			}
		}

		public void Delete(string filename)
		{
			Filetype type = GetFileType(filename);
			IFileStore store;
			if (_files.TryGetValue(type, out store))
			{
				store.Remove(filename);
				_isSandboxDirty = true;
			}
		}

		/// <summary>
		///     Creates a new sandbox from the current values.
		/// </summary>
		/// <returns></returns>
		public Sandbox CreateSandbox()
		{
			// We are called pretty regularly and we want to avoid creating garbage when *nothing* has changed
			// (which is a pretty significant amount of total time this class is used) and therefore we
			// only create new objects when something has changed.
			if (_isSandboxDirty)
			{
				var projects = _cSharpProjects.CreateProjects();
				IEnumerable<Solution> solutions = _solutions.CreateSolutions(projects);
				_lastSandbox = new Sandbox(solutions, projects.Values);
				_isSandboxDirty = false;
			}

			return _lastSandbox;
		}
	}
}