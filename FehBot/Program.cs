using System;
using IrcDotNet;
using System.Threading;
using System.Linq;

namespace FehBot
{
	class FehBot
	{

		private Boolean run = true;
		private RegistrationInfoFactory infoFactory = new RegistrationInfoFactory();

		public static void Main (string[] args)
		{
			var bot = new FehBot ();
			bot.Run ();
		}

		public void Run() {
			StandardIrcClient client = new StandardIrcClient();

			client.Connected += Connected;
			client.Registered += Registered;
			client.Disconnected += Disconnected;

			// Wait until connection has succeeded or timed out.
			using (var connectedEvent = new ManualResetEventSlim(false))
			{
				client.Connected += (sender2, e2) => connectedEvent.Set();

				var registrationInfo = infoFactory.Registration;

				client.Connect(infoFactory.Server, false, registrationInfo);
				if (!connectedEvent.Wait(100))
				{
					client.Dispose();
					run = false;
					Console.Error.WriteLine ("Connection Timeout");
					return;
				}
			}

			while (run) {
				Console.WriteLine (client.Channels.Count);
			}
		}

		private void Connected(object client, EventArgs e)
		{
			
		}

		private void Disconnected(object client, EventArgs e)
		{
			run = false;
		}

		private void Registered(object _client, EventArgs e)
		{
			var client = (IrcClient)_client;

			client.LocalUser.JoinedChannel += JoinedChannel;

			client.Channels.Join (infoFactory.Channels);
		}

		private void JoinedChannel (object _client, IrcChannelEventArgs e2) 
		{
			var client = e2.Channel.Client;
			var channel = e2.Channel;

			client.LocalUser.SendMessage(channel, "Hello World!");
			client.Quit(2000, "Finished ");

			client.Dispose();

		}
	}
}
