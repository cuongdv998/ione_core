using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace iOne.Policy.Policies;

public class PartnerMotorPolicyIssueInput
{
    [Required]
    public Guid PolicyId { get; set; }

    public MotorPolicyOwnerInfoDto? OwnerInfo { get; set; }

    public MotorPolicyBillInfoDto? BillInfo { get; set; }
}

public class MotorPolicyOwnerInfoDto : IValidatableObject
{
    public bool IsCustomer { get; set; } = true;

    [StringLength(150)]
    public string? FullName { get; set; }

    [StringLength(20)]
    public string? Phone { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }

    [StringLength(150)]
    [EmailAddress]
    public string? Email { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!IsCustomer)
        {
            if (string.IsNullOrWhiteSpace(FullName))
                yield return new ValidationResult(
                    "FullName is required when IsCustomer is false.",
                    new[] { nameof(FullName) });

            if (string.IsNullOrWhiteSpace(Phone))
                yield return new ValidationResult(
                    "Phone is required when IsCustomer is false.",
                    new[] { nameof(Phone) });

            if (string.IsNullOrWhiteSpace(Address))
                yield return new ValidationResult(
                    "Address is required when IsCustomer is false.",
                    new[] { nameof(Address) });
        }
    }
}

public class MotorPolicyBillInfoDto
{
    public bool IsChoice { get; set; }

    [StringLength(250)]
    public string? BillActor { get; set; }

    [StringLength(50)]
    [EmailAddress]
    public string? Email { get; set; }
}
