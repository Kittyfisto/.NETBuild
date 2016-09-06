using System;
using System.Collections.Generic;
using Build.DomainModel.MSBuild;

namespace Build.ExpressionEngine
{
	public sealed class MetadataReference
		: IExpression
	{
		private readonly string _metadata;

		public MetadataReference(string metadata)
		{
			if (metadata == null)
				throw new ArgumentNullException("metadata");

			_metadata = metadata;
		}

		public object Evaluate(IFileSystem fileSystem, BuildEnvironment environment)
		{
			throw new NotImplementedException();
		}

		public string ToString(IFileSystem fileSystem, BuildEnvironment environment)
		{
			throw new NotImplementedException();
		}

		public string ToString(IFileSystem fileSystem, BuildEnvironment environment, ProjectItem item)
		{
			if (item == null)
				return string.Empty;

			var value = item[_metadata];
			return value;
		}

		public void ToItemList(IFileSystem fileSystem, BuildEnvironment environment, List<ProjectItem> items)
		{
			throw new NotImplementedException();
		}

		public override string ToString()
		{
			return string.Format("%({0})", _metadata);
		}

		private bool Equals(MetadataReference other)
		{
			return string.Equals(_metadata, other._metadata);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is MetadataReference && Equals((MetadataReference) obj);
		}

		public override int GetHashCode()
		{
			return _metadata.GetHashCode();
		}
	}
}