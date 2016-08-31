using System;
using System.Collections;
using System.Collections.Generic;

namespace Build.DomainModel.MSBuild
{
	public sealed class ReadOnlyPropertyGroup
		: IPropertyGroup
	{
		private readonly Property[] _values;

		public static readonly ReadOnlyPropertyGroup Instance;

		static ReadOnlyPropertyGroup()
		{
			Instance = new ReadOnlyPropertyGroup();
		}

		public override string ToString()
		{
			return "Count: 0";
		}

		private ReadOnlyPropertyGroup()
		{
			_values = new Property[0];
		}

		public IEnumerator<Property> GetEnumerator()
		{
			return ((IEnumerable<Property>)_values).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public string Condition
		{
			get { return null; }
		}

		public int Count
		{
			get { return 0; }
		}

		public Property this[int index]
		{
			get { throw new IndexOutOfRangeException(); }
		}
	}
}