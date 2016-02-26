using System;

namespace FehBot.Handlers
{
	public sealed class HandlerUtils
	{
		private HandlerUtils ()
		{
		}

		public static string removeMention (string message, string nickName)
		{
			if (message.StartsWith(nickName)) {
				return message.Substring (message.IndexOf (nickName) + nickName.Length + 1).Trim ();//remove mention
			} 
			return message;
		}

		public static bool addressedToMe (string message, string nick)
		{
			return message.IndexOf(nick) == 0;
		}

	}
}

