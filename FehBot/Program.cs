using System;
using IrcDotNet;
using System.Threading;
using System.Linq;
using MongoDB.Driver;
using System.Collections.Generic;
using FehBot.Handlers;

namespace FehBot
{
	public class FehBot
	{

		private Boolean run = true;
		private RegistrationInfoFactory infoFactory = new RegistrationInfoFactory();
		private IMongoDatabase db;
		private List<IHandler> handlers = new List<IHandler>{new KarmaHandler()};

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

			var mongo = new MongoClient();
			db = mongo.GetDatabase("fehBot");

			// Wait until connection has succeeded or timed out.
			using (var connectedEvent = new ManualResetEventSlim(false))
			{
				client.Connected += (sender2, e2) => connectedEvent.Set();

				var registrationInfo = infoFactory.Registration;

				client.Connect(infoFactory.Server, false, registrationInfo);
				if (!connectedEvent.Wait(1000))
				{
					client.Dispose();
					run = false;
					Console.Error.WriteLine ("Connection Timeout");
					return;
				}
			}

			while (run) {
				
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
			client.LocalUser.MessageReceived += HandleMessage;
			client.Channels.Join (infoFactory.Channels);
		}

		private void JoinedChannel (object _client, IrcChannelEventArgs e2) 
		{
			var client = e2.Channel.Client;
			var channel = e2.Channel;
			channel.MessageReceived += HandleMessage;
			client.LocalUser.SendMessage(channel, "Hello World!");


		}

		void HandleMessage (object sender, IrcMessageEventArgs e)
		{

			IrcUser from;
			IrcClient client;
			IrcChannel channel;
			if (sender is IrcChannel) {
				client = ((IrcChannel)sender).Client;
				channel = (IrcChannel)sender;
			} else {
				client = ((IrcLocalUser)sender).Client;
				channel = null;
			}

			if (e.Source is IrcUser) {
				from =(IrcUser) e.Source;
				handlers.ForEach ( handler => {handler.handle(infoFactory, client , this, db, from, channel ,e.Text);});
			} 
		}
	}
}
