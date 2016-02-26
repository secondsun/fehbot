using System;
using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Driver;
using Itenso.TimePeriod;

namespace FehBot.Handlers
{
	public class TellHandler : IHandler
	{
		private readonly Regex TellExpression = new Regex(@"^tell (\w+)\s(.+)$");

		public TellHandler ()
		{
		}

		public void handle (RegistrationInfoFactory infoFactory, IrcDotNet.IrcClient client, FehBot bot, MongoDB.Driver.IMongoDatabase db, IrcDotNet.IrcUser from, IrcDotNet.IrcChannel to, string message)
		{

			var nickName = infoFactory.NickName;
			var messageCollection = db.GetCollection<BsonDocument> ("Message");
			if (isTellMessage (message, nickName)) {
				Tuple<string, string> request = parseRequest (HandlerUtils.removeMention (message, nickName));
				if (!request.Item1.Equals (nickName)) {
					
					messageCollection.InsertOne (new BsonDocument {
						{ "network", infoFactory.Server },
						{ "channel", to.Name.ToLower () },
						{ "recipient", request.Item1 },
						{ "sender", from.NickName },
						{ "message", request.Item2 }
					});
					client.LocalUser.SendMessage (to, from.NickName + ": kk");
				}
			} else {
					var builder = Builders<BsonDocument>.Filter;
					var filter = builder.Eq("recipient", from.NickName) & builder.Eq("channel", to.Name.ToLower()) & builder.Eq("network", infoFactory.Server);
					messageCollection.Find (filter).ForEachAsync ((document) => {
						var objectId = document.GetValue("_id").AsObjectId;
						var createdDate = objectId.CreationTime;
					    DateDiff diff = new DateDiff(createdDate, DateTime.Now.ToUniversalTime());
						client.LocalUser.SendMessage(to, String.Format("({0} ago) {1} => {2}, {3}", diff.GetDescription( 6 ), document.GetValue("sender").AsString, document.GetValue("recipient").AsString, document.GetValue("message").AsString));
						messageCollection.DeleteOne(document);
					});


				}

		}

		private bool isTellMessage (string message, string nickName)
		{
			string normalizedMessage = HandlerUtils.removeMention (message, nickName);
			return (HandlerUtils.addressedToMe (message, nickName) && TellExpression.IsMatch (normalizedMessage));
		}

		private Tuple<string, string> parseRequest (string message)
		{
			var match = TellExpression.Match(message).Groups;
			return new Tuple<string, string> (match[1].ToString().Trim(),match[2].ToString().Trim());
		}
	}
}

