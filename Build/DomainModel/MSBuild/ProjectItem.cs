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
			if (!string.Equals(Exclude, other.Exclude))
				return false;

			if (!string.Equals(Include, other.Include))
				return false;

			if (!string.Equals(Remove, other.Remove))
				return false;

			if (!string.Equals(Type, other.Type))
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
				int hashCode = (Exclude != null ? Exclude.GetHashCode() : 0);
				hashCode = (hashCode*397) ^ (Include != null ? Include.GetHashCode() : 0);
				hashCode = (hashCode*397) ^ (Remove != null ? Remove.GetHashCode() : 0);
				hashCode = (hashCode*397) ^ (Type != null ? Type.GetHashCode() : 0);
				return hashCode;
			}
		}

		private readonly Dictionary<string, Metadata> _metadata;

		private static readonly Dictionary<string, Metadata> NoMetadata;

		static ProjectItem()
		{
			NoMetadata = new Dictionary<string, Metadata>();
		}

		public ProjectItem()
		{
			_metadata = new Dictionary<string, Metadata>();
		}

		public ProjectItem(string type,
		            string include,
		            string exclude = null,
		            string remove = null,
		            string condition = null,
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

			Type = type;
			Include = include;
			Exclude = exclude;
			Remove = remove;
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
			set
			{
				_metadata[metadataName] = new Metadata(metadataName, value);
			}
		}

		public string Type { get; set; }

		public string Include { get; set; }

		public string Exclude { get; set; }

		public string Remove { get; set; }

		public override string ToString()
		{
			var builder = new StringBuilder();
			builder.AppendFormat("<{0} Include=\"{1}\" ",
			                     Type,
			                     Include);

			if (Exclude != null)
			{
				builder.AppendFormat("Exclude=\"{0}\" ", Exclude);
			}

			if (Remove != null)
			{
				builder.AppendFormat("Remove=\"{0}\" ", Remove);
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
				builder.AppendFormat("</{0}>", Type);
			}

			return builder.ToString();
		}
	}
}