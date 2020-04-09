using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BamChatBot.Models
{
	public class ConversationFlowInput
	{
		public string u_chatbot_conversation_flow { get; set; }
		public string u_value { get; set; }
		public string release { get; set; }
		public string paramName { get; set; }

		public string paramType { get; set; }
		public string u_conversation_id { get; set; }
		public string u_parent_id { get; set; }
		public bool u_is_object { get; set; }
		public bool u_is_array { get; set; }

	}
}
