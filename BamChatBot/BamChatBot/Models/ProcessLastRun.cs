using System;

namespace BamChatBot.Models
{
	public class ProcessLastRun
	{
		public DateTime? StartTime { get; set; }
		public DateTime? EndTime { get; set; }
		public string State { get; set; }
		public string info { get; set; }
	}
}