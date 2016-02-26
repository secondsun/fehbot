using System;
using System.Net;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using MongoDB.Bson;

namespace FehBot
{
	public class RESTServer 
	{
		private bool listening = true;
		private readonly IMongoDatabase db;
		private readonly string LinkIRCNickToUserIdPrefix;
		public RESTServer (IMongoDatabase db, int port) 
		{
			this.db = db;
			string BasePrefix = String.Format("http://+:{0}/", port.ToString());

			LinkIRCNickToUserIdPrefix = BasePrefix + "link/";

		}

		public void StopListening() {
			listening = false;
		}

		public void StartListening() {

			HttpListener listener = new HttpListener();

			listener.Prefixes.Add (LinkIRCNickToUserIdPrefix);

			listener.Start ();

			while (listening) {
				var result = listener.BeginGetContext (new AsyncCallback(HandleRequest),listener);
				while (listening && !result.IsCompleted) {
					//loop
				}
			}

			listener.Stop();

		}

		public void HandleRequest(IAsyncResult result) 
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
			default:
				break;

				}
			}  catch (Exception ex) {
				Console.Write(ex);
			} finally {

				response.Close ();
			}
		}

		void HandleLinkIRCNickToUserIdPrefix (HttpListenerRequest request, HttpListenerResponse response)
		{
			string jsonEntity = readEntity (request);

			JObject entity = JObject.Parse (jsonEntity);
			string nickName = entity.GetValue ("nick").ToString ();
			string remoteName = entity.GetValue ("remoteUserName").ToString ();
			string code = new Random ().Next (10000000, 99999999).ToString();

			saveLinkRequest (nickName, remoteName, code);

			JObject responseObject = new JObject ();
			responseObject.Add ("code", code);
			string responseString = responseObject.ToString ();

			byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);

			response.ContentLength64 = buffer.Length;
			System.IO.Stream output = response.OutputStream;
			output.Write(buffer,0,buffer.Length);

			output.Close();

			response.StatusCode = 200;
		}

		void saveLinkRequest (string nickName, string remoteName, string code)
		{
			var links = db.GetCollection<BsonDocument>("Links");
			var builder = Builders<BsonDocument>.Filter;
			var filter = builder.Eq ("nick", nickName) & builder.Eq ("remoteUserName", remoteName);

			var document = links.Find(filter).FirstOrDefault();
			if (document == null) {
				links.InsertOne (new BsonDocument {
					{ "nick", nickName },
					{ "remoteUserName", remoteName },
					{ "code", code }
				});

			} else {
				var update = Builders<BsonDocument>.Update.Set("code", code);
				links.UpdateOne(filter, update);
			}
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
	}
}

