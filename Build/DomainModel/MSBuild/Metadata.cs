using System;
using System.Linq;

namespace Build.DomainModel.MSBuild
{
	public sealed class Metadata
	{
		private readonly string _name;
		private readonly string _value;

		public Metadata(string name, string value)
		{
			if (name == null)
				throw new ArgumentNullException("name");

			if (name.Any(char.IsWhiteSpace))
				throw new ArgumentException("name may not contain whitespace", "name");

			_name = name;
			_value = value;
		}

		public string Name
		{
			get { return _name; }
		}

		public string Value
		{
			get { return _value; }
		}

		private bool Equals(Metadata other)
		{
			return string.Equals(_value, other._value) && string.Equals(_name, other._name);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is Metadata && Equals((Metadata) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((_value != null ? _value.GetHashCode() : 0)*397) ^ (_name != null ? _name.GetHashCode() : 0);
			}
		}

		public override string ToString()
		{
			return string.Format("<{0}>{1}</{0}>", _name, _value);
		}
	}
}