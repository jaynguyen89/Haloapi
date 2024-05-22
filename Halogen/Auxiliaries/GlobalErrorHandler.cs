using System.Net;
using Halogen.Bindings.ViewModels;
using HelperLibrary.Shared.Logger;
using Newtonsoft.Json;

namespace Halogen.Auxiliaries;

public sealed class GlobalErrorHandler {

    private readonly RequestDelegate _next;
    private readonly ILoggerService _logger;

    public GlobalErrorHandler(RequestDelegate next, ILoggerService logger) {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext httpContext) {
        try {
            await _next(httpContext);
        }
        catch (Exception ex) {
            await HandleError(httpContext, ex);
        }
    }

    public async Task HandleError(HttpContext context, Exception exception) {
        _logger.Log(new LoggerBinding<GlobalErrorHandler> {
            Message = "Uncaught exception, possibly from the service factories. Please check previous logs for more clues.",
            Data = exception
        });

        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        await context.Response.WriteAsync(JsonConvert.SerializeObject(new ErrorResponse(HttpStatusCode.InternalServerError)));
    }
}