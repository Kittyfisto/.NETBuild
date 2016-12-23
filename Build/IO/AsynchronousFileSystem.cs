using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Build.DomainModel.MSBuild;

namespace Build.IO
{
	public sealed class AsynchronousFileSystem
		: IFileSystem
			, IDisposable
	{
		private readonly IFileSystem _fileSystem;
		private readonly ConcurrentQueue<ReadTask> _pendingReads;
		private readonly ConcurrentQueue<WriteTask> _pendingWrites;

		private readonly Thread _read;

		private readonly Thread _write;
		private bool _isDisposed;

		public AsynchronousFileSystem(IFileSystem fileSystem)
		{
			if (fileSystem == null)
				throw new ArgumentNullException(nameof(fileSystem));

			_fileSystem = fileSystem;
			_pendingWrites = new ConcurrentQueue<WriteTask>();
			_write = new Thread(Write);
			_write.Start();

			_pendingReads = new ConcurrentQueue<ReadTask>();
			_read = new Thread(Read);
			_read.Start();
		}

		public void Dispose()
		{
			_isDisposed = true;

			_read.Join();
			_write.Join();
		}

		public bool Exists(string filename)
		{
			throw new NotImplementedException();
		}

		public FileInfo GetFileInfo(string filename)
		{
			throw new NotImplementedException();
		}

		public void CopyFile(string sourceFileName, string destFileName, bool overwrite)
		{
			throw new NotImplementedException();
		}

		public void DeleteFile(string fileName)
		{
			throw new NotImplementedException();
		}

		public void CreateDirectory(string directoryPath)
		{
			throw new NotImplementedException();
		}

		public string ReadAllText(string fileName)
		{
			throw new NotImplementedException();
		}

		public Stream OpenWrite(string fileName)
		{
			return new OpenWriteStream(this, fileName);
		}

		public Stream OpenRead(string fileName)
		{
			var task = new ReadTask(fileName, new MemoryStream());
			return new OpenReadStream(task);
		}

		private void Read()
		{
			while (!_isDisposed)
			{
				ReadTask task;
				while (_pendingReads.TryDequeue(out task))
					Read(task);

				Thread.Sleep(TimeSpan.FromMilliseconds(10));
			}
		}

		private void Read(ReadTask task)
		{
			using (var stream = _fileSystem.OpenRead(task.FileName))
			{
				stream.CopyTo(task.Data);
				task.DataRead.Set();
			}
		}

		private void Write()
		{
			while (!_isDisposed)
			{
				WriteTask task;
				while (_pendingWrites.TryDequeue(out task))
					Write(task);

				Thread.Sleep(TimeSpan.FromMilliseconds(10));
			}
		}

		private void Write(WriteTask task)
		{
			using (var stream = _fileSystem.OpenWrite(task.FileName))
			{
				task.Data.CopyTo(stream);
			}
		}

		/// <summary>
		///     Queues a write task to write the given data to the file system.
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="data"></param>
		private void WriteAsync(string fileName, MemoryStream data)
		{
			var task = new WriteTask(fileName, data);
			_pendingWrites.Enqueue(task);
		}

		public string CurrentDirectory
		{
			get { return _fileSystem.CurrentDirectory; }
			set { _fileSystem.CurrentDirectory = value; }
		}

		public bool ExistsDirectory(string directory)
		{
			return _fileSystem.Exists(directory);
		}

		public void WriteAllText(string fileName, string text)
		{
			throw new NotImplementedException();
		}

		public void WriteAllBytes(string fileName, byte[] data)
		{
			throw new NotImplementedException();
		}

		public byte[] ReadAllBytes(string fileName)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<string> EnumerateFiles(string path)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<string> EnumerateFiles(string path, string searchPattern)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption)
		{
			throw new NotImplementedException();
		}

		private struct ReadTask
		{
			public readonly string FileName;
			public readonly MemoryStream Data;
			public readonly AutoResetEvent DataRead;

			public ReadTask(string fileName, MemoryStream data)
			{
				FileName = fileName;
				Data = data;
				DataRead = new AutoResetEvent(false);
			}
		}

		private struct WriteTask
		{
			public readonly string FileName;
			public readonly MemoryStream Data;

			public WriteTask(string fileName, MemoryStream data)
			{
				FileName = fileName;
				Data = data;
			}
		}

		private sealed class OpenReadStream
			: Stream
		{
			private readonly ReadTask _task;
			private long _position;
			private MemoryStream _stream;

			public OpenReadStream(ReadTask task)
			{
				_task = task;
			}

			public override bool CanRead
			{
				get { return true; }
			}

			public override bool CanSeek
			{
				get { return true; }
			}

			public override bool CanWrite
			{
				get { return false; }
			}

			public override long Length
			{
				get
				{
					Wait();
					return _stream.Length;
				}
			}

			public override long Position
			{
				get { return _position; }
				set
				{
					Wait();
					_position = value;
					_stream.Position = value;
				}
			}

			private void Wait()
			{
				if (_stream == null)
				{
					_task.DataRead.WaitOne();
					_task.DataRead.Dispose();
					_stream = _task.Data;
				}
			}

			public override void Flush()
			{
			}

			public override long Seek(long offset, SeekOrigin origin)
			{
				Wait();
				var ret = _stream.Seek(offset, origin);
				_position = _stream.Position;
				return ret;
			}

			public override void SetLength(long value)
			{
				throw new NotImplementedException();
			}

			public override int Read(byte[] buffer, int offset, int count)
			{
				Wait();
				return _stream.Read(buffer, offset, count);
			}

			public override void Write(byte[] buffer, int offset, int count)
			{
				throw new NotImplementedException();
			}
		}

		private sealed class OpenWriteStream
			: MemoryStream
		{
			private readonly AsynchronousFileSystem _cache;
			private readonly string _fileName;

			public OpenWriteStream(AsynchronousFileSystem cache, string fileName)
			{
				if (cache == null)
					throw new ArgumentNullException("cache");
				if (fileName == null)
					throw new ArgumentNullException("fileName");

				_cache = cache;
				_fileName = fileName;
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
					_cache.WriteAsync(_fileName, this);

				base.Dispose(disposing);
			}
		}
	}
}