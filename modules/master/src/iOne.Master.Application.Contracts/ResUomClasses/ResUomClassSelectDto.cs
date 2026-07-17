using System;
using System.ComponentModel.DataAnnotations;

namespace iOne.Master.ResUomClasses;

public class ResUomClassSelectDto
{
    [Display(Name = "ResUomClass:Id")]
    public Guid Id { get; set; }

    [Display(Name = "ResUomClass:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "ResUomClass:Name")]
    public string Name { get; set; } = null!;
}
