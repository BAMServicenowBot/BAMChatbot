using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace BamChatBot.Models
{
	public class RPAOptions
	{
		public List<PromptOption> Options { get; private set; }
		public RPAOptions()
		{
			Options = new List<PromptOption>
			{
				new PromptOption {Id = "RPAOptions", Value = "Start Process"},
				new PromptOption {Id = "RPAOptions", Value = "Process Status"},
				new PromptOption {Id = "RPAOptions", Value = "Stop a Process"},
				new PromptOption {Id = "RPAOptions", Value = "Report an Issue"},
				new PromptOption {Id = "RPAOptions", Value = "Request an Enhancement"},
				new PromptOption {Id = "RPAOptions", Value = "Submit a New Idea"},
				new PromptOption {Id = "RPAOptions", Value = "**Contact RPA Support**"},
				new PromptOption {Id = "RPAOptions", Value = "Bot Portal"},
				new PromptOption {Id = "RPAOptions", Value = "RPA Process Schedules"},
				new PromptOption {Id = "RPAOptions", Value = "End Chat"}
			};
		}
	}
}
