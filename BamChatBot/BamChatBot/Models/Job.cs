using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BamChatBot.Models
{
    public class Job
    {
        public JobResult Result { get; set; }
        public string Error { get; set; }
    }

    public class JobResult
    {
        public string Code { get; set; }
        public JobBody Body { get; set; }
    }

    public class JobBody
    {
        public List<JobBodyValue> Value { get; set; }
        public JobBody()
        {
            Value = new List<JobBodyValue>();
        }
    }

    public class JobBodyValue
    {
        public string Key { get; set; }
        public string State { get; set; }
        public string Source { get; set; }
        public string Id { get; set; }
    }
}
