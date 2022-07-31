using HelperLibrary.Shared;

namespace Halogen.Bindings.ApiBindings; 

public class VisibilityPolicy {
        
    public Enums.Visibility Visibility { get; set; }
}

public interface IPolicyWithDataFormat {
        
    byte DataFormat { get; set; }
}

public interface IPolicyWithSingleTarget {
    
    string? VisibleToTargetId { get; set; }
        
    string? TargetTypeName { get; set; }
}

public interface IPolicyWithMultipleTargets {
    
    string[] VisibleToTargetIds { get; set; }
        
    string TargetTypeName { get; set; }
}

public interface IPolicyWithMultipleTypedTargets {
    
    TypedTarget[] TypedTargets { get; set; }
    
    public sealed class TypedTarget {

        public string[] VisibleToTargetIds { get; set; } = null!;

        public string TargetTypeName { get; set; } = null!;
    }
}