using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BamChatBot.Models
{
    public class APIRequest
    {
        public List<string> Ids { get; set; }
        public bool IsJob { get; set; }
		public string ProcessId { get; set; }
		public bool Queued { get; set; }
		public APIRequest()
        {
            Ids = new List<string>();
        }
    }
}
