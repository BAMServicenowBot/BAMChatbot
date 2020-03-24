namespace BamChatBot.Models
{
	public class ProcessParameters
	{
		public string sys_id { get; set; }
		public string parmName { get; set; }
		public string parmType { get; set; }
		public string value { get; set; }
		public InputObj inputType { get; set; }
	}

	public class InputObj
	{
		public string field { get; set; }
		public string type { get; set; }
		public string valueField { get; set; }
	}
}