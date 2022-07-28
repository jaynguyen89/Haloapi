namespace HelperLibrary.Shared.Ecosystem; 

public sealed class Ecosystem: IEcosystem {

    public string Environment { get; init; } = null!;

    public bool UseLongerId { get; set; } = false;

    public string GetEnvironment() {
        return Environment;
    }

    public bool GetUseLongerId() {
        return UseLongerId;
    }
}