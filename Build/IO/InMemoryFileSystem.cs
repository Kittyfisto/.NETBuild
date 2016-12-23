using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Build.BuildEngine;

namespace Build.IO
{
	public sealed class InMemoryFileSystem
		: IFileSystem
	{
		private readonly HashSet<string> _directories;
		private readonly Dictionary<string, MemoryStream> _files;
		private readonly object _syncRoot;
		private string _currentDirectory;

		public InMemoryFileSystem()
		{
			var comparer = new FilenameComparer();
			_files = new Dictionary<string, MemoryStream>(comparer);
			_directories = new HashSet<string>(comparer);
			_syncRoot = new object();

			var root = "A:\\";
			Mount(root);
			_currentDirectory = root;
		}
		
		public IReadOnlyDictionary<string, string> Files
		{
			get { return _files.Select(x => new KeyValuePair<string, string>(x.Key, Read(x.Value)))
					.ToDictionary(x => x.Key, x => x.Value); }
		}

		private static string Read(MemoryStream stream)
		{
			var tmp = new MemoryStream();
			stream.Position = 0;
			stream.CopyTo(tmp);
			tmp.Position = 0;

			using (var reader = new StreamReader(tmp))
			{
				return reader.ReadToEnd();
			}
		}

		public void Mount(string rootName)
		{
			lock (_syncRoot)
			{
				_directories.Add(rootName);
			}
		}

		[Pure]
		public bool Exists(string filename)
		{
			lock (_syncRoot)
			{
				filename = Normalize(filename);
				return _files.ContainsKey(filename);
			}
		}

		[Pure]
		public bool ExistsDirectory(string directory)
		{
			lock (_syncRoot)
			{
				directory = Normalize(directory);
				return _directories.Contains(directory);
			}
		}

		public FileInfo GetFileInfo(string filename)
		{
			lock (_syncRoot)
			{
				filename = Normalize(filename);

				MemoryStream data;
				if (!_files.TryGetValue(filename, out data))
				{
					return new FileInfo(false, 0, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue);
				}

				return new FileInfo();
			}
		}

		public void CopyFile(string sourceFileName, string destFileName, bool overwrite)
		{
			lock (_syncRoot)
			{
				sourceFileName = Normalize(sourceFileName);
				destFileName = Normalize(destFileName);

				var data = OpenRead(sourceFileName);

				if (!overwrite && Exists(destFileName))
					throw new Exception();

				using (var stream = OpenWrite(destFileName))
				{
					data.CopyTo(stream);
				}
			}
		}

		public void DeleteFile(string fileName)
		{
			lock (_syncRoot)
			{
				fileName = Normalize(fileName);

				_files.Remove(fileName);
			}
		}

		public void CreateDirectory(string directoryPath)
		{
			lock (_syncRoot)
			{
				directoryPath = Normalize(directoryPath);

				var root = Path.GetRootDir(directoryPath);
				if (!_directories.Contains(root))
					throw new DirectoryNotFoundException(string.Format("Could not find a part of the path '{0}'.", root));

				_directories.Add(directoryPath);
			}
		}

		public string ReadAllText(string fileName)
		{
			fileName = Normalize(fileName);

			using (var stream = OpenRead(fileName))
			using (var reader = new StreamReader(stream))
			{
				return reader.ReadToEnd();
			}
		}

		sealed class ProxyStream
			: Stream
		{
			private readonly MemoryStream _stream;

			public ProxyStream(MemoryStream data)
			{
				_stream = data;
			}

			public override void Flush()
			{
				
			}

			public override long Seek(long offset, SeekOrigin origin)
			{
				switch (origin)
				{
					case SeekOrigin.Begin:
						Position = offset;
						break;
					case SeekOrigin.Current:
						Position += offset;
						break;
					case SeekOrigin.End:
						Position -= offset;
						break;
				}

				return Position;
			}

			public override void SetLength(long value)
			{
				_stream.SetLength(value);
			}

			public override int Read(byte[] buffer, int offset, int count)
			{
				throw new Exception();
			}

			public override void Write(byte[] buffer, int offset, int count)
			{
				_stream.Position = Position;
				_stream.Write(buffer, offset, count);
				Position += count;
			}

			public override bool CanRead
			{
				get { return false; }
			}

			public override bool CanSeek
			{
				get { return true; }
			}

			public override bool CanWrite
			{
				get { return true; }
			}

			public override long Length
			{
				get { return _stream.Length; }
			}

			public override long Position { get; set; }
		}

		public Stream OpenWrite(string fileName)
		{
			lock (_syncRoot)
			{
				fileName = Normalize(fileName);
				var directory = Path.GetDirectory(fileName);
				if (!_directories.Contains(directory))
					throw new DirectoryNotFoundException(string.Format("Could not find a part of the path '{0}'.", fileName));

				var data = new MemoryStream();
				_files[fileName] = data;
				return new ProxyStream(data);
			}
		}

		public Stream OpenRead(string fileName)
		{
			lock (_syncRoot)
			{
				fileName = Normalize(fileName);

				MemoryStream data;
				if (!_files.TryGetValue(fileName, out data))
					throw new DirectoryNotFoundException(string.Format("Could not find a part of the path '{0}'.", fileName));

				var readStream = new MemoryStream();
				data.Position = 0;
				data.CopyTo(readStream);
				readStream.Position = 0;
				return readStream;
			}
		}

		public string CurrentDirectory
		{
			get { return _currentDirectory; }
			set
			{
				if (value == null)
					throw new ArgumentNullException();

				if (!Path.IsPathRooted(value))
					throw new ArgumentException();

				lock (_syncRoot)
				{
					_currentDirectory = value;
				}
			}
		}

		[Pure]
		private string Normalize(string filename)
		{
			return Path.MakeAbsolute(_currentDirectory, filename);
		}

		public void WriteAllText(string fileName, string text)
		{
			using (var stream = OpenWrite(fileName))
			using(var writer = new StreamWriter(stream))
			{
				writer.Write(text);
			}
		}

		public void WriteAllBytes(string fileName, byte[] data)
		{
			using (var stream = OpenWrite(fileName))
			{
				stream.Write(data, 0, data.Length);
			}
		}

		public byte[] ReadAllBytes(string fileName)
		{
			using (var stream = OpenRead(fileName))
			{
				return stream.ReadToEnd();
			}
		}

		public IEnumerable<string> EnumerateFiles(string path)
		{
			return EnumerateFiles(path, "*");
		}

		public IEnumerable<string> EnumerateFiles(string path, string searchPattern)
		{
			return EnumerateFiles(path, searchPattern, SearchOption.TopDirectoryOnly);
		}

		public IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption)
		{
			path = Normalize(path);
			var ret = new List<string>();
			var regexPattern = "^" + Regex.Escape(searchPattern)
				                   .Replace(@"\*", ".*")
				                   .Replace(@"\?", ".")
			                   + "$";
			var regex = new Regex(regexPattern);

			foreach (var pair in _files)
			{
				if (Matches(pair.Key, path, regex, searchOption))
				{
					ret.Add(pair.Key);
				}
			}

			return ret;
		}

		[Pure]
		private static bool Matches(string filePath, string path, Regex regex, SearchOption searchOption)
		{
			var directory = Path.GetDirectory(filePath);
			if (!directory.StartsWith(path))
				return false;

			if (searchOption == SearchOption.TopDirectoryOnly &&
			    path.Length > directory.Length)
				return false;

			var fileName = Path.GetFilename(filePath);
			var match = regex.Match(fileName);
			if (match.Success)
				return true;

			return false;
		}
	}
}