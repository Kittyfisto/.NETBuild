using System;
using System.Collections.Generic;

namespace Build.DomainModel.MSBuild
{
	/// <summary>
	///     Represents a visual studio C# project (*.csproj).
	/// </summary>
	public sealed class Project
		: IFile
	{
		private readonly List<PropertyGroup> _propertyGroups;
		private readonly List<ItemGroup> _itemGroups;
		private readonly List<Target> _targets;

		public Project()
		{
			_propertyGroups = new List<PropertyGroup>();
			_itemGroups = new List<ItemGroup>();
			_targets = new List<Target>();
		}

		public List<PropertyGroup> Properties
		{
			get { return _propertyGroups; }
		}

		public List<ItemGroup> ItemGroups
		{
			get { return _itemGroups; }
		}

		public string Filename { get; set; }

		public DateTime LastModified
		{
			get
			{
				return DateTime.Now;
			}
		}

		public List<Target> Targets
		{
			get { return _targets; }
		}

		public override string ToString()
		{
			return Filename;
		}
	}
}