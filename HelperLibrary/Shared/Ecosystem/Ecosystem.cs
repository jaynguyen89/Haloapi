namespace HelperLibrary.Shared.Ecosystem; 

public sealed class Ecosystem: IEcosystem {

    public string Environment { get; init; } = null!;

    public string GetEnvironment() {
        return Environment;
    }
}