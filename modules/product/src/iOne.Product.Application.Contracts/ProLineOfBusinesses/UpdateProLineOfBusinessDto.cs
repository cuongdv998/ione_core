using System;
using System.ComponentModel.DataAnnotations;
using iOne.ProLineOfBusinesses;

namespace iOne.Product.ProLineOfBusinesses;

public class UpdateProLineOfBusinessDto
{
    // ⚠️ QUAN TRỌNG: Không có Code - Code không được phép sửa

    [Display(Name = "ProLineOfBusiness:ParentId")]
    public Guid? ParentId { get; set; }

    [Required(ErrorMessage = "ProLineOfBusiness:NameRequired")]
    [StringLength(250, ErrorMessage = "ProLineOfBusiness:NameMaxLength")]
    [Display(Name = "ProLineOfBusiness:Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "ProLineOfBusiness:StatusRequired")]
    [Display(Name = "ProLineOfBusiness:Status")]
    public ProLineOfBusinessStatus Status { get; set; }

    [StringLength(500, ErrorMessage = "ProLineOfBusiness:DescriptionMaxLength")]
    [Display(Name = "ProLineOfBusiness:Description")]
    public string? Description { get; set; }
}




