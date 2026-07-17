using System;
using System.ComponentModel.DataAnnotations;

namespace iOne.Claim.Claims;

/// <summary>
/// Input for assigning an on-site assessment task for a claim.
/// </summary>
public class AssignOnsiteAssessmentInput
{
    /// <summary>
    /// Organization/department ID of the onsite assessment unit (hr_department with dept_level = Unit).
    /// </summary>
    [Required]
    public Guid AssigneeOrganizationId { get; set; }

    /// <summary>
    /// Employee ID of the onsite assessor.
    /// </summary>
    [Required]
    public Guid AssigneeId { get; set; }

    /// <summary>
    /// Planned start datetime of the onsite assessment (server local time).
    /// </summary>
    [Required]
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Planned end datetime of the onsite assessment (server local time).
    /// </summary>
    [Required]
    public DateTime EndDate { get; set; }
}

