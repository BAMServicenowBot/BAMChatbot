using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BamChatBot.Models;

namespace BamChatBot
{
    public class ProcessDetails
    {
        public User User { get; set; }
        public List<ProcessModel> Processes { get; set; }
        public ProcessModel ProcessSelected { get; set; }
        public string Action { get; set; }
        public string ConfirmAction { get; set; }
        public int MoreThan10Index { get; set; }
        public string Error { get; set; }
        public List<Job> Jobs { get; set; }
		public bool LoadMore { get; set; }
		public ConversationFlow CurrentQuestion { get; set; }
		public int AttemptCount { get; set; }
		public ProcessDetails()
        {
            Processes = new List<ProcessModel>();
            Jobs = new List<Job>();
        }

    }
}
