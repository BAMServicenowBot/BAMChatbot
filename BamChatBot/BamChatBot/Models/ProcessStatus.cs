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
        public string ActivityId { get; set; }
    }
}
