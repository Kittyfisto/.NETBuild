namespace Build.DomainModel.MSBuild
{
	public sealed class Copy
		: Task
	{
		/// <summary>
		///     Optional Boolean parameter.
		///     Overwrite files even if they are marked as read only files
		/// </summary>
		public bool OverwriteReadOnlyFiles { get; set; }

		/// <summary>
		///     Optional Int32 parameter.
		///     Specifies how many times to attempt to copy, if all previous attempts have failed. Defaults to zero.
		/// </summary>
		/// <remarks>
		///     The use of retries can mask a synchronization problem in your build process.
		/// </remarks>
		public int Retries { get; set; }

		/// <summary>
		///     Specifies the delay between any necessary retries. Defaults to the RetryDelayMillisecondsDefault argument,
		///     which is passed to the CopyTask constructor.
		/// </summary>
		public int RetryDelayMilliseconds { get; set; }

		/// <summary>
		///     Contains the items that were successfully copied.
		/// </summary>
		public string CopiedFiles { get; set; }

		/// <summary>
		///     Specifies the list of files to copy the source files to.
		///     This list is expected to be a one-to-one mapping with the list specified in the SourceFiles parameter.
		///     That is, the first file specified in SourceFiles will be copied to the first location specified in DestinationFiles, and so forth.
		/// </summary>
		public string DestinationFiles { get; set; }

		/// <summary>
		///     Specifies the files to copy.
		/// </summary>
		public string SourceFiles { get; set; }
	}
}