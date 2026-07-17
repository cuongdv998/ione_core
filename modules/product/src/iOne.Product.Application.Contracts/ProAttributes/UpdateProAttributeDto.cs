using System.ComponentModel.DataAnnotations;
using iOne.ProAttributes;

namespace iOne.Product.ProAttributes;

public class UpdateProAttributeDto
{
    [Required]
    [MaxLength(250)]
    [Display(Name = "ProAttribute:Name")]
    public string Name { get; set; } = null!;

    [Required]
    [Display(Name = "ProAttribute:Status")]
    public ProAttributeStatus Status { get; set; }

    [Required]
    [Display(Name = "ProAttribute:Spec")]
    public ProAttributeSpec Spec { get; set; }

    [MaxLength(250)]
    [Display(Name = "ProAttribute:Description")]
    public string? Description { get; set; }

    [MaxLength(50)]
    [Display(Name = "ProAttribute:DataPath")]
    public string? DataPath { get; set; }

    [Required]
    [Display(Name = "ProAttribute:DataType")]
    public ProAttributeDataType DataType { get; set; }

    [Display(Name = "ProAttribute:ComputeScript")]
    public string? ComputeScript { get; set; }

    [MaxLength(1000)]
    [Display(Name = "ProAttribute:ClearDataScript")]
    public string? ClearDataScript { get; set; }
}
