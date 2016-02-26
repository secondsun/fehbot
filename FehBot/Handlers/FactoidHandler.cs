using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FehBot.Handlers
{
	public class FactoidHandler  : IHandler
	{

		private readonly Regex FactoidStoreExpression = new Regex(@"^(\w+) is (.+)$");
		private readonly Regex FactoidRemoveExpression = new Regex(@"^forget (\w+)");
		private readonly Regex FactoidGetExpression = new Regex(@"^\?(\w+)$");
		private readonly Regex FactoidMentionExpression = new Regex(@"^\?(\w+)(\s*)@(\s*)(.+)$");

		public FactoidHandler ()
		{
		}

		public void handle (RegistrationInfoFactory infoFactory, IrcDotNet.IrcClient client, FehBot bot, MongoDB.Driver.IMongoDatabase db, IrcDotNet.IrcUser from, IrcDotNet.IrcChannel to, string message)
		{
			if (isFactoidMentionRequest(message)) {
				Tuple<string, string> request = ParseFactionMention (message);
				var term = request.Item1.Trim();
				var target = request.Item2.Trim();

				var factoid = db.GetCollection<BsonDocument>("factoid");
				var builder = Builders<BsonDocument>.Filter;
				var filter = builder.Eq("term", term) & builder.Eq("channel", to.Name.ToLower()) & builder.Eq("network", infoFactory.Server);

				var document = factoid.Find(filter).FirstOrDefault();

				if (document == null) {
					string template = "{0}, Huh?";
					client.LocalUser.SendMessage(to, String.Format(template, from.NickName));	
				} else {
					string template = "{0}, {1}.";
					client.LocalUser.SendMessage(to, String.Format(template, target,  document.GetValue("meaning").AsString));
				}

			} else if (isFactoidGetRequest(message)) {
				var term = parseFactoidGet (message);
				var factoid = db.GetCollection<BsonDocument>("factoid");
				var builder = Builders<BsonDocument>.Filter;
				var filter = builder.Eq("term", term) & builder.Eq("channel", to.Name.ToLower()) & builder.Eq("network", infoFactory.Server);

				var document = factoid.Find(filter).FirstOrDefault();

				if (document == null) {
					string template = "{0}, Huh?";
					client.LocalUser.SendMessage(to, String.Format(template, from.NickName));

				} else {
					string template = "{0}, {1}";
					client.LocalUser.SendMessage(to, String.Format(template, from.NickName, document.GetValue("meaning").AsString));
				}

			} else if (isFactoidStoreRequest(message, infoFactory.NickName)) {
				message = removeMention (message, infoFactory.NickName);
				var request = parseFactoidStoreRequest (message);
				var term = request.Item1;
				var definition = request.Item2;

				var factoid = db.GetCollection<BsonDocument>("factoid");
				var builder = Builders<BsonDocument>.Filter;
				var filter = builder.Eq("term", term) & builder.Eq("channel", to.Name.ToLower()) & builder.Eq("network", infoFactory.Server);

				var document = factoid.Find(filter).FirstOrDefault();
				if (document == null) {
					factoid.InsertOne (new BsonDocument {
						{ "term", term },
						{ "channel", to.Name.ToLower () },
						{ "network", infoFactory.Server },
						{ "meaning", definition }
					});

				} else {
					var update = Builders<BsonDocument>.Update.Set("meaning", definition);
					factoid.UpdateOne(filter, update);
				}
				string template = "{0}, Got it!";
				client.LocalUser.SendMessage(to, String.Format(template, from.NickName));


			} else if (isFactoidRemoveRequest(message, infoFactory.NickName)) {
				message = removeMention (message, infoFactory.NickName);
				string term = parseFactoidRemoveRequest (message);

				var factoid = db.GetCollection<BsonDocument>("factoid");
				var builder = Builders<BsonDocument>.Filter;
				var filter = builder.Eq("term", term) & builder.Eq("channel", to.Name.ToLower()) & builder.Eq("network", infoFactory.Server);

				var document = factoid.Find(filter).FirstOrDefault();
				if (document == null) {
					string template = "{0}, Huh?";
					client.LocalUser.SendMessage(to, String.Format(template, from.NickName));	
				} else {
					factoid.DeleteOne(filter);
					string template = "{0}, It never happened.";
					client.LocalUser.SendMessage(to, String.Format(template, from.NickName));
				}

			} 
		}

		string removeMention (string message, string nickName)
		{
			return message.Substring (message.IndexOf (nickName) + nickName.Length + 1).Trim ();//remove mention
		}

		bool addressedToMe (string message, string nick)
		{
			return message.IndexOf(nick) == 0;
		}

		private bool isFactoidStoreRequest(string message, string nick) {
			string noMention = removeMention (message, nick);
			return FactoidStoreExpression.IsMatch (noMention) && addressedToMe(message, nick);
		}

		private bool isFactoidRemoveRequest(string message, string nick) {
			string noMention = removeMention (message, nick);
			return FactoidRemoveExpression.IsMatch (noMention) && addressedToMe(message, nick);
		}
		private bool isFactoidGetRequest(string message) {
			return FactoidGetExpression.IsMatch (message);
		}
		private bool isFactoidMentionRequest(string message) {
			return FactoidMentionExpression.IsMatch (message);
		}

		Tuple<string, string> ParseFactionMention (string message)
		{
			var matchData = message.Split('@');
			return new Tuple<string, string>( matchData[0].ToString().Split('?')[1], matchData[1].ToString() );
		}

		private Tuple<string, string> parseFactoidStoreRequest(string message) {
			var request = FactoidStoreExpression.Match (message).Groups;
			return new Tuple<string, string>( request[1].ToString(), request[2].ToString() );
		}

		string parseFactoidRemoveRequest (string message)
		{
			return FactoidRemoveExpression.Match (message).Groups [1].ToString();
		}

		private string parseFactoidGet (string message)
		{
			return FactoidGetExpression.Match (message).Groups [1].ToString();
		}
	}
}

