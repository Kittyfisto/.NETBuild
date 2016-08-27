using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Build.DomainModel.MSBuild
{
	public sealed class ProjectItem
		: Node
	{
		private bool Equals(ProjectItem other)
		{
			if (!string.Equals(_exclude, other._exclude))
				return false;

			if (!string.Equals(_include, other._include))
				return false;

			if (!string.Equals(_remove, other._remove))
				return false;

			if (!string.Equals(_type, other._type))
				return false;

			if (_metadata.Count != other._metadata.Count)
				return false;

			foreach (var pair in _metadata)
			{
				Metadata metadata;
				if (!other._metadata.TryGetValue(pair.Key, out metadata))
					return false;

				if (!string.Equals(pair.Value.Value, metadata.Value))
					return false;
			}

			return true;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is ProjectItem && Equals((ProjectItem) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = (_exclude != null ? _exclude.GetHashCode() : 0);
				hashCode = (hashCode*397) ^ (_include != null ? _include.GetHashCode() : 0);
				hashCode = (hashCode*397) ^ (_remove != null ? _remove.GetHashCode() : 0);
				hashCode = (hashCode*397) ^ (_type != null ? _type.GetHashCode() : 0);
				return hashCode;
			}
		}

		private readonly string _exclude;
		private readonly string _include;
		private readonly string _remove;
		private readonly string _type;
		private readonly Dictionary<string, Metadata> _metadata;

		private static readonly Dictionary<string, Metadata> NoMetadata;

		static ProjectItem()
		{
			NoMetadata = new Dictionary<string, Metadata>();
		}

		public ProjectItem(string type,
		            string include,
		            string exclude = null,
		            string remove = null,
		            Condition condition = null,
					List<Metadata> metadata = null)
			: base(condition)
		{
			if (type == null)
				throw new ArgumentNullException("type");
			if (type.Any(char.IsWhiteSpace))
				throw new ArgumentException("type may not contain whitespace", "type");
			if (string.IsNullOrWhiteSpace(include))
				throw new ArgumentException("include");

			if (metadata == null || metadata.Count == 0)
			{
				_metadata = NoMetadata;
			}
			else
			{
				_metadata = new Dictionary<string, Metadata>(metadata.Count);
				foreach (var property in metadata)
				{
					if (!_metadata.ContainsKey(property.Name))
						_metadata.Add(property.Name, property);
				}
			}

			_type = type;
			_include = include;
			_exclude = exclude;
			_remove = remove;
		}

		public IEnumerable<Metadata> Metadata
		{
			get { return _metadata.Values; }
		}

		public string this[string metadataName]
		{
			get
			{
				Metadata metadata;
				if (_metadata.TryGetValue(metadataName, out metadata))
					return metadata.Value;

				return string.Empty;
			}
		}

		public string Type
		{
			get { return _type; }
		}

		public string Include
		{
			get { return _include; }
		}

		public string Exclude
		{
			get { return _exclude; }
		}

		public string Remove
		{
			get { return _remove; }
		}

		public override string ToString()
		{
			var builder = new StringBuilder();
			builder.AppendFormat("<{0} Include=\"{1}\" ",
			                     _type,
			                     _include);

			if (_exclude != null)
			{
				builder.AppendFormat("Exclude=\"{0}\" ", _exclude);
			}

			if (_remove != null)
			{
				builder.AppendFormat("Remove=\"{0}\" ", _remove);
			}

			if (_metadata == NoMetadata)
			{
				builder.Append("/>");
			}
			else
			{
				builder.AppendLine(">");
				foreach (var metadata in _metadata.Values)
				{
					builder.AppendFormat("	{0}", metadata);
				}
				builder.AppendFormat("</{0}>", _type);
			}

			return builder.ToString();
		}
	}
}