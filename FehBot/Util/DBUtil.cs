using System;
using FehBot.Vo;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Collections.Generic;
using MongoDB.Bson.IO;

namespace FehBot.DBUtil
{
	public static class DBUtil
	{

		public static string LookupAccountKey(){
			return "";
		}

		public static DeleteResult DeleteWebHooks (this IMongoDatabase db, WebHook webHook)
		{
			var hooks = db.GetCollection<BsonDocument>("Hooks");
			var builder = Builders<BsonDocument>.Filter;
			var filter = builder.Eq ("apiKey", webHook.ApiKey) & builder.Eq ("action", webHook.Action) & builder.Eq ("callbackUrl", webHook.CallbackUrl) & builder.Eq ("secret", webHook.Secret);

			return hooks.DeleteMany (filter);

		}

		public static List<WebHook> GetWebHookForAction(this IMongoDatabase db, String action) {
			var hooks = db.GetCollection<BsonDocument>("Hooks");
			var builder = Builders<BsonDocument>.Filter;
			var filter = builder.Eq ("action", action);

			var toReturn = new List<WebHook> ();

			var documents = hooks.Find (filter).ToList();
			foreach (var doc in documents) {
				toReturn.Add (WebHook.FromJson (doc.ToJson (new JsonWriterSettings { OutputMode = JsonOutputMode.Strict })));
			}
			return toReturn;
		}

		public static void CreateWebHook(this IMongoDatabase db, WebHook webHook) {
			
			var hooks = db.GetCollection<BsonDocument>("Hooks");
			var builder = Builders<BsonDocument>.Filter;
			var filter = builder.Eq ("apiKey", webHook.ApiKey) & builder.Eq ("action", webHook.Action) & builder.Eq ("callbackUrl", webHook.CallbackUrl);

			var document = hooks.Find(filter).FirstOrDefault();
			if (document == null) {
				hooks.InsertOne (webHook.ToBSON());
			} else {
				throw new DuplicateObjectException ();
			}
		}

		public static void SaveLinkRequest (this IMongoDatabase db, NickNameLink link)
		{
			var links = db.GetCollection<BsonDocument>("Links");
			var builder = Builders<BsonDocument>.Filter;
			var filter = builder.Eq ("nick", link.Nick) & builder.Eq ("remoteUserName", link.RemoteUserName);

			var document = links.Find(filter).FirstOrDefault();
			if (document == null) {
				links.InsertOne (link.ToBSON());

			} else {
				var update = Builders<BsonDocument>.Update.Set("code", link.Code);
				links.UpdateOne(filter, update);
			}
		}

		public static NickNameLink UpdatdeLinkRequestToEventUser(this IMongoDatabase db, NickNameLink link) {
			var remoteUserName = link.RemoteUserName;
			var nick = link.Nick;

			var links = db.GetCollection<BsonDocument>("Links");
			var eventUsers = db.GetCollection<BsonDocument>("EventUsers");

			var builder = Builders<BsonDocument>.Filter;
			var filter = builder.Eq ("nick", nick);
			links.DeleteMany (filter);

			filter = builder.Eq ("nick", nick) & builder.Eq ("remoteUserName", remoteUserName);
			var document = eventUsers.Find(filter).FirstOrDefault();

			if (document == null) {
				eventUsers.InsertOne (new BsonDocument {
					{ "nick", nick },
					{ "remoteUserName", remoteUserName }
				});
			} 

			link.Code = null;
			return link;

		}

		public static NickNameLink GetNickNameRequest(this IMongoDatabase db, string nick, string code) {
			var links = db.GetCollection<BsonDocument>("Links");
			var builder = Builders<BsonDocument>.Filter;
			var filter = builder.Eq ("nick", nick) & builder.Eq ("code", code);

			BsonDocument document = links.Find(filter).FirstOrDefault();
			return NickNameLink.FromJson(document.ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.Strict }));
		}

		public static List<NickNameLink> GetNickNameLink(this IMongoDatabase db, string nick) {
			var links = db.GetCollection<BsonDocument>("EventUsers");
			var builder = Builders<BsonDocument>.Filter;
			var filter = builder.Eq ("nick", nick);

			var documents = links.Find(filter).ToList();

			var toReturn = new List<NickNameLink> ();
			documents.ForEach(doc =>{toReturn.Add(NickNameLink.FromJson(doc.ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.Strict })));});
			return toReturn;
		}


		public static Karma UpdateKarma(this IMongoDatabase db, string nick, string channel, string server, int incr) {
			var karma = db.GetCollection<BsonDocument>("Karma");
			var builder = Builders<BsonDocument>.Filter;
			var filter = builder.Eq("nick", nick) & builder.Eq("channel", channel.ToLower()) & builder.Eq("network", server);
			var document = karma.Find(filter).FirstOrDefault();
			if (document == null) {
				karma.InsertOne(new BsonDocument{
					{"nick", nick},
					{"channel", channel.ToLower()},
					{"network", server},
					{"score", 0}
				});
				document = karma.Find(filter).FirstOrDefault();
			}

			var update = Builders<BsonDocument>.Update.Inc("score", incr);
			karma.UpdateOne(filter, update);
			var toReturn = Karma.FromJson (document.ToJson (new JsonWriterSettings { OutputMode = JsonOutputMode.Strict }));
			toReturn.Score += incr;
			return toReturn;

		}

	}
}

