namespace Halogen.Bindings; 

public sealed class HaloArgumentNullException<T>: ArgumentNullException {

    public override string Message => $"{typeof(T).Name}: exception thrown at {ParamName}";
    
    public HaloArgumentNullException(string paramName): base(paramName) { }
}
