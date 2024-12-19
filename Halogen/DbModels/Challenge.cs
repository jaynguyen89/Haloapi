using System;
using System.Collections.Generic;

namespace Halogen.DbModels;

public partial class Challenge
{
    public string Id { get; set; } = null!;

    public string Question { get; set; } = null!;

    public string? CreatedById { get; set; }

    public DateTime CreatedOn { get; set; }

    public virtual ICollection<ChallengeResponse> ChallengeResponses { get; set; } = new List<ChallengeResponse>();

    public virtual Account? CreatedBy { get; set; }
}
