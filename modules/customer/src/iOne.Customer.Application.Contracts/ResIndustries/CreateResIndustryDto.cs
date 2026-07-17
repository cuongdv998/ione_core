using System.ComponentModel.DataAnnotations;
using iOne.ResIndustries;

namespace iOne.Customer.ResIndustries;

public class CreateResIndustryDto
{
    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = null!;

    [Required]
    [MaxLength(250)]
    public string Name { get; set; } = null!;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public ResIndustryStatus Status { get; set; }
}


