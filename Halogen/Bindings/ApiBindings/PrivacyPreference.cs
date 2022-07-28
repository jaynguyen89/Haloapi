using HelperLibrary.Shared;

namespace Halogen.Bindings.ApiBindings; 

internal class VisibilityPolicy {
        
    public Enums.Visibility Visibility { get; set; }
}

internal interface IPolicyWithDataFormat {
        
    byte DataFormat { get; set; }
}

internal interface IPolicyWithSingleTarget {
    
    string VisibleToTargetId { get; set; }
        
    string TargetTypeName { get; set; }
}

internal interface IPolicyWithMultipleTargets {
    
    string[] VisibleToTargetIds { get; set; }
        
    string TargetTypeName { get; set; }
}

internal interface IPolicyWithMultipleTypedTargets {
    
    TypedTarget[] TypedTargets { get; set; }
    
    internal sealed class TypedTarget {
        
        public string[] VisibleToTargetIds { get; set; }
        
        public string TargetTypeName { get; set; }
    }
}