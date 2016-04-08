using System;

namespace TwitchSpy
{
	public class Config
	{
		public static readonly bool useDb = false;
		public static readonly string mongodbConnectionString = "mongodb://localhost:27017/";
		public static readonly string mongoDatabaseName = "twitchspy";
		public static int topChannelsLimit = 50;

		static Config ()
		{
			
		}
	}
}

