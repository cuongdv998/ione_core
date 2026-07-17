using System;
using System.ComponentModel.DataAnnotations;

namespace iOne.Product.ProLineOfBusinesses;

public class ProLineOfBusinessSelectDto
{
    [Display(Name = "ProLineOfBusiness:Id")]
    public Guid Id { get; set; }

    [Display(Name = "ProLineOfBusiness:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "ProLineOfBusiness:Name")]
    public string Name { get; set; } = null!;
}
