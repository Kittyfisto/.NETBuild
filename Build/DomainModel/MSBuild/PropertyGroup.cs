using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Build.DomainModel.MSBuild
{
	/// <summary>
	///     A key-value storage where the key is a string and the value is a property that
	///     consists of a string value and an optional condition.
	/// </summary>
	public sealed class PropertyGroup
		: Node
		, IPropertyGroup
	{
		private readonly Dictionary<string, Property> _properties;

		public PropertyGroup(List<Property> properties, Condition condition = null)
			: base(condition)
		{
			_properties = new Dictionary<string, Property>(properties.Count);
			AddMany(properties);
		}

		public PropertyGroup(Condition condition = null)
			: base(condition)
		{
			_properties = new Dictionary<string, Property>();
		}

		public override string ToString()
		{
			return string.Format("Count: {0}", _properties.Count);
		}

		public int Count
		{
			get { return _properties.Count; }
		}

		public Property this[string key]
		{
			get
			{
				Property property;
				if (_properties.TryGetValue(key, out property))
					return property;

				return null;
			}
		}

		public IEnumerator<Property> GetEnumerator()
		{
			return _properties.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(Property property)
		{
			_properties[property.Name] = property;
		}

		public void AddMany(IEnumerable<Property> properties)
		{
			foreach (Property property in properties)
			{
				Add(property);
			}
		}

		Property IReadOnlyList<Property>.this[int index]
		{
			get { return _properties.Values.ElementAt(index); }
		}
	}
}