using System;
using System.ComponentModel.DataAnnotations;

namespace iOne.Claim.Claims;

public class ReassignOnsiteAssessmentInput
{
    [Required]
    public Guid AssigneeOrganizationId { get; set; }

    [Required]
    public Guid AssigneeId { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }
}
