using System;
using System.Text.RegularExpressions;
using MongoDB.Driver;
using MongoDB.Bson;

namespace FehBot.Handlers
{
	public class AccountLinkHandler : IHandler
	{
		private readonly Regex CodeGetExpression = new Regex(@"^\?code (\d\d\d\d\d\d\d\d)$");

		public AccountLinkHandler ()
		{
		}

		public void handle (RegistrationInfoFactory infoFactory, IrcDotNet.IrcClient client, FehBot bot, MongoDB.Driver.IMongoDatabase db, IrcDotNet.IrcUser from, IrcDotNet.IrcChannel to, string message)
		{
			if (to == null && isCode (message)) 
			{
				string code = parseCodeRequest(message);
				string nick = from.NickName;
				var document = getForCode (nick.Trim(), code.Trim(), db);
				if (document != null) {
					addListener (document, db);
				}

			}
		}

		private void addListener (BsonDocument document, IMongoDatabase db)
		{
			var remoteUserName = document.GetValue ("remoteUserName").ToString ();
			var nick = document.GetValue ("nick").ToString ();

			var links = db.GetCollection<BsonDocument>("Links");
			var eventUsers = db.GetCollection<BsonDocument>("EventUsers");

			var builder = Builders<BsonDocument>.Filter;
			var filter = builder.Eq ("nick", document.GetValue("nick").ToString());
			links.DeleteMany (filter);

			filter = builder.Eq ("nick", document.GetValue ("nick").ToString ()) & builder.Eq ("remoteUserName", document.GetValue ("remoteUserName").ToString ());
			document = eventUsers.Find(filter).FirstOrDefault();

			if (document == null) {
				eventUsers.InsertOne (new BsonDocument {
					{ "nick", nick },
					{ "remoteUserName", remoteUserName }
					});
			} 

		}

		private BsonDocument getForCode(string nick, string code, IMongoDatabase db)
		{
			var links = db.GetCollection<BsonDocument>("Links");
			var builder = Builders<BsonDocument>.Filter;
			var filter = builder.Eq ("nick", nick) & builder.Eq ("code", code);

			BsonDocument document = links.Find(filter).FirstOrDefault();
			return document;

		}

		private string parseCodeRequest (string message)
		{
			return CodeGetExpression.Match (message).Groups [1].ToString();
		}

		private bool isCode (string message)
		{
			return CodeGetExpression.IsMatch (message);
		}
	}
}

