using System;
using System.Collections;
using System.Collections.Generic;

namespace Build.DomainModel.MSBuild
{
	public sealed class PropertyGroups
		: IPropertyGroups
	{
		private readonly List<IPropertyGroup> _groups;

		public PropertyGroups(IEnumerable<IPropertyGroup> groups)
		{
			if (groups == null)
				throw new ArgumentNullException("groups");

			_groups = new List<IPropertyGroup>(groups);
		}

		public override string ToString()
		{
			return string.Format("Count: {0}", _groups.Count);
		}

		public IEnumerator<IPropertyGroup> GetEnumerator()
		{
			return _groups.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public int Count
		{
			get { return _groups.Count; }
		}

		public IPropertyGroup this[int index]
		{
			get { return _groups[index]; }
		}
	}
}