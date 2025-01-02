using System;
using System.Collections.Generic;

namespace Halogen;

public partial class Apitoken
{
    public string Id { get; set; } = null!;

    public string AccountId { get; set; } = null!;

    public string TokenString { get; set; } = null!;

    public uint Timestamp { get; set; }

    public byte? ValidityDuration { get; set; }

    public string TargetEndpoint { get; set; } = null!;
}
