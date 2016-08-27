using System;
using System.Collections;
using System.Collections.Generic;

namespace Build.DomainModel.MSBuild
{
	public sealed class ReadOnlyPropertyGroups
		: IPropertyGroups
	{
		public static readonly ReadOnlyPropertyGroups Instance;

		static ReadOnlyPropertyGroups()
		{
			Instance = new ReadOnlyPropertyGroups();
		}

		private ReadOnlyPropertyGroups()
		{
			_groups = new IPropertyGroup[0];
		}

		public override string ToString()
		{
			return "Count: 0";
		}

		private readonly IPropertyGroup[] _groups;

		public IEnumerator<IPropertyGroup> GetEnumerator()
		{
			return ((IEnumerable<IPropertyGroup>)_groups).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public int Count
		{
			get { return 0; }
		}

		public IPropertyGroup this[int index]
		{
			get { throw new IndexOutOfRangeException(); }
		}
	}
}