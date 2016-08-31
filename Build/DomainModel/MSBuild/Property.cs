using System;
using System.Linq;

namespace Build.DomainModel.MSBuild
{
	public sealed class Property
		: Node
	{
		private string _name;
		private string _value;

		public Property()
		{
			
		}

		public Property(string name, string value, string condition = null)
			: base(condition)
		{
			if (name == null)
				throw new ArgumentNullException("name");
			if (name.Length == 0)
				throw new ArgumentException("name must not be empty", "name");
			if (name.Any(char.IsWhiteSpace))
				throw new ArgumentException("name must not contain whitespace", "name");

			_name = name;
			_value = value;
		}

		public string Value
		{
			get { return _value; }
			set { _value = value; }
		}

		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		private bool Equals(Property other)
		{
			return string.Equals(_name, other._name) &&
			       string.Equals(_value, other._value) &&
			       Equals(Condition, other.Condition);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is Property && Equals((Property) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((_name != null ? _name.GetHashCode() : 0)*397) ^
				       (_value != null ? _value.GetHashCode() : 0) ^
				       (Condition != null ? Condition.GetHashCode() : 0);
			}
		}

		public override string ToString()
		{
			if (Condition != null)
				return string.Format("<{0} {1}>{2}</{0}>", _name, Condition, _value);

			return string.Format("<{0}>{1}</{0}>", _name, _value);
		}
	}
}