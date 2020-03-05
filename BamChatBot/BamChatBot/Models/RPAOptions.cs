using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BamChatBot.Models
{
	public class RPAOptions
	{
		public List<string> Options { get; private set; }
		public RPAOptions()
		{
			Options = new List<string>
			{
				"Start Process",
				"Process Status",
				"Report an Issue",
				"Request an Enhancement",
				"Submit a New Idea",
				"Contact RPA Support",
				"Stop a Process",
				"Start Over",
				"Done"
			};
	
		}
	}
}
