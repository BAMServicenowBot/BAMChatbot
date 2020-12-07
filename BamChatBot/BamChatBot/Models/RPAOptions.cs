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
				new PromptOption {Id = "RPAOptions", Value = "Start Process", DisplayText="Start Process", Text="Start Process"},
				new PromptOption {Id = "RPAOptions", Value = "Process Status", DisplayText="Process Status", Text="Process Status"},
				new PromptOption {Id = "RPAOptions", Value = "Stop a Process", DisplayText="Stop a Process", Text="Stop a Process"},
				new PromptOption {Id = "RPAOptions", Value = "bam?id=rpa_new_request&type=incident", DisplayText="openUrl", Text="Report an Issue"},
				new PromptOption {Id = "RPAOptions", Value = "bam?id=rpa_new_request&type=enhancement", DisplayText="openUrl", Text="Request an Enhancement"},
				new PromptOption {Id = "RPAOptions", Value = "bam?id=sc_cat_item&sys_id=a41ac289db7c6f0004b27709af9619a3", DisplayText="openUrl", Text="Submit a New Idea"},
				new PromptOption {Id = "RPAOptions", Value = "**Contact RPA Support**", DisplayText="**Contact RPA Support**", Text="**Contact RPA Support**" },
				new PromptOption {Id = "RPAOptions", Value = "bam?id=rpa_processes", DisplayText="openUrl", Text="Bot Portal"},
				new PromptOption {Id = "RPAOptions", Value = "bam?id=rpa_process_scheduler", DisplayText="openUrl", Text="RPA Process Schedules"},
				new PromptOption {Id = "RPAOptions", Value = "End Chat", DisplayText="End Chat", Text="End Chat"}
			};
		}
	}
}
