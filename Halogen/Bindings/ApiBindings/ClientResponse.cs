﻿using HelperLibrary.Shared;

namespace Halogen.Bindings.ApiBindings; 

internal sealed class ClientResponse {

    public Enums.ApiResult Result { get; set; } = Enums.ApiResult.SUCCESS;

    public object? Data { get; set; }
}