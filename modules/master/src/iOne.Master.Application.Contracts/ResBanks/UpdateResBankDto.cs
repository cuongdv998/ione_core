using System.ComponentModel.DataAnnotations;
using iOne.ResBanks;

namespace iOne.Master.ResBanks;

public class UpdateResBankDto
{
    // ⚠️ QUAN TRỌNG: Không có Code property - Code không được phép sửa

    [Required(ErrorMessage = "ResBank:NameRequired")]
    [StringLength(250, ErrorMessage = "ResBank:NameMaxLength")]
    [Display(Name = "ResBank:Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "ResBank:StatusRequired")]
    [Display(Name = "ResBank:Status")]
    public ResBankStatus Status { get; set; }
}

