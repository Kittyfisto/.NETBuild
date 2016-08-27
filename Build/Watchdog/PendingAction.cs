using System;

namespace Build.Watchdog
{
	public struct PendingAction
	{
		public readonly PendingActionType Type;
		public readonly string Path;

		public static PendingAction CreateOrUpdate(string filename)
		{
			return new PendingAction(PendingActionType.CreateOrUpdate, filename);
		}

		public static PendingAction Delete(string filename)
		{
			return new PendingAction(PendingActionType.Remove, filename);
		}

		public static PendingAction Reload(string rootFolder)
		{
			return new PendingAction(PendingActionType.Reload, rootFolder);
		}

		private PendingAction(PendingActionType type, string path)
		{
			if (path == null)
				throw new ArgumentNullException("path");

			Type = type;
			Path = path;
		}

		public override string ToString()
		{
			return string.Format("{0}: {1}", Type, Path);
		}
	}
}