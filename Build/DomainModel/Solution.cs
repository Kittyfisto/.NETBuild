﻿using System;
using System.Collections.Generic;
using Build.DomainModel.MSBuild;

namespace Build.DomainModel
{
	/// <summary>
	///     Represents a visual studio solution (*.sln) file.
	/// </summary>
	public sealed class Solution
		: IFile
	{
		private readonly string _filename;
		private readonly DateTime _lastModified;
		private readonly Dictionary<string, Project> _projects;

		private Solution(string filename, DateTime lastModified, IEnumerable<Project> projects)
		{
			if (filename == null)
				throw new ArgumentNullException("filename");

			if (projects == null)
				throw new ArgumentNullException("projects");

			_filename = filename;
			_lastModified = lastModified;
			_projects = new Dictionary<string, Project>();
			foreach (Project project in projects)
			{
				_projects.Add(project.Filename, project);
			}
		}

		public IEnumerable<Project> Projects
		{
			get { return _projects.Values; }
		}

		public DateTime LastModified
		{
			get { return _lastModified; }
		}

		public Solution Clone(IReadOnlyDictionary<string, Project> projects)
		{
			var newProjects = new Dictionary<string, Project>(_projects.Count);
			foreach (string filename in _projects.Keys)
			{
				Project project;
				if (projects.TryGetValue(filename, out project))
				{
					_projects.Add(filename, project);
				}
			}
			return new Solution(_filename, _lastModified, newProjects.Values);
		}

		public override string ToString()
		{
			return _filename;
		}
	}
}