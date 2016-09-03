using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Build.DomainModel.MSBuild
{
	/// <summary>
	///     Represents a visual studio C# project (*.csproj).
	/// </summary>
	public sealed class Project
		: IFile
	{
		private readonly List<ItemGroup> _itemGroups;
		private readonly List<PropertyGroup> _propertyGroups;
		private readonly List<Target> _targets;

		public Project()
		{
			_propertyGroups = new List<PropertyGroup>();
			_itemGroups = new List<ItemGroup>();
			_targets = new List<Target>();
		}

		public string Filename { get; set; }

		public List<PropertyGroup> Properties
		{
			get { return _propertyGroups; }
		}

		public List<ItemGroup> ItemGroups
		{
			get { return _itemGroups; }
		}

		public List<Target> Targets
		{
			get { return _targets; }
		}

		public DateTime LastModified
		{
			get { return DateTime.Now; }
		}

		[Pure]
		public Project Merged(Project other)
		{
			var project = new Project
				{
					Filename = Filename,
				};

			project.ItemGroups.AddRange(ItemGroups.Concat(other.ItemGroups));
			project.Properties.AddRange(Properties.Concat(other.Properties));
			project.Targets.AddRange(Targets.Concat(other.Targets));

			return project;
		}

		public override string ToString()
		{
			return Filename;
		}
	}
}