﻿using System;
using MongoDB.Driver;
using MongoDB.Bson;

namespace TwitchSpy
{
	public class ManageDb
	{
		protected static IMongoClient client;
		protected static IMongoDatabase database;

		public ManageDb (string connectionString, string dbName)
		{
			client = new MongoClient (connectionString);
			database = client.GetDatabase (dbName);
		}

		public void InsertSpyInfo (User watchedUser, string channel, DateTime when)
		{
			BsonDocument document = new BsonDocument {
				{ "text", string.Format ("{0} was watching {1} on {2}", watchedUser, channel, DateTime.Now) }
			};


			var collection = database.GetCollection<BsonDocument> ("logs");
			collection.InsertOneAsync (document);
		}
	}
}

