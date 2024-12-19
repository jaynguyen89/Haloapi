using System.Text.RegularExpressions;
using HelperLibrary.Shared;
using HelperLibrary.Shared.Helpers;

namespace Halogen.Bindings.ApiBindings;

public sealed class ResponseData {

    public string Id { get; set; } = null!;

    public string Response { get; set; } = null!;

    public List<string> VerifyResponse() {
        Response = Regex.Replace(Response.Trim(), Constants.MultiSpace, Constants.MonoSpace);
        return Response.VerifyLength(nameof(Response), 5);
    }
}

public sealed class ChallengeResponseData {
    
    public string ChallengeId { get; set; } = null!;
    
    public string Response { get; set; } = null!;
    
    public List<string> VerifyResponse() {
        Response = Regex.Replace(Response.Trim(), Constants.MultiSpace, Constants.MonoSpace);
        return Response.VerifyLength(nameof(Response), 5);
    }
}
