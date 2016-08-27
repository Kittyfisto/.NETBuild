using System;

namespace Build.DomainModel.MSBuild
{
	public sealed class Compile
	{
		private readonly string _includePath;

		public Compile(string includePath)
		{
			if (includePath == null)
				throw new ArgumentNullException("includePath");

			_includePath = includePath;
		}

		public string IncludePath
		{
			get { return _includePath; }
		}

		public override string ToString()
		{
			return _includePath;
		}
	}
}