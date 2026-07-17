using System;
using System.ComponentModel.DataAnnotations;
using iOne.ProCoverages;

namespace iOne.Product.ProCoverages;

public class CreateProCoverageDto
{
    [Required(ErrorMessage = "ProCoverage:LobIdRequired")]
    [Display(Name = "ProCoverage:LobId")]
    public Guid LobId { get; set; }

    [Required(ErrorMessage = "ProCoverage:ObjectTypeIdRequired")]
    [Display(Name = "ProCoverage:ObjectTypeId")]
    public Guid? ObjectTypeId { get; set; }

    [Required(ErrorMessage = "ProCoverage:CoverageGroupIdRequired")]
    [Display(Name = "ProCoverage:CoverageGroupId")]
    public Guid CoverageGroupId { get; set; }

    [Required(ErrorMessage = "ProCoverage:CoverageTypeIdRequired")]
    [Display(Name = "ProCoverage:CoverageTypeId")]
    public Guid? CoverageTypeId { get; set; }

    [Required(ErrorMessage = "ProCoverage:CodeRequired")]
    [MaxLength(50, ErrorMessage = "ProCoverage:CodeMaxLength")]
    [Display(Name = "ProCoverage:Code")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "ProCoverage:ShortNameRequired")]
    [MaxLength(50, ErrorMessage = "ProCoverage:ShortNameMaxLength")]
    [Display(Name = "ProCoverage:ShortName")]
    public string ShortName { get; set; } = null!;

    [Required(ErrorMessage = "ProCoverage:NameRequired")]
    [MaxLength(250, ErrorMessage = "ProCoverage:NameMaxLength")]
    [Display(Name = "ProCoverage:Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "ProCoverage:TypeRequired")]
    [Display(Name = "ProCoverage:Type")]
    public ProCoverageTermType Type { get; set; }

    [MaxLength(500, ErrorMessage = "ProCoverage:DescriptionMaxLength")]
    [Display(Name = "ProCoverage:Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "ProCoverage:StatusRequired")]
    [Display(Name = "ProCoverage:Status")]
    public ProCoverageStatus Status { get; set; }
}
