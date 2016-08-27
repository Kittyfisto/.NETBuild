using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Build.DomainModel;
using Build.Parser;
using log4net;

namespace Build.Watchdog
{
	public class FileStore<T>
		: IFileStore
		where T : class, IFile
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly Dictionary<string, T> _filesByPath;
		private readonly IFileParser<T> _parser;

		public FileStore(IFileParser<T> parser)
		{
			if (parser == null)
				throw new ArgumentNullException("parser");

			_filesByPath = new Dictionary<string, T>();
			_parser = parser;
		}

		public int Count
		{
			get { return _filesByPath.Count; }
		}

		public IEnumerable<T> Values
		{
			get { return _filesByPath.Values; }
		}

		public void CreateOrUpdate(string filename)
		{
			string normalizedFilename = Path.Normalize(filename);
			string lowercaseFilename = normalizedFilename.ToLower();

			DateTime lastModified = File.GetLastWriteTime(normalizedFilename);
			T file;
			if (_filesByPath.TryGetValue(lowercaseFilename, out file))
			{
				if (file.LastModified >= lastModified)
				{
					Log.DebugFormat("Latest file version from '{0}' is already loaded, skipping...", normalizedFilename);
					return;
				}
			}

			file = _parser.Parse(filename);
			if (file == null)
				return;

			_filesByPath[lowercaseFilename] = file;
		}

		public void Remove(string filename)
		{
			string normalizedFilename = Path.Normalize(filename);
			string lowercaseFilename = normalizedFilename.ToLower();

			_filesByPath.Remove(lowercaseFilename);
		}

	}
}