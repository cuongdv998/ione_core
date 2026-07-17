using System.ComponentModel.DataAnnotations;
using iOne.ResIndustries;

namespace iOne.Customer.ResIndustries;

public class UpdateResIndustryDto
{
    // ⚠️ QUAN TRỌNG: KHÔNG có Code - Code là immutable

    [Required]
    [MaxLength(250)]
    public string Name { get; set; } = null!;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public ResIndustryStatus Status { get; set; }
}


