using System;
using IrcDotNet;
using FehBot;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;

namespace FehBot.Handlers
{
	public enum Actions {Link, Factiod, Karma, Status, Tell};

	public interface IHandler
	{
		
		void handle(RegistrationInfoFactory infoFactory, IrcClient client, FehBot bot, IMongoDatabase db, IrcUser from, IrcChannel to, string message);
		void callWebHook(IMongoDatabase db, JObject webHookBody);
	}
}

