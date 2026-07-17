using System;
using System.ComponentModel.DataAnnotations;

namespace iOne.Policy.Policies;

/// <summary>
/// Per-coverage endorsement change (Tăng/Giảm phí) for endorsement payload.
/// Sent from FE; backend persists one PolicyAmount per item (when fee item COVERAGE_{coverageId} exists).
/// </summary>
public class EndorsementCoverageChangeDto
{
    [Required]
    public Guid CoverageId { get; set; }

    /// <summary>
    /// Change amount (Premium + Vat change). Can be negative (giảm phí) or positive (tăng phí).
    /// </summary>
    public decimal ChangeAmount { get; set; }
}
