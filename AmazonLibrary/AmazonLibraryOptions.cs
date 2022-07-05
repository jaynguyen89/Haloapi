namespace AmazonLibrary;
    
public sealed class AmazonLibraryOptions {

    public class Development { }
    
    public sealed class Staging: Development { }

    public sealed class Production: Development { }
}
