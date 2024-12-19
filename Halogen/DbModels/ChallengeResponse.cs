using System;
using System.Collections.Generic;

namespace Halogen.DbModels;

public partial class ChallengeResponse
{
    public string Id { get; set; } = null!;

    public string AccountId { get; set; } = null!;

    public string ChallengeId { get; set; } = null!;

    public string Response { get; set; } = null!;

    public DateTime UpdatedOn { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Challenge Challenge { get; set; } = null!;
}
