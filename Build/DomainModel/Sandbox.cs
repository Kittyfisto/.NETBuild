using System;
using System.Collections.Generic;
using Build.DomainModel.MSBuild;

namespace Build.DomainModel
{
	/// <summary>
	///     Represents the entire contents of a sandbox (a directory tree on the filesystem).
	/// </summary>
	public sealed class Sandbox
	{
		private readonly List<Project> _projects;
		private readonly List<Solution> _solutions;

		public Sandbox(IEnumerable<Solution> solutions, IEnumerable<Project> projects)
		{
			if (solutions == null)
				throw new ArgumentNullException("solutions");

			_solutions = new List<Solution>(solutions);
			_projects = new List<Project>(projects);
		}

		public override string ToString()
		{
			return string.Format("{0} Solution(s), {1} Project(s)", _solutions.Count, _projects.Count);
		}

		public IEnumerable<Project> Projects
		{
			get { return _projects; }
		}

		public IEnumerable<Solution> Solutions
		{
			get { return _solutions; }
		}
	}
}