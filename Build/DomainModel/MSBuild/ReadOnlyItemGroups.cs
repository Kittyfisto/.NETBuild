using System;
using System.Collections;
using System.Collections.Generic;

namespace Build.DomainModel.MSBuild
{
	public sealed class ReadOnlyItemGroups
		: IItemGroups
	{
		public static readonly ReadOnlyItemGroups Instance;

		static ReadOnlyItemGroups()
		{
			Instance = new ReadOnlyItemGroups();
		}

		private readonly List<IItemGroup> _items;

		private ReadOnlyItemGroups()
		{
			_items = new List<IItemGroup>();
		}

		public IEnumerator<IItemGroup> GetEnumerator()
		{
			return _items.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public int Count
		{
			get { return 0; }
		}

		public IItemGroup this[int index]
		{
			get { throw new IndexOutOfRangeException(); }
		}
	}
}