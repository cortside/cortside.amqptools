namespace Cortside.AmqpTools.Configuration {
    public class AmqpToolsConfiguration {
        public string Queue { get; set; }
        public int Max { get; set; }
        public int InitialCredit { get; set; }
        public int Timeout { get; set; }
        public string Namespace { get; set; }
        public string Key { get; set; }
        public string PolicyName { get; set; }
        public string Protocol { get; set; }
        public int Durable { get; set; }
        public bool Forever { get; set; }
        public string Url => $"{Protocol}://{PolicyName}:{Key}@{Namespace}/";
        public string ConnectionString {
            get {
                if (Namespace.Contains("windows.net")) {
                    return $"Endpoint=sb://{Namespace}/;SharedAccessKeyName={PolicyName};SharedAccessKey={Key}";
                }
                return null;
            }
        }
    }
}
