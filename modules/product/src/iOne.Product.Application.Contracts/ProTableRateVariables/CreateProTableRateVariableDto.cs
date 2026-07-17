using System;
using System.ComponentModel.DataAnnotations;

namespace iOne.Product.ProTableRateVariables;

public class CreateProTableRateVariableDto
{
    [Required(ErrorMessage = "ProTableRateVariable:AttributeIdRequired")]
    [Display(Name = "ProTableRateVariable:AttributeId")]
    public Guid AttributeId { get; set; }

    [Required(ErrorMessage = "ProTableRateVariable:OperatorRequired")]
    [StringLength(15, ErrorMessage = "ProTableRateVariable:OperatorMaxLength")]
    [Display(Name = "ProTableRateVariable:Operator")]
    public string Operator { get; set; } = null!;
}
