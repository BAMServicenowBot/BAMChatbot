﻿using System.Collections.Generic;

namespace BamChatBot.Models
{
    public class Release
    {
        public string U_key { get; set; }
        public UOU U_ou { get; set; }
        public string U_robots { get; set; }
        public string Sys_id { get; set; }
        public IList<Asset> Assets { get; set; }
					
    }
}