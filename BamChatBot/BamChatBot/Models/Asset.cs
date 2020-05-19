namespace BamChatBot.Models
{
    public class Asset
    {
        public string name { get; set; }
        public bool canBeDeleted { get; set; }
        public string ValueScope { get; set; }
        public string value { get; set; }
        public string ValueType { get; set; }
        public AssetString StringValue { get; set; }
        public AssetBool BoolValue { get; set; }
        public AssetInt IntValue { get; set; }
        public Credential CredentialUsername { get; set; }
        public Credential CredentialPassword { get; set; }
        public string Id { get; set; }
        public string sys_id { get; set; }
        public OU ou { get; set; }
		public bool ValueFromChild { get; set; }
		public bool PerRobot { get; set; }
		public string UserId { get; set; }

	}

}