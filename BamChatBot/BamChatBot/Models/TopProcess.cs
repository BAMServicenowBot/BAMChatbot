using Microsoft.Bot.Builder.Dialogs.Choices;
using System;
using System.Collections.Generic;
using System.Linq;


namespace BamChatBot.Models
{
	public class TopProcess
	{
		public List<Choice> Choices { get; set; }
		public int LastIndex { get; set; }
		public TopProcess()
		{
			Choices = new List<Choice>();
		}
	}
}
