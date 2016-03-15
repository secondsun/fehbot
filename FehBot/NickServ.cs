using System;
using IrcDotNet;
using System.Threading;
using System.Linq;
using MongoDB.Driver;
using System.Collections.Generic;
using FehBot.Handlers;
using System.Threading.Tasks;

namespace FehBot
{
	class NickServ : IIrcMessageTarget
	{
		#region IIrcMessageTarget implementation
		public string Name {
			get {
				return "NickServ";
			}
		}
		#endregion
	}

}
