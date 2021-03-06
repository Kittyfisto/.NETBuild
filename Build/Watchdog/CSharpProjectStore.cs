﻿using System.Collections.Generic;
using Build.DomainModel.MSBuild;
using Build.IO;
using Build.Parser;

namespace Build.Watchdog
{
	public sealed class CSharpProjectStore
		: FileStore<Project>
	{
		public CSharpProjectStore(IFileSystem fileSystem)
			: base(new ProjectParser(fileSystem))
		{}

		public IReadOnlyDictionary<string, Project> CreateProjects()
		{
			// Currently projects are immutable and thus we don't need to clone them in any way
			var projects = new Dictionary<string, Project>(Count);
			foreach (Project project in Values)
			{
				projects.Add(project.Filename, project);
			}
			return projects;
		}
	}
}