using System;
using FehBot;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using MongoDB.Driver;
using IrcDotNet.Collections;
using MongoDB.Bson;

namespace FehBot.Handlers
{
	public class KarmaHandler : IHandler
	{
	    private readonly Regex karmaRe = new Regex(@"(\w+)([+]{2}|[-]{2})");

		public KarmaHandler ()
		{

		}

		private bool isKarmaRequest (string message)
		{
			return karmaRe.IsMatch (message);
		}

		private Dictionary<string, int> parseKarmaRequest (string message)
		{
			MatchCollection found = karmaRe.Matches(message);
			var result = new Dictionary<string, int>();
			if (found.Count > 0) {
				for(var i = 0; i < found.Count; i++) {
					var karmaReqStr = found[i];
					GroupCollection match = karmaReqStr.Groups;
					string user = match[1].Value.ToLower();
					int dir = match[2].Value.Equals("++") ? 1 : -1;
					result.Add(user, dir );
				}
			}
			return result;

		}

		public void handle (RegistrationInfoFactory infoFactory, IrcDotNet.IrcClient client, FehBot bot, IMongoDatabase db, IrcDotNet.IrcUser from, IrcDotNet.IrcChannel to, string message)
		{
			Console.WriteLine (message);
			if (to == null) 
			{
				return;
			}

			if (isKarmaRequest(message))
			{
				var request = parseKarmaRequest (message);
				request.Keys.ForEach (key => {
					var karma = db.GetCollection<BsonDocument>("Karma");
					var builder = Builders<BsonDocument>.Filter;
					var filter = builder.Eq("nick", key) & builder.Eq("channel", to.Name.ToLower()) & builder.Eq("network", infoFactory.Server);
					var document = karma.Find(filter).FirstOrDefault();
					if (document == null) {
						karma.InsertOne(new BsonDocument{
							{"nick", key},
							{"channel", to.Name.ToLower()},
							{"network", infoFactory.Server},
							{"score", 0}
						});
						document = karma.Find(filter).FirstOrDefault();
					}

					var update = Builders<BsonDocument>.Update.Inc("score", request[key]);
					karma.UpdateOne(filter, update);
					string template = request[key] > 0 ? "{0} gained a level! (Karma: {1})":"{0} lost a level! (Karma: {1})";
					client.LocalUser.SendMessage(to, String.Format(template, key, document.GetValue("score").AsInt32 + request[key]));
				});

			}
		}
	}
}

