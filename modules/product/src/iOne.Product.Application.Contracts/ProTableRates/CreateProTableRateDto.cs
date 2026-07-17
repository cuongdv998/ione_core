using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using iOne.Product.ProTableRateVariables;
using iOne.ProTableRates;

namespace iOne.Product.ProTableRates;

public class CreateProTableRateDto
{
    [Required(ErrorMessage = "ProTableRate:LobIdRequired")]
    [Display(Name = "ProTableRate:LobId")]
    public Guid LobId { get; set; }

    [Display(Name = "ProTableRate:InsurerId")]
    public Guid? InsurerId { get; set; }

    [Required(ErrorMessage = "ProTableRate:CodeRequired")]
    [StringLength(50, ErrorMessage = "ProTableRate:CodeMaxLength")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "ProTableRate:CodeInvalidFormat")]
    [Display(Name = "ProTableRate:Code")]
    public string Code { get; set; } = null!;

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
    public List<CreateProTableRateVariableDto> Variables { get; set; } = new List<CreateProTableRateVariableDto>();
}
