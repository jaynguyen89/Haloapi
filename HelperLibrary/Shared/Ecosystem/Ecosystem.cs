namespace HelperLibrary.Shared.Ecosystem; 

public sealed class Ecosystem: IEcosystem {

    public string Environment { get; init; } = null!;

    public bool UseLongerId { get; set; }

    public ServerSettings ServerSetting { get; set; } = null!;

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
        public string AwsAccessKeyId { get; set; } = null!;
        public string AwsSecretAccessKey { get; set; } = null!;
        public string AwsRegion { get; set; } = null!;
    }
}