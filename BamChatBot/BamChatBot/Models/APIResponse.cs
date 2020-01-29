using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BamChatBot.Models
{
    public class APIResponse
    {
        public string Content { get; set; }
        public bool IsSuccess { get; set; }
		public string Error { get; set; }
	}
}
