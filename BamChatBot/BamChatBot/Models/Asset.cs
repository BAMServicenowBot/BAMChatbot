namespace BamChatBot.Models
{
    public class Asset
    {
        public string Name { get; set; }
        public bool CanBeDeleted { get; set; }
        public string ValueScope { get; set; }
        public string Value { get; set; }
        public string ValueType { get; set; }
        public AssetString StringValue { get; set; }
        public AssetBool BoolValue { get; set; }
        public AssetInt IntValue { get; set; }
        public Credential CredentialUsername { get; set; }
        public Credential CredentialPassword { get; set; }
        public string Id { get; set; }
        public string Sys_id { get; set; }
        public OU Ou { get; set; }
					
    }

}