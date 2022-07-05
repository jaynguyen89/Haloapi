using System;
using System.Collections.Generic;

namespace Halogen.DbModels
{
    public partial class TrustedDevice
    {
        public string Id { get; set; } = null!;
        public string AccountId { get; set; } = null!;
        public string? DeviceName { get; set; }
        public byte? DeviceType { get; set; }
        public string? UniqueIdentifier { get; set; }
        public string? DeviceLocation { get; set; }
        public string? IpAddress { get; set; }
        public string? OperatingSystem { get; set; }
        public DateTime AddedOn { get; set; }
        public bool IsTrusted { get; set; }

        public virtual Account Account { get; set; } = null!;
    }
}
