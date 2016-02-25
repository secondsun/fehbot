using System;
using IrcDotNet;
using System.Threading;
using System.Linq;

namespace FehBot
{
	class MainClass
	{



		public static void Main (string[] args)
		{
			StandardIrcClient client = new StandardIrcClient();
			var run = true;

			client.Connected += (object sender, EventArgs e) => {

				client.LocalUser.JoinedChannel += (object ignore, IrcChannelEventArgs e2) => {
					var channel = e2.Channel;
					client.LocalUser.SendMessage(channel, "Hello World!");
					client.Quit(2000, "Finished ");
					run = false;
					client.Dispose();
				};


				client.Channels.Join ("#aerobot-test");
			};

			// Wait until connection has succeeded or timed out.
			using (var connectedEvent = new ManualResetEventSlim(false))
			{
				client.Connected += (sender2, e2) => connectedEvent.Set();

				var  registrationInfo = new IrcUserRegistrationInfo();
				registrationInfo.NickName = "fehbot";
				registrationInfo.UserName = "fehbot";
				registrationInfo.RealName = "FeedHenry Chat Bot";

				client.Connect("irc.freenode.net", false, registrationInfo);
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
	}
}
