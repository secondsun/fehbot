using System;

namespace FehBot.Handlers
{
	public class StatusHandler : IHandler
	{
		public StatusHandler ()
		{
		}

		public void handle (RegistrationInfoFactory infoFactory, IrcDotNet.IrcClient client, FehBot bot, MongoDB.Driver.IMongoDatabase db, IrcDotNet.IrcUser from, IrcDotNet.IrcChannel to, string message)
		{
			throw new NotImplementedException ();
		}

	}
}

