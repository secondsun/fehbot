using System;
using IrcDotNet;
using FehBot;

namespace FehBot
{
	public interface IHandler
	{
		void handle(RegistrationInfoFactory infoFactory, IrcClient client, FehBot bot, object db, IrcClient from, IrcChannel to, string message);
	}
}

