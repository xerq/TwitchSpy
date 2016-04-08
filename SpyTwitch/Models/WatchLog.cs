using System;

namespace TwitchSpy
{
	public struct WatchLog
	{
		public string channelName;
		public DateTime when;

		public WatchLog (string channelName, DateTime when)
		{
			this.channelName = channelName;
			this.when = when;
		}
	}
}

