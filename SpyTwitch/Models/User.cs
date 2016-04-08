using System;

namespace TwitchSpy
{
	public class User
	{
		public string name;
		public List<WatchLog> watchingList;
		public List<string> follows;

		public User (string name)
		{
			this.name = name;
			watchingList = new List<WatchLog> ();
			follows = new List<string> ();
		}
	}
}

