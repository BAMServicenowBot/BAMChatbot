using System.Collections.Generic;

namespace BamChatBot.Models
{
    public class Release
    {
        public string u_key { get; set; }
        public UOU u_ou { get; set; }
        public string u_robots { get; set; }
		public List<Robot> robots { get; set; }
		public string sys_id { get; set; }
        public IList<Asset> assets { get; set; }
		public bool parameters_required { get; set; }
		public List<ProcessParameters> parameters { get; set; }
		public Release()
		{
			parameters = new List<ProcessParameters>();
			robots = new List<Robot>();
		}

	}

	public class Robot
	{
		public string id { get; set; }
		public string name { get; set; }
		public bool Shown { get; set; }
		public bool Selected { get; set; }
	}
}