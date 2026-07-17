using System.ComponentModel.DataAnnotations;
using iOne.ResDocumentTypes;

namespace iOne.Master.ResDocumentTypes;

public class UpdateResDocumentTypeDto
{
    // ⚠️ QUAN TRỌNG: Không có Code property - Code không được phép sửa

    [Required(ErrorMessage = "Master::ResDocumentType:NameRequired")]
    [StringLength(250, ErrorMessage = "Master::ResDocumentType:NameMaxLength")]
    [Display(Name = "Master::ResDocumentType:Name")]
    public string Name { get; set; } = null!;

    [StringLength(500, ErrorMessage = "Master::ResDocumentType:DescriptionMaxLength")]
    [Display(Name = "Master::ResDocumentType:Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Master::ResDocumentType:StatusRequired")]
    [Display(Name = "Master::ResDocumentType:Status")]
    public ResDocumentTypeStatus Status { get; set; }

    [StringLength(50, ErrorMessage = "Master::ResDocumentType:BucketMaxLength")]
    [Display(Name = "Master::ResDocumentType:Bucket")]
    public string? Bucket { get; set; }

    [StringLength(50, ErrorMessage = "Master::ResDocumentType:DocumentGroupCodeMaxLength")]
    [Display(Name = "Master::ResDocumentType:DocumentGroupCode")]
    public string? DocumentGroupCode { get; set; }
}

