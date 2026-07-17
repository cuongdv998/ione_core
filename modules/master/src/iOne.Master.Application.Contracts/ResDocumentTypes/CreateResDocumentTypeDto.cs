using System.ComponentModel.DataAnnotations;
using iOne.ResDocumentTypes;

namespace iOne.Master.ResDocumentTypes;

public class CreateResDocumentTypeDto
{
    [Required(ErrorMessage = "Master::ResDocumentType:CodeRequired")]
    [StringLength(25, ErrorMessage = "Master::ResDocumentType:CodeMaxLength")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "Master::ResDocumentType:CodeInvalid")]
    [Display(Name = "Master::ResDocumentType:Code")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "Master::ResDocumentType:NameRequired")]
    [StringLength(250, ErrorMessage = "Master::ResDocumentType:NameMaxLength")]
    [Display(Name = "Master::ResDocumentType:Name")]
    public string Name { get; set; } = null!;

    [StringLength(500, ErrorMessage = "Master::ResDocumentType:DescriptionMaxLength")]
    [Display(Name = "Master::ResDocumentType:Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Master::ResDocumentType:StatusRequired")]
    [Display(Name = "Master::ResDocumentType:Status")]
    public ResDocumentTypeStatus Status { get; set; } = ResDocumentTypeStatus.Active;

    [Required(ErrorMessage = "Master::ResDocumentType:BucketRequired")]
    [StringLength(50, ErrorMessage = "Master::ResDocumentType:BucketMaxLength")]
    [RegularExpression(@"^[a-z0-9][a-z0-9\-\.]{1,61}[a-z0-9]$", ErrorMessage = "Master::ResDocumentType:BucketInvalid")]
    [Display(Name = "Master::ResDocumentType:Bucket")]
    public string Bucket { get; set; } = null!;

    [StringLength(50, ErrorMessage = "Master::ResDocumentType:DocumentGroupCodeMaxLength")]
    [Display(Name = "Master::ResDocumentType:DocumentGroupCode")]
    public string? DocumentGroupCode { get; set; }
}

