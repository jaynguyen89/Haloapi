namespace HelperLibrary.Shared.Ecosystem; 

public interface IEcosystem {

    string GetEnvironment();

    bool GetUseLongerId();

    Ecosystem.ServerSettings GetServerSettings();
    
    class ServerSettings {
        public string AwsAccessKeyId { get; set; }
        public string AwsSecretAccessKey { get; set; }
        public string AwsRegion { get; set; }
        public string AwsLogGroupName { get; set; }
    }
}