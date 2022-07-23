namespace Halogen.Bindings.ServiceBindings; 

internal sealed class CacheEntry {

    internal string Key { get; set; } = null!;

    internal object Value { get; set; } = null!;
}