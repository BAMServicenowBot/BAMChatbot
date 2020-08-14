using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BamChatBot.Models
{
    public class ProcessModel
    {
        public string Sys_id { get; set; }
        public string Name { get; set; }
        public IList<Release> Releases { get; set; }
		public IDictionary<string, List<ProcessParameters>> ProcessParameters { get; set; }
		public bool MissingBots { get; set; }
		public ProcessLastRun LastRun { get; set; }
										  
		public string StartedBy { get; set; }

        public string UserId { get; set; }

        public string Error { get; set; }
		public string ConversationId { get; set; }
		public Robot Bot { get; set; }
		public bool FirstBot { get; set; }
		public bool ReEnterBot { get; set; }
		public string queuedId { get; set; }
		public string releaseId { get; set; }

		public ProcessModel()
        {
            Releases = new List<Release>();
			ProcessParameters = new Dictionary<string, List<ProcessParameters>>();
        }
    }
}
