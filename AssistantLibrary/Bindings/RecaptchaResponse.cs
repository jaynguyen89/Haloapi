#pragma warning disable 8618
using Newtonsoft.Json;

namespace AssistantLibrary.Bindings; 

public sealed class RecaptchaResponse {
    [JsonProperty("success")]
    public bool Result { get; set; }
    
    [JsonProperty("challenge_ts")]
    public DateTime? VerifiedOn { get; set; }
    
    [JsonProperty("hostname")]
    public string HostName { get; set; }
    
    [JsonProperty("error-codes")]
    public string[] Errors { get; set; }
}