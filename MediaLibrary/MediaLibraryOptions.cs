namespace MediaLibrary;
    
public sealed class MediaLibraryOptions {
    
    public class Local {
        public DbSettings DbSettings { get; set; }
    }

    public class Development: Local { }
    
    public sealed class Staging: Development { }

    public sealed class Production: Development { }
}

public sealed class DbSettings {
    public string Endpoint { get; set; }
    public string Port { get; set; }
    public string DbName { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
}