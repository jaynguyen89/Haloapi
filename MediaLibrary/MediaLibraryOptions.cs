#pragma warning disable 8618
namespace MediaLibrary;
    
public sealed class MediaLibraryOptions {
    
    public class Local {
        public string HttpClientBaseUri { get; set; }
        public MediaRoutePath MediaRoutePath { get; set; }
        public DbSettings DbSettings { get; set; }
    }

    public class Development: Local { }
    
    public sealed class Staging: Local { }

    public sealed class Production: Local { }
}

public sealed class DbSettings {
    public string Endpoint { get; set; }
    public string Port { get; set; }
    public string DbName { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
}

public sealed class MediaRoutePath {
    public string Avatar { get; set; }
    public string Cover { get; set; }
    public string Attachment { get; set; }
}
