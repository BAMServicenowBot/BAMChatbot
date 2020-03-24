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
		public string sys_id { get; set; }
		public int u_last_question_index { get; set; }
		public string u_release_id { get; set; }
		public string u_conversation_id { get; set; }
		public string u_param_name { get; set; }
		public string u_type { get; set; }
		public bool u_active { get; set; }
	}
}
