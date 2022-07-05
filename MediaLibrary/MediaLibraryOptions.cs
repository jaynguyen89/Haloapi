namespace MediaLibrary;
    
public sealed class MediaLibraryOptions {
    
    public class Development { }
    
    public sealed class Staging: Development { }

    public sealed class Production: Development { }
}
