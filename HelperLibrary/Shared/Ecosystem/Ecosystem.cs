namespace HelperLibrary.Shared.Ecosystem; 

public sealed class Ecosystem: IEcosystem {

    public string Environment { get; init; } = null!;

    public bool UseLongerId { get; set; } = false;

    public ServerSettings ServerSetting { get; set; }

    public string GetEnvironment() {
        return Environment;
    }

    public bool GetUseLongerId() {
        return UseLongerId;
    }

    public ServerSettings GetServerSettings() {
        return ServerSetting;
    }

    public sealed class ServerSettings {
        public string AwsAccessKeyId { get; set; }
        public string AwsSecretAccessKey { get; set; }
        public string AwsRegion { get; set; }
    }
}