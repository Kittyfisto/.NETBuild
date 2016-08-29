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
		private readonly List<Property> _properties;
		private readonly Dictionary<string, Property> _propertiesByName;

		public PropertyGroup(List<Property> properties, Condition condition = null)
			: base(condition)
		{
			_properties = properties;
			_propertiesByName = new Dictionary<string, Property>(properties.Count);
			AddMany(properties);
		}

		public PropertyGroup(Condition condition = null)
			: base(condition)
		{
			_properties = new List<Property>();
			_propertiesByName = new Dictionary<string, Property>();
		}

		public override string ToString()
		{
			return string.Format("Count: {0}", Count);
		}

		public int Count
		{
			get { return _properties.Count; }
		}

		public Property this[int index]
		{
			get { return _properties[index]; }
		}

		public Property this[string key]
		{
			get
			{
				Property property;
				if (_propertiesByName.TryGetValue(key, out property))
					return property;

				return null;
			}
		}

		public IEnumerator<Property> GetEnumerator()
		{
			return _propertiesByName.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(Property property)
		{
			_propertiesByName[property.Name] = property;
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
			get { return _propertiesByName.Values.ElementAt(index); }
		}
	}
}