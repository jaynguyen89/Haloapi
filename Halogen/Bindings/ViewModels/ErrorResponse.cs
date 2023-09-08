using System.Net;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Helpers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Halogen.Bindings.ViewModels; 

public sealed class ErrorResponse: ContentResult {
    
    // ReSharper disable once UnusedMember.Global
    public string StatusCodeName => ((HttpStatusCode)(StatusCode ?? 500)).ToString().Lucidify();

    public ErrorResponse() {
        StatusCode = (int)HttpStatusCode.InternalServerError;
        Content = JsonConvert.SerializeObject(new { statusCodeName = StatusCodeName });
        ContentType = Constants.ContentTypes["json"];
    }
    
    public ErrorResponse(HttpStatusCode statusCode) {
        StatusCode = (int)statusCode;
        Content = JsonConvert.SerializeObject(new { statusCodeName = StatusCodeName });
        ContentType = Constants.ContentTypes["json"];
    }
    
    public ErrorResponse(HttpStatusCode statusCode, object value) {
        StatusCode = (int)statusCode;
        Content = JsonConvert.SerializeObject(new { statusCodeName = StatusCodeName, value });
        ContentType = Constants.ContentTypes["json"];
    }
}