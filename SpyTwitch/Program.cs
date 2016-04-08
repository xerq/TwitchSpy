using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

//MONGODB
using MongoDB.Bson;

namespace TwitchSpy
{
	class MainClass
	{
		static List<string> watchingChannels;
		static List<User> watchedUsers;
		static List<string> topChannels;
		static List<string> customChannels;

		static ManageDb dbmanager;

		public static void Main (string[] args)
		{
			if (Config.useDb)
				dbmanager = new ManageDb (Config.mongodbConnectionString, Config.mongoDatabaseName);

			watchingChannels = new List<string> ();
			topChannels = new List<string> ();
			watchedUsers = new List<User> ();

			TwitchApi.GetTopChannels (Config.topChannelsLimit, (channels) => {
				channels.ForEach (channel => topChannels.Add (channel));
			});

			Console.WriteLine ("Enter user's name you wanna watch");
			string userName = Console.ReadLine ();
			User currentUser = new User (userName);
			watchedUsers.Add (currentUser);


			TwitchApi.GetFollowing (userName, (List<string> follows) => {
				
				follows.ForEach (follow => {
					Console.WriteLine (follow);
					currentUser.follows.Add (follow);
				});

			});

			Task.Run (async delegate {
				while (true) {
					bool isDone = false;
					WatchUsers (watchedUsers, (status) => {
						isDone = true;
					});
					while (!isDone) {
						await Task.Delay (10);
					}
				}

			});

			string[] commands = new string[] {
				"info",
				"addChannels",
				"removeChannels",
				"customChannels",
				"totalChannels",
				"help"
			};

			string command;
			while ((command = Console.ReadLine ()) != "exit") {
				if (command == "info") {
					currentUser.watchingList.ForEach (wd => {
						Console.WriteLine ("{0} was watching {1} on {2}", currentUser.name, wd.channelName, wd.when);
					});
				} else if (command == "addChannels") {
					Console.WriteLine ("Enter channels you want the program to watch. Format: channelname1,channelname2,channelname3 etc.");
					List<string> channelsToAdd = new List<string> (Console.ReadLine ().Split (','));
					if (customChannels == null)
						customChannels = new List<string> ();
					customChannels.AddRange (channelsToAdd);
				} else if (command == "removeChannels") {
					if (customChannels == null || customChannels.Count == 0) {
						Console.WriteLine ("You haven't added any custom channels yet");
						continue;
					}
					Console.WriteLine ("Enter channels you wanna remove. Format: channelname1,channelname2,channelname3 etc.");
					List<string> channelsToRemove = new List<string> (Console.ReadLine ().Split (','));

					channelsToRemove.ForEach (channel => customChannels.Remove (channel));
				} else if (command == "customChannels") {
					if (customChannels == null || customChannels.Count == 0) {
						Console.WriteLine ("You haven't added any custom channels yet");
						continue;
					}
					customChannels.ForEach (customChannel => Console.WriteLine (customChannel));
				} else if (command == "totalChannels") {
					HashSet<string> channels = new HashSet<string> ();

					topChannels.ForEach (topChannel => channels.Add (topChannel));

					watchedUsers.ForEach (user => {
						user.follows.ForEach (follow => channels.Add (follow));
					});

					if (customChannels != null)
						customChannels.ForEach (customChannel => channels.Add (customChannel));

					List<string> channelsList = channels.ToList ();

					channelsList.ForEach (channel => Console.WriteLine (channel));
				} else if (command == "help") {
					Console.WriteLine ("Available commands: {0}", string.Join (", ", commands));
				}
			}
		}

		public static void WatchUsers (List<User> watchedUsers, Action<bool> statusCallback)
		{
			HashSet<string> channels = new HashSet<string> ();

			topChannels.ForEach (topChannel => channels.Add (topChannel));

			watchedUsers.ForEach (user => {
				user.follows.ForEach (follow => channels.Add (follow));
			});

			if (customChannels != null)
				customChannels.ForEach (customChannel => channels.Add (customChannel));

			List<string> channelsList = channels.ToList ();

			int done = 0;

			channelsList.ForEach (channel => {
				TwitchApi.GetViewersOfChannel (channel, (viewers) => {
					watchedUsers.ForEach (watchedUser => {
						int count = viewers.Count (viewer => viewer == watchedUser.name);
						if (count != 0) {
							watchedUser.watchingList.Add (new WatchLog (channel, DateTime.Now));
							Console.WriteLine ("{0} is watching {1} on {2}", watchedUser.name, channel, DateTime.Now);

							if (dbmanager != null)
								dbmanager.InsertSpyInfo (watchedUser, channel, DateTime.Now);
						}
					});

					done++;
					if (done == channelsList.Count - 1) {
						statusCallback (true);
					}
				});
			});
		}
	}
}
