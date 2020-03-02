namespace BamChatBot.Models
{
	public class ProcessParameters
	{
		public string Sys_id { get; set; }
		public string ParmName { get; set; }
		public string ParmType { get; set; }
		public InputObj InputType { get; set; }
	}

	public class InputObj
	{
		public string Field { get; set; }
		public string Type { get; set; }
		public string ValueField { get; set; }
	}
}