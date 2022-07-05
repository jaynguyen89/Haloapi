namespace AssistantLibrary;
    
public sealed class AssistantLibraryOptions {
    
    public class Development { }
    
    public sealed class Staging: Development { }

    public sealed class Production: Development { }
}
