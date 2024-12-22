using Halogen.Bindings.ApiBindings;
using Halogen.DbModels;
using HelperLibrary.Shared;
using Newtonsoft.Json;

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

    public static implicit operator PreferenceVM(Preference preference) => new() {
        Id = preference.Id,
        AppTheme = (Enums.ApplicationTheme)preference.ApplicationTheme,
        AppLanguage = (Enums.Language)preference.ApplicationLanguage,
        DataFormats = JsonConvert.DeserializeObject<DataFormatVM[]>(preference.DataFormat!)!,
    };
}
