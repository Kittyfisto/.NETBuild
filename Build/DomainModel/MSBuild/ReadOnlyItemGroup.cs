using System;
using System.Collections;
using System.Collections.Generic;

namespace Build.DomainModel.MSBuild
{
	public sealed class ReadOnlyItemGroup
		: Node
		, IItemGroup
	{
		public static readonly ReadOnlyItemGroup Instance;
		private readonly ProjectItem[] _items;

		static ReadOnlyItemGroup()
		{
			Instance = new ReadOnlyItemGroup();
		}

		private ReadOnlyItemGroup() : base(null)
		{
			_items = new ProjectItem[0];
		}

		public override string ToString()
		{
			return string.Format("Count: {0}", _items.Length);
		}

		public IEnumerator<ProjectItem> GetEnumerator()
		{
			return ((IEnumerable<ProjectItem>)_items).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public int Count
		{
			get { return 0; }
		}

		public ProjectItem this[int index]
		{
			get { throw new IndexOutOfRangeException(); }
		}
	}
}