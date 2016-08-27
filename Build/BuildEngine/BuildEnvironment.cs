using System.Collections;
using System.Collections.Generic;

namespace Build.BuildEngine
{
	public sealed class BuildEnvironment
		: IEnumerable<KeyValuePair<string, string>>
	{
		public const string Configuration = "Configuration";
		public const string Platform = "Platform";
		public const string OutputPath = "OutputPath";

		private readonly BuildEnvironment _parent;
		private readonly BuildEnvironment _default;
		private readonly Dictionary<string, string> _values;

		public BuildEnvironment(BuildEnvironment parent = null, BuildEnvironment @default = null)
		{
			_parent = parent;
			_default = @default;
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
			set
			{
				_values[propertyName] = value;
			}
		}

		public bool TryGetValue(string propertyName, out string propertyValue)
		{
			if (_values.TryGetValue(propertyName, out propertyValue))
				return true;

			if (_parent != null && _parent.TryGetValue(propertyName, out propertyValue))
				return true;

			if (_default != null && _default.TryGetValue(propertyName, out propertyValue))
				return true;

			propertyValue = string.Empty;
			return false;
		}

		public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
		{
			return _values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(string name, string value)
		{
			_values[name] = value;
		}
	}
}