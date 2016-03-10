using System;
using MongoDB.Bson;
using Newtonsoft.Json.Linq;

namespace FehBot.Vo
{
	public class WebHook
	{

		public string Secret { get; set;}
		public string ApiKey { get; set;}
		public string CallbackUrl { get; set;}
		public string Action { get; set;}

		public WebHook ()
		{
		}

		public JObject ToJSON() {
			JObject json = new JObject();
			if (Secret != null) {
				json.Add("secret", Secret);
			}
			json.Add("apiKey", ApiKey);
			json.Add("callbackUrl", CallbackUrl);
			json.Add("action", Action);
			return json;
		}

		public BsonDocument ToBSON() {
			return new BsonDocument {
				{ "apiKey", ApiKey },
				{ "action", Action },
				{ "callbackUrl", CallbackUrl },
				{ "secret", Secret }
			};
		}

		public static WebHook FromJson(string data) {
			return FromJson (JObject.Parse (data));
		}

		public static WebHook FromJson(JObject data) {
			var result = new WebHook ();
			result.Secret = data.GetValue ("secret")?.ToString ();
			result.Action = data.GetValue ("action")?.ToString ();
			result.ApiKey = data.GetValue ("apiKey")?.ToString ();
			result.CallbackUrl = data.GetValue ("callbackUrl")?.ToString ();
			return result;
		}

	}

	public class NickNameLink {
		public string Nick { get; set;}
		public string Code { get; set;}
		public string RemoteUserName { get; set; }

		public BsonDocument ToBSON() {
			return new BsonDocument {
				{ "nick", Nick },
				{ "code", Code },
				{ "remoteUserName", RemoteUserName },
			};
		}

		public static NickNameLink FromJson(string data) {
			return FromJson (JObject.Parse (data));
		}

		public static NickNameLink FromJson(JObject data) {
			var result = new NickNameLink ();
			result.Nick = data.GetValue ("nick")?.ToString ();
			result.Code = data.GetValue ("code")?.ToString ();
			result.RemoteUserName = data.GetValue ("remoteUserName")?.ToString ();

			return result;
		}

	}

	public class Karma {

		public string Nick { get; set;}
		public string Channel { get; set;}
		public string Network { get; set;}
		public int Score { get; set;} = 0;

		public Karma(){}

		public BsonDocument ToBSON() {
			return new BsonDocument {
				{ "nick", Nick },
				{ "channel", Channel },
				{ "network", Network },
				{"score", Score  }
			};
		}

		public static Karma FromJson(string data) {
			return FromJson (JObject.Parse (data));
		}

		public static Karma FromJson(JObject data) {
			var result = new Karma ();
			result.Nick = (data.GetValue ("nick")?.ToString ()) ?? "";
			result.Channel = (data.GetValue ("channel")?.ToString ()) ?? "";
			result.Network = (data.GetValue ("network")?.ToString ()) ?? "";
			result.Score = Int32.Parse((data.GetValue ("score")?.ToString()) ?? "0");

			return result;
		}


	}


}

