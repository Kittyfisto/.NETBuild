using System;
using System.Collections;
using System.Collections.Generic;

namespace Build.DomainModel.MSBuild
{
	public sealed class ItemGroups
		: IItemGroups
	{
		private readonly List<IItemGroup> _groups;

		public ItemGroups(IEnumerable<IItemGroup> groups)
		{
			if (groups == null)
				throw new ArgumentNullException("groups");

			_groups = new List<IItemGroup>(groups);
		}

		public override string ToString()
		{
			return string.Format("Count: {0}", _groups.Count);
		}

		public IEnumerator<IItemGroup> GetEnumerator()
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

		public IItemGroup this[int index]
		{
			get { return _groups[index]; }
		}
	}
}