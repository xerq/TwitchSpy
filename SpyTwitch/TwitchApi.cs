using System;
using System.Net;
using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.CSharp;

namespace TwitchSpy
{
	public class TwitchApi
	{
		public TwitchApi ()
		{
		}

		public static void getViewersOfChannel (string channelName, Action<List<string>> callback)
		{
			WebClient wc = new WebClient ();
			string apiLink = "https://tmi.twitch.tv/group/user/" + channelName + "/chatters";
			wc.DownloadStringCompleted += (object sender, DownloadStringCompletedEventArgs e) => {
				if (e.Error != null) {
					Console.WriteLine ("Error on loading " + apiLink);
					callback (new List<string> ());
				} else {
					string apiContentJson = e.Result;

					////JSON
					dynamic obj = JsonConvert.DeserializeObject (apiContentJson);
					var chatters = obj.chatters;
					var viewers = chatters.viewers;
					var moderators = chatters.moderators;
					var staff = chatters.staff;
					var admins = chatters.admins;

					List<string> ret = viewers.ToObject<List<string>> ();
					ret.AddRange (moderators.ToObject<List<string>> ());
					ret.AddRange (staff.ToObject<List<string>> ());
					ret.AddRange (admins.ToObject<List<string>> ());

					wc.Dispose ();
					callback (ret);
				}
			};
			wc.DownloadStringAsync (new Uri (apiLink));
		}

		public static void getFollowingCount (string channelName, Action<int> callback)
		{
			WebClient wc = new WebClient ();

			string countLink = "https://api.twitch.tv/kraken/users/" + channelName + "/follows/channels?direction=DESC&limit=1&offset=0";

			wc.DownloadStringCompleted += (object sender, DownloadStringCompletedEventArgs e) => {
				string countData = e.Result;

				dynamic countJson = JsonConvert.DeserializeObject (countData);
				int countOfFollows = Convert.ToInt32 (countJson ["_total"]);

				wc.Dispose ();
				callback (countOfFollows);
			};

			wc.DownloadStringAsync (new Uri (countLink));
		}

		public static void getFollowing (string channelName, Action<List<string>> callback)
		{
			////GET NUMBER OF FOLLOWS
			getFollowingCount (channelName, (int followsCount) => {
				int currentOffset = 0;

				for (currentOffset = 0; currentOffset < followsCount; currentOffset += 100) {
					string apiLink = "https://api.twitch.tv/kraken/users/" + channelName + "/follows/channels?direction=DESC&limit=100&offset=" + currentOffset + "&sortby=created_at";
					WebClient currentOffsetWC = new WebClient ();
					currentOffsetWC.DownloadStringCompleted += (object sender, DownloadStringCompletedEventArgs e) => {
						string apiContentJson = e.Result;


						////JSON
						dynamic obj = JsonConvert.DeserializeObject (apiContentJson);
						var follows = obj.follows;

						List<string> followsParsed = new List<string> ();
						foreach (var follow in follows) {
							var user = follow.channel;
							var userName = user.name;

							followsParsed.Add (userName.Value);
						}

						currentOffsetWC.Dispose ();
						callback (followsParsed);
					};
					currentOffsetWC.DownloadStringAsync (new Uri (apiLink));
				}
			});
		}

		[Obsolete]
		public static void getFollowingEasy (string channelName, Action<List<string>> callback)
		{
			WebClient wc = new WebClient ();
			int currentOffset = 0;
			string apiLink = "https://api.twitch.tv/kraken/users/" + channelName + "/follows/channels?direction=DESC&limit=100&offset=" + currentOffset + "&sortby=created_at";
			wc.DownloadStringCompleted += (object sender, DownloadStringCompletedEventArgs e) => {
				string apiContentJson = e.Result;
				List<string> followsParsed = new List<string> ();

				////JSON
				dynamic obj = JsonConvert.DeserializeObject (apiContentJson);
				var follows = obj.follows;
				foreach (var follow in follows) {
					var user = follow.channel;
					var userName = user.name;

					followsParsed.Add (userName.Value);
				}

				wc.Dispose ();
				callback (followsParsed);
			};
			wc.DownloadStringAsync (new Uri (apiLink));
		}

		public static void getTopChannels (int limit, Action<List<string>> callback)
		{
			for (int offset = 0; offset < limit; offset += 100) {
				WebClient wc = new WebClient ();

				wc.DownloadStringCompleted += (object sender, DownloadStringCompletedEventArgs e) => {
					string result = e.Result;

					dynamic resultJson = JsonConvert.DeserializeObject (result);

					var featuredChannels = resultJson.featured;
					List<string> channels = new List<string> ();
					foreach (var featuredChannel in featuredChannels) {
						var channel = featuredChannel.stream.channel;
						string channelName = channel.name;
						channels.Add (channelName);
					}

					wc.Dispose ();
					callback (channels);
				};


				wc.DownloadStringAsync (new Uri ("https://api.twitch.tv/kraken/streams/featured?limit=" + limit + "&offset=" + offset));
			}
		}
	}
}
