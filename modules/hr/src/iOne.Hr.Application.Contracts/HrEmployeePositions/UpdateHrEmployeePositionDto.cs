using System.ComponentModel.DataAnnotations;
using iOne.HrEmployeePositions;

namespace iOne.Hr.HrEmployeePositions;

public class UpdateHrEmployeePositionDto
{
    [Required(ErrorMessage = "HrEmployeePosition:NameRequired")]
    [StringLength(250, ErrorMessage = "HrEmployeePosition:NameMaxLength")]
    [Display(Name = "HrEmployeePosition:Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "HrEmployeePosition:TypeRequired")]
    [Display(Name = "HrEmployeePosition:Type")]
    public HrEmployeePositionType Type { get; set; }

    [Required(ErrorMessage = "HrEmployeePosition:StatusRequired")]
    [Display(Name = "HrEmployeePosition:Status")]
    public HrEmployeePositionStatus Status { get; set; }
}

