using System.Net;
using HelperLibrary;
using HelperLibrary.Shared.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Halogen.Bindings.ViewModels; 

public sealed class ErrorResponse: StatusCodeResult {
    private new int StatusCode { get; set; } = (int)HttpStatusCode.InternalServerError;
    
    // ReSharper disable once UnusedMember.Global
    public string StatusCodeName => ((HttpStatusCode)StatusCode).ToString().Lucidify();
    
    public string? Message { get; set; }
    
    public object? Value { get; set; }
    
    public bool IsHandled { get; set; }

    public ErrorResponse(bool isHandled = true): base((int)HttpStatusCode.InternalServerError) {
        IsHandled = isHandled;
    }

    public ErrorResponse(HttpStatusCode statusCode, bool isHandled = true) : base((int)statusCode) {
        IsHandled = isHandled;
    }
    
    public ErrorResponse(HttpStatusCode statusCode, string message, bool isHandled = true) : base((int)statusCode) {
        Message = message;
        IsHandled = isHandled;
    }
    
    public ErrorResponse(HttpStatusCode statusCode, object value, bool isHandled = true): base((int)statusCode) {
        Value = value;
        IsHandled = isHandled;
    }
    
    public ErrorResponse(HttpStatusCode statusCode, object value, string message, bool isHandled = true): base((int)statusCode) {
        Value = value;
        Message = message;
        IsHandled = isHandled;
    }
}