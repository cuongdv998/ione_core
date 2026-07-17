using System;
using System.ComponentModel.DataAnnotations;

namespace iOne.Claim.Claims;

public class OnsiteRejectTaskInput
{
    [Required]
    public Guid? ReasonId { get; set; }

    [Required]
    [MaxLength(250)]
    public string? ReasonDescription { get; set; }
}

