using System;
using System.Net;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using MongoDB.Bson;
using FehBot.Vo;
using FehBot.DBUtil;

namespace FehBot
{
	public class RESTServer 
	{
		private bool listening = true;
		private readonly IMongoDatabase db;
		private readonly string LinkIRCNickToUserIdPrefix;
		private readonly string WebHookRegistrationPrefix;

		public RESTServer (IMongoDatabase db, int port) 
		{
			this.db = db;
			string BasePrefix = String.Format("http://+:{0}/", port.ToString());

			LinkIRCNickToUserIdPrefix = BasePrefix + "link/";
			WebHookRegistrationPrefix = BasePrefix + "webhook/";
		}

		public void StopListening() {
			listening = false;
		}

		public void StartListening() {

			HttpListener listener = new HttpListener();

			listener.Prefixes.Add (LinkIRCNickToUserIdPrefix);
			listener.Prefixes.Add (WebHookRegistrationPrefix);

			listener.Start ();

			while (listening) {
				var result = listener.BeginGetContext (new AsyncCallback(HandleRequest),listener);
				while (listening && !result.IsCompleted) {
					//loop
				}
			}

			listener.Stop();

		}

		void HandleRequest(IAsyncResult result) 
		{
			HttpListener listener = (HttpListener) result.AsyncState;
			HttpListenerContext context = listener.EndGetContext(result);
			HttpListenerRequest request = context.Request;
			HttpListenerResponse response = context.Response;

			string url = request.Url.ToString();

			try {
			switch (request.Url.AbsolutePath) {
			case "/link":
				HandleLinkIRCNickToUserIdPrefix (request, response);
				break;
			case "/webhook":
				HandleWebHookPrefix (request, response);
				break;
			default:
				break;

				}
			}  catch (Exception ex) {
				Console.Write(ex);
			} finally {

				response.Close ();
			}
		}

		void HandleWebHookPrefix (HttpListenerRequest request, HttpListenerResponse response)
		{
			string jsonEntity = readEntity (request);

			WebHook webHook = WebHook.FromJson (JObject.Parse (jsonEntity));

			string method = request.HttpMethod;

			switch (method.ToLower()) {
			case "post":
				SaveWebhook (webHook, response);
				break;
				case "delete":
				 RemoveWebhook (webHook, response);
				break;
			default:
				 SendUnsupportedMethod (response);
				break;
			}

		}

		void RemoveWebhook (WebHook webHook, HttpListenerResponse response)
		{
			
			var result = db.DeleteWebHooks(webHook);
			SendResponse (response, "deleted " + result.DeletedCount, 200);

		}

		void SaveWebhook (WebHook webHook, HttpListenerResponse response)
		{

			string secret = new Random ().Next (100000000, 999999999).ToString();
			webHook.Secret = secret;

			try {
				db.CreateWebHook(webHook);

				JObject responseObject = new JObject ();
				responseObject.Add ("secret", secret);

				SendResponse (response, responseObject, 200);
			} catch (DuplicateObjectException ignore) {
				SendResponse (response, "WebHook Already Exists", 500);
			}

		}

		void HandleLinkIRCNickToUserIdPrefix (HttpListenerRequest request, HttpListenerResponse response)
		{
			string jsonEntity = readEntity (request);

			NickNameLink entity = NickNameLink.FromJson (JObject.Parse (jsonEntity));
			entity.Code = new Random ().Next (10000000, 99999999).ToString();

			db.SaveLinkRequest (entity);

			JObject responseObject = new JObject ();
			responseObject.Add ("code", entity.Code);

			SendResponse (response, responseObject, 200);
		}

		string readEntity (HttpListenerRequest request)
		{
			System.IO.Stream body = request.InputStream;
			System.Text.Encoding encoding = request.ContentEncoding;
			System.IO.StreamReader reader = new System.IO.StreamReader(body, encoding);
			string entity = reader.ReadToEnd();
			body.Close();
			reader.Close();
			return entity;

		}

		void SendResponse (HttpListenerResponse response, JObject body, int status)
		{
			string responseString = body.ToString ();
			byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
			response.ContentLength64 = buffer.Length;
			System.IO.Stream output = response.OutputStream;
			output.Write(buffer,0,buffer.Length);
			output.Close();

			response.StatusCode = status;
		}

		void SendUnsupportedMethod (HttpListenerResponse response)
		{
			SendResponse (response, "unsupported HTTP Method", 500);
		}

		void SendResponse (HttpListenerResponse response, string message, int status)
		{
			JObject responseObject = new JObject ();
			responseObject.Add ("message", message);
			string responseString = responseObject.ToString ();
			byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
			response.ContentLength64 = buffer.Length;
			System.IO.Stream output = response.OutputStream;
			output.Write(buffer,0,buffer.Length);
			output.Close();

			response.StatusCode = status;
		}
	}
}

