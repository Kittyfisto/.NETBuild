using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Build.DomainModel;
using Build.DomainModel.MSBuild;
using Build.IO;
using log4net;

namespace Build.Watchdog
{
	/// <summary>
	///     Is responsible for watching over a directory tree on the filesystem and creating an in-memory-representation
	///     of its contents via <see cref="CurrentSandbox" />.
	/// </summary>
	public sealed class SandboxWatchdog
		: IDisposable
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly FileSystemWatcher _fileSystemWatcher;
		private readonly ConcurrentQueue<PendingAction> _pendingActions;
		private readonly string _rootFolder;
		private readonly Thread _thread;
		private Sandbox _currentSandbox;
		private bool _isDisposed;
		private readonly IFileSystem _filesystem;

		public SandboxWatchdog(string rootFolder)
		{
			if (rootFolder == null)
				throw new ArgumentNullException("rootFolder");

			_filesystem = new FileSystem();
			_rootFolder = rootFolder;
			_pendingActions = new ConcurrentQueue<PendingAction>();

			_currentSandbox = new Sandbox(Enumerable.Empty<Solution>(), Enumerable.Empty<Project>());

			_thread = new Thread(ExecutePendingActions)
				{
					IsBackground = true
				};
			_fileSystemWatcher = new FileSystemWatcher(_rootFolder)
				{
					IncludeSubdirectories = true,
					NotifyFilter = NotifyFilters.LastWrite |
					               NotifyFilters.FileName |
					               NotifyFilters.CreationTime |
					               NotifyFilters.Size,
					Filter = "*.csproj"
				};
			_fileSystemWatcher.Created += OnCreated;
			_fileSystemWatcher.Deleted += OnDeleted;
			_fileSystemWatcher.Changed += OnChanged;
			_fileSystemWatcher.Renamed += OnRenamed;
			_fileSystemWatcher.Error += OnError;
			_fileSystemWatcher.EnableRaisingEvents = true;

			_pendingActions.Enqueue(PendingAction.Reload(_rootFolder));
			_thread.Start();
		}

		#region FileSystemWatcher events

		private void OnError(object sender, ErrorEventArgs e)
		{
			Log.ErrorFormat("Cauhgt unexpected exception: {0}",
			                e.GetException());
		}

		private void OnCreated(object sender, FileSystemEventArgs e)
		{
			_pendingActions.Enqueue(PendingAction.CreateOrUpdate(e.Name));
		}

		private void OnDeleted(object sender, FileSystemEventArgs e)
		{
			_pendingActions.Enqueue(PendingAction.Delete(e.Name));
		}

		private void OnChanged(object sender, FileSystemEventArgs e)
		{
			_pendingActions.Enqueue(PendingAction.CreateOrUpdate(e.Name));
		}

		private void OnRenamed(object sender, RenamedEventArgs e)
		{
			_pendingActions.Enqueue(PendingAction.CreateOrUpdate(e.Name));
		}

		#endregion

		#region Execution

		public Sandbox CurrentSandbox
		{
			get { return _currentSandbox; }
		}

		private void ExecutePendingActions()
		{
			var sandboxLoader = new SandboxLoader(_filesystem);
			while (!_isDisposed)
			{
				PendingAction action;
				while (_pendingActions.TryDequeue(out action))
				{
					Execute(sandboxLoader, action);
				}

				// TODO: We should probably add a delay 

				_currentSandbox = sandboxLoader.CreateSandbox();

				Thread.Sleep(TimeSpan.FromMilliseconds(100));
			}
		}

		private void Execute(SandboxLoader loader, PendingAction action)
		{
			try
			{
				switch (action.Type)
				{
					case PendingActionType.CreateOrUpdate:
						loader.CreateOrUpdate(action.Path);
						break;
					case PendingActionType.Remove:
						loader.Delete(action.Path);
						break;
					case PendingActionType.Reload:
						foreach (string file in Directory.EnumerateFiles(action.Path, "*.csproj", SearchOption.AllDirectories))
						{
							_pendingActions.Enqueue(PendingAction.CreateOrUpdate(file));
						}
						break;
				}
			}
			catch (Exception e)
			{
				// We should retry operations for certain failures because
				// changes are the operation will succeed the next time.
				// Reasons are:
				// - The file is currently owned by another process
				// - The file is being modified and not yet ready to be consumed
				// - Some other related race condition occured that will likely be remedied in the future
				// Algorithm:
				// Exponential backoff with a fixed upper limit of tries.
				// TODO: Implement!

				Log.ErrorFormat("Caught unexpected exception while executing '{0}': {1}",
				                action,
				                e);
			}
		}

		#endregion

		public void Dispose()
		{
			_fileSystemWatcher.Dispose();
			_isDisposed = true;
		}
	}
}