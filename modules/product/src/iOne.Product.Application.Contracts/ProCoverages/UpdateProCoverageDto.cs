using System;
using System.ComponentModel.DataAnnotations;
using iOne.ProCoverages;

namespace iOne.Product.ProCoverages;

public class UpdateProCoverageDto
{
    [Required(ErrorMessage = "ProCoverage:LobIdRequired")]
    [Display(Name = "ProCoverage:LobId")]
    public Guid LobId { get; set; }

    [Display(Name = "ProCoverage:ObjectTypeId")]
    public Guid? ObjectTypeId { get; set; }

    [Required(ErrorMessage = "ProCoverage:CoverageGroupIdRequired")]
    [Display(Name = "ProCoverage:CoverageGroupId")]
    public Guid CoverageGroupId { get; set; }

    [Display(Name = "ProCoverage:CoverageTypeId")]
    public Guid? CoverageTypeId { get; set; }

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
