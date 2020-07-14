using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BamChatBot.Models
{
	public class Commands
	{
		internal List<string> GetStartCommands()
		{
			var startCommands = new List<string>
			{
				"start process", "start",  "restart bot",  "start bot"
			};
			return startCommands;
		}
	}
}
