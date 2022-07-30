namespace Halogen.Bindings.ServiceBindings; 

public sealed class CacheEntry {

    internal string Key { get; set; } = null!;

    internal object Value { get; set; } = null!;
}