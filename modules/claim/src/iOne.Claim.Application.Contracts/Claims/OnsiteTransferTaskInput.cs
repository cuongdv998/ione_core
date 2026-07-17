using System;
using System.ComponentModel.DataAnnotations;

namespace iOne.Claim.Claims;

public class OnsiteTransferTaskInput
{
    [Required]
    public Guid? ReasonId { get; set; }

    [Required]
    [MaxLength(250)]
    public string? ReasonDescription { get; set; }

    [Required]
    public Guid DepartmentId { get; set; }

    [Required]
    public Guid AssigneeId { get; set; }
}

