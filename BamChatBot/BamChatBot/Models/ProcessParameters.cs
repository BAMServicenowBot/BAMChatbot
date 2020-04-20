using System.Collections.Generic;

namespace BamChatBot.Models
{
	public class ProcessParameters
	{
		public string sys_id { get; set; }
		public string parmName { get; set; }
		public string parmType { get; set; }
		public string value { get; set; }
		public string parmId { get; set; }
		public string parentId { get; set; }
		public List<ProcessParameters> obj { get; set; }
		public List<ProcessParameters> array { get; set; }
		public int length { get; set; }
		public InputObj inputType { get; set; }
		public bool isObjArray { get; set; }
		public bool required { get; set; }

		public ProcessParameters()
		{
			obj = new List<ProcessParameters>();
			array = new List<ProcessParameters>();
		}
	}

	public class InputObj
	{
		public string field { get; set; }
		public string type { get; set; }
		public string valueField { get; set; }
	}
}