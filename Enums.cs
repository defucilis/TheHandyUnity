namespace Defucilis.TheHandyUnity
{
	public enum HandyStatus
	{
		Off = 0,
		Automatic = 1,
		Position = 2,
		Calibration = 3,
		Sync = 4
	}

	public enum HandyLogMode
	{
		None = 0,
		Errors = 1,
		Responses = 2,
		Verbose = 3
	}

	public struct HandyStatusData
	{
		public HandyStatus Status;
		public float CurrentPosition;
		public float Speed;
		public float Stroke;
	}

	public struct HandySpatialData
	{
		public float PercentageValue;
		public float RawValue;
	}

	public struct HandyVersionData
	{
		public string CurrentVersion;
		public string LatestVersion;
	}

	public struct HandyPlayingData
	{
		public bool Playing;
		public int SetOffset;
	}
}