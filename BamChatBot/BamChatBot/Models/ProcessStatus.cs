using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BamChatBot.Models
{
    public class ProcessStatus
    {
        public string State { get; set; }
        public string Release { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public string ChatbotUser { get; set; }
		public string Process { get; set; }
		public int SuccessfulExecutions { get; set; }
		public int Exceptions { get; set; }
		public int TotalTransactions { get; set; }
		public string Runtime { get; set; }
		public double TotalTransSuccessful { get; set; }
		public double TotalExceptions { get; set; }
		public string ProcessType { get; set; }
		public bool IsCompletation { get; set; }

	}
}
