﻿using System.Collections;
using System.Collections.Generic;
using Build.DomainModel.MSBuild;

namespace Build.BuildEngine
{
	public sealed class BuildEnvironment
	{
		public const string Configuration = "Configuration";
		public const string Platform = "Platform";
		public const string OutputPath = "OutputPath";

		private readonly BuildEnvironment _default;
		private readonly EnvironmentItemLists _itemLists;
		private readonly string _name;
		private readonly BuildEnvironment _parent;

		private readonly EnvironmentProperties _properties;

		public BuildEnvironment(BuildEnvironment parent = null, BuildEnvironment @default = null, string name = null)
		{
			_name = name;
			_parent = parent;
			_default = @default;
			_properties = new EnvironmentProperties(
				_parent != null ? _parent._properties : null,
				_default != null ? _default._properties : null
				);
			_itemLists = new EnvironmentItemLists(
				_parent != null ? _parent._itemLists : null,
				_default != null ? _default._itemLists : null
				);
		}

		public EnvironmentProperties Properties
		{
			get { return _properties; }
		}

		public EnvironmentItemLists ItemLists
		{
			get { return _itemLists; }
		}

		public override string ToString()
		{
			if (_name == null)
				return "<Unnamed>";

			return _name;
		}

		public sealed class EnvironmentItemLists
			: IEnumerable<ProjectItem>
		{
			private readonly EnvironmentItemLists _defaultItems;
			private readonly EnvironmentItemLists _parentItems;
			private readonly Dictionary<string, ProjectItem> _items;

			public EnvironmentItemLists(EnvironmentItemLists parentItems = null,
			                            EnvironmentItemLists defaultItems = null)
			{
				_parentItems = parentItems;
				_defaultItems = defaultItems;
				_items = new Dictionary<string, ProjectItem>();
			}

			public ProjectItem this[string itemName]
			{
				get
				{
					ProjectItem value;
					TryGetValue(itemName, out value);
					return value;
				}
				set { _items[itemName] = value; }
			}

			public IEnumerator<ProjectItem> GetEnumerator()
			{
				return _items.Values.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			public bool TryGetValue(string itemName, out ProjectItem item)
			{
				if (_items.TryGetValue(itemName, out item))
					return true;

				if (_parentItems != null && _parentItems.TryGetValue(itemName, out item))
					return true;

				if (_defaultItems != null && _defaultItems.TryGetValue(itemName, out item))
					return true;

				item = null;
				return false;
			}

			public void Add(ProjectItem item)
			{
				_items[item.Include] = item;
			}

			public List<ProjectItem> GetItemsOfType(string type)
			{
				var items = new List<ProjectItem>();
				foreach (var item in _items.Values)
				{
					if (string.Equals(item.Type, type))
						items.Add(item);
				}
				return items;
			}
		}

		public sealed class EnvironmentProperties
			: IEnumerable<KeyValuePair<string, string>>
		{
			private readonly EnvironmentProperties _defaultProperties;
			private readonly EnvironmentProperties _parentProperties;
			private readonly Dictionary<string, string> _values;

			public EnvironmentProperties(EnvironmentProperties parentProperties = null,
			                             EnvironmentProperties defaultProperties = null)
			{
				_parentProperties = parentProperties;
				_defaultProperties = defaultProperties;
				_values = new Dictionary<string, string>();
			}

			public string this[string propertyName]
			{
				get
				{
					string value;
					TryGetValue(propertyName, out value);
					return value;
				}
				set { _values[propertyName] = value; }
			}

			public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
			{
				return _values.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			public bool TryGetValue(string propertyName, out string propertyValue)
			{
				if (_values.TryGetValue(propertyName, out propertyValue))
					return true;

				if (_parentProperties != null && _parentProperties.TryGetValue(propertyName, out propertyValue))
					return true;

				if (_defaultProperties != null && _defaultProperties.TryGetValue(propertyName, out propertyValue))
					return true;

				propertyValue = string.Empty;
				return false;
			}

			public void Add(string name, string value)
			{
				_values[name] = value;
			}
		}
	}
}