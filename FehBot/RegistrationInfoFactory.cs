using System;
using IrcDotNet;
using System.Collections.Generic;

namespace FehBot
{
	/// <summary>
	/// This class will load registration info from environment variables or use sensible defaults.
	/// 
	/// FEHBOT_IRC_SERVER : Default irc.freenode.net
	/// FEHBOT_IRC_USERNAME : Default fehbot
	/// FEHBOT_IRC_NICKNAME : Default fehbot
	/// FEHBOT_IRC_PASSWORD : Default ε (an empty string)
	/// FEHBOT_CHANNELS : Default #aerogear-test
	/// </summary>
	public class RegistrationInfoFactory
	{

		private const string FEHBOT_IRC_SERVER_KEY = "FEHBOT_IRC_SERVER";
		private const string FEHBOT_IRC_USERNAME_KEY = "FEHBOT_USERNAME";
		private const string FEHBOT_IRC_NICKNAME_KEY = "FEHBOT_NICKNAME";
		private const string FEHBOT_CHANNELS_KEY = "FEHBOT_CHANNELS";
		private const string FEHBOT_IRC_NICKSERV_PASSWORD_KEY = "FEHBOT_NICKSERV_PASSWORD";

		private readonly string userName, nickName, server, password;
		private readonly string[] channels;

		/// <summary>
		/// Get the readonly property username
		/// </summary>
		/// <value>The name of the user.</value>
		public string UserName { get { return userName; } }

		/// <summary>
		/// Gets the IRC nickname / handle.
		/// </summary>
		/// <value>The name which will appear in IRC.</value>
		public string NickName { get  {return nickName; } }

		/// <summary>
		/// Gets the server URL.
		/// </summary>
		/// <value>The server URL.</value>
		public string Server { get { return server; } }

		/// <summary>
		/// Gets the nick serv password.
		/// </summary>
		/// <value>The nick serv password.</value>
		public string NickServPassword { get { return password; } }

		/// <summary>
		/// Gets the channels to subscribe to on startup.
		/// </summary>
		/// <value>The channels.</value>
		public List<string> Channels {get { return new List<string> (channels); } }


		public IrcUserRegistrationInfo Registration {
			get 
			{
				var reg = new IrcUserRegistrationInfo ();
				reg.RealName = "FeedHenry Chat Bot";
				reg.UserName = userName;
				reg.NickName = nickName;
				return reg;
			}
		}


		public RegistrationInfoFactory ()
		{
			
			nickName = Environment.GetEnvironmentVariable (FEHBOT_IRC_NICKNAME_KEY) ?? "fehbot";
			userName = Environment.GetEnvironmentVariable (FEHBOT_IRC_USERNAME_KEY) ?? "fehbot";
			server = Environment.GetEnvironmentVariable (FEHBOT_IRC_SERVER_KEY) ?? "irc.freenode.net";
			channels = (Environment.GetEnvironmentVariable (FEHBOT_CHANNELS_KEY) ?? "#aerobot-test").Split(',');
			password = Environment.GetEnvironmentVariable (FEHBOT_IRC_NICKSERV_PASSWORD_KEY) ?? "";


		}
	}
}

