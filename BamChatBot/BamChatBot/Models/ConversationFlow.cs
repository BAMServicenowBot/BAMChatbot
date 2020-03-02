using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BamChatBot.Models
{
	public class ConversationFlow
	{
		public IDictionary<string, List<ProcessParameters>> ProcessParameters { get; set; }
		public bool AskingForParameters { get; set; }
		public int GroupLastIndex { get; set; }
		public int ParamLastIndex { get; set; }
	}
}
