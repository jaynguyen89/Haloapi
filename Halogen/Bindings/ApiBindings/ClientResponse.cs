using HelperLibrary.Shared;

namespace Halogen.Bindings.ApiBindings; 

public sealed class ClientResponse {

    public Enums.ApiResult Result { get; set; } = Enums.ApiResult.Success;

    public object? Data { get; set; }
}