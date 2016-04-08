using System;
using MongoDB.Driver;
using MongoDB.Bson;

namespace TwitchSpy
{
	public class manageDb
	{
		protected static IMongoClient client;
		protected static IMongoDatabase database;

		public manageDb (string connectionString, string dbName)
		{
			client = new MongoClient (connectionString);
			database = client.GetDatabase (dbName);
		}

		public void insertSpyInfo(User watchedUser, string channel, DateTime when){
			BsonDocument document = new BsonDocument{
				{"text", string.Format("{0} is watching {1} on {2}", watchedUser, channel, DateTime.Now)}
			};


			var collection = database.GetCollection<BsonDocument> ("logs");
			collection.InsertOneAsync (document);
		}
	}
}

