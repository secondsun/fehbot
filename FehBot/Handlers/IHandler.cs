using System;
using IrcDotNet;
using FehBot;
using MongoDB.Driver;

namespace FehBot.Handlers
{
	public interface IHandler
	{
		void handle(RegistrationInfoFactory infoFactory, IrcClient client, FehBot bot, IMongoDatabase db, IrcUser from, IrcChannel to, string message);
	}
}

