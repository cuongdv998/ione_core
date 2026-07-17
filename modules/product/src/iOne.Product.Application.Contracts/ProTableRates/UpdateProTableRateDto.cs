using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using iOne.Product.ProTableRateVariables;
using iOne.ProTableRates;

namespace iOne.Product.ProTableRates;

public class UpdateProTableRateDto
{
    // ⚠️ QUAN TRỌNG: Không có Code - Code không được phép sửa

    [Required(ErrorMessage = "ProTableRate:LobIdRequired")]
    [Display(Name = "ProTableRate:LobId")]
    public Guid LobId { get; set; }

    [Display(Name = "ProTableRate:InsurerId")]
    public Guid? InsurerId { get; set; }

    [Required(ErrorMessage = "ProTableRate:NameRequired")]
    [StringLength(250, ErrorMessage = "ProTableRate:NameMaxLength")]
    [Display(Name = "ProTableRate:Name")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "ProTableRate:StatusRequired")]
    [Display(Name = "ProTableRate:Status")]
    public ProTableRateStatus Status { get; set; }

    [StringLength(500, ErrorMessage = "ProTableRate:DescriptionMaxLength")]
    [Display(Name = "ProTableRate:Description")]
    public string? Description { get; set; }

    [Display(Name = "ProTableRate:Variables")]
    public List<UpdateProTableRateVariableDto> Variables { get; set; } = new List<UpdateProTableRateVariableDto>();
}
