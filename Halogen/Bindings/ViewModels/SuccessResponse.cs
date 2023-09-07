using System.Net;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Halogen.Bindings.ViewModels; 

public sealed class SuccessResponse: JsonResult {

    public new int StatusCode { get; set; } = (int)HttpStatusCode.OK;
    
    // ReSharper disable once UnusedMember.Global
    public string StatusCodeName => StatusCode.ToString().Lucidify();

    public SuccessResponse(): base(null) { }

    public SuccessResponse(object value) : base(value) { }

    public SuccessResponse(HttpStatusCode? statusCode, object value) : base(value) {
        StatusCode = (int)(statusCode ?? HttpStatusCode.OK);
        ContentType = Constants.ContentTypes!.GetDictionaryValue(Enums.ContentType.Json.ToString().ToLower());
    }
    
    public SuccessResponse(HttpStatusCode? statusCode, object value, Enums.ContentType contentType) : base(value, contentType) {
        StatusCode = (int)(statusCode ?? HttpStatusCode.OK);
        ContentType = Constants.ContentTypes!.GetDictionaryValue(contentType.ToString().ToLower());
    }
}