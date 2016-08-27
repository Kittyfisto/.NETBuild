using System;
using System.Collections;
using System.Collections.Generic;

namespace Build.DomainModel.MSBuild
{
	public sealed class ItemGroup
		: Node
		, IItemGroup
	{
		private readonly List<ProjectItem> _items;

		public ItemGroup(IEnumerable<ProjectItem> items, Condition condition = null)
			: base(condition)
		{
			if (items == null)
				throw new ArgumentNullException("items");

			_items = new List<ProjectItem>(items);
		}

		public ItemGroup(Condition condition = null)
			: base(condition)
		{
			_items = new List<ProjectItem>();
		}

		public override string ToString()
		{
			return string.Format("Count: {0}", _items.Count);
		}

		public void Add(ProjectItem item)
		{
			_items.Add(item);
		}

		public IEnumerator<ProjectItem> GetEnumerator()
		{
			return _items.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public int Count
		{
			get { return _items.Count; }
		}

		public ProjectItem this[int index]
		{
			get { return _items[index]; }
		}
	}
}