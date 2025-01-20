using System;
using System.Collections.Generic;

namespace Halogen.DbModels;

public partial class Preference
{
    public string Id { get; set; } = null!;

    public string AccountId { get; set; } = null!;

    public byte ApplicationTheme { get; set; }

    public byte ApplicationLanguage { get; set; }

    public string DataFormat { get; set; } = null!;

    public string Privacy { get; set; } = null!;

    public DateTime UpdatedOn { get; set; }

    public virtual Account Account { get; set; } = null!;
}
