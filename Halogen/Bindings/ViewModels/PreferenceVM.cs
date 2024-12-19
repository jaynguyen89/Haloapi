using Halogen.Bindings.ApiBindings;
using HelperLibrary.Shared;

namespace Halogen.Bindings.ViewModels;

public sealed class DataFormatVM {
        
    public DataFormat.DataType DataType { get; set; }
        
    public byte Format { get; set; }
}

public sealed class PreferenceVM {

    public string Id { get; set; } = null!;
    
    public Enums.ApplicationTheme AppTheme { get; set; }
    
    public Enums.Language AppLanguage { get; set; }

    public DataFormatVM[] DataFormats { get; set; } = null!;
}
