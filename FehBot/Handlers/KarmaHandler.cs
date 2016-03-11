using System;
using FehBot;
using FehBot.DBUtil;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using MongoDB.Driver;
using IrcDotNet.Collections;
using MongoDB.Bson;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using FehBot.Vo;
using MongoDB.Bson.IO;
using System.Text;

namespace FehBot.Handlers
{
	public class KarmaHandler : IHandler
	{
	    private readonly Regex karmaRe = new Regex(@"(\w+)([+]{2}|[-]{2})");

		public KarmaHandler ()
		{

		}

		private bool isKarmaRequest (string message)
		{
			return karmaRe.IsMatch (message);
		}

		private Dictionary<string, int> parseKarmaRequest (string message)
		{
			MatchCollection found = karmaRe.Matches(message);
			var result = new Dictionary<string, int>();
			if (found.Count > 0) {
				for(var i = 0; i < found.Count; i++) {
					var karmaReqStr = found[i];
					GroupCollection match = karmaReqStr.Groups;
					string user = match[1].Value.ToLower();
					int dir = match[2].Value.Equals("++") ? 1 : -1;
					result.Add(user, dir );
				}
			}
			return result;

		}

		public void callWebHook (IMongoDatabase db, JObject webHookBody)
		{
			var hooks = db.GetWebHookForAction("karma");
			hooks.ForEach ((hook) => {
				string apiKey = hook.ApiKey;
				Uri callbackUrl = new Uri(hook.CallbackUrl);
				string secret = hook.Secret;
				var links = db.GetNickNameLink(webHookBody.GetValue("nick").ToString());
				links.ForEach(link => {
					using (HttpClient client = new HttpClient ()) {
						client.DefaultRequestHeaders.Accept.Clear();
						client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json")); 
						var content = new StringContent(webHookBody.ToString(),Encoding.UTF8, "application/json");

						client.PostAsync(callbackUrl, content).Wait();
					}
				});

			});

		}

		public void handle (RegistrationInfoFactory infoFactory, IrcDotNet.IrcClient client, FehBot bot, IMongoDatabase db, IrcDotNet.IrcUser from, IrcDotNet.IrcChannel channel, string message)
		{
			Console.WriteLine (message);
			if (channel == null) 
			{
				return;
			}

			if (isKarmaRequest(message))
			{
				var request = parseKarmaRequest (message);
				request.Keys.ForEach (nick => {
					Karma document = db.UpdateKarma(nick, channel.Name, infoFactory.Server, request[nick]);

					string template = request[nick] > 0 ? "{0} gained a level! (Karma: {1})":"{0} lost a level! (Karma: {1})";
					client.LocalUser.SendMessage(channel, String.Format(template, nick, document.Score));
					JObject body = new JObject();

					body.Add("nick",document.Nick);
					body.Add("from", from.NickName);
					body.Add("score", document.Score);
					body.Add("channel", document.Channel);
					body.Add("direction", request[nick]);

					callWebHook(db, body);
				});

			}
		}
	}
}

