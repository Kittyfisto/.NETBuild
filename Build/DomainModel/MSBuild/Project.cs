using System;

namespace Build.DomainModel.MSBuild
{
	/// <summary>
	///     Represents a visual studio C# project (*.csproj).
	/// </summary>
	public sealed class Project
		: IFile
	{
		private readonly IPropertyGroups _propertyGroups;
		private readonly IItemGroups _itemGroups;
		private readonly string _filename;

		public Project(string filename,
							 IPropertyGroups propertyGroups = null,
							 IItemGroups itemGroups = null)
		{
			if (filename == null)
				throw new ArgumentNullException("filename");

			_filename = filename;
			_propertyGroups = propertyGroups ?? ReadOnlyPropertyGroups.Instance;
			_itemGroups = itemGroups ?? ReadOnlyItemGroups.Instance;
		}

		public IPropertyGroups Properties
		{
			get { return _propertyGroups; }
		}

		public IItemGroups ItemGroups
		{
			get { return _itemGroups; }
		}

		public string Filename
		{
			get { return _filename; }
		}

		public DateTime LastModified
		{
			get
			{
				return DateTime.Now;
			}
		}

		public override string ToString()
		{
			return _filename;
		}
	}
}