using System;
using System.ComponentModel.DataAnnotations;

namespace iOne.Partner.ResPartners;

public class ResPartnerSelectDto
{
    [Display(Name = "Partner:ResPartner:Id")]
    public Guid Id { get; set; }

    [Display(Name = "Partner:ResPartner:Code")]
    public string? Code { get; set; }

    [Display(Name = "Partner:ResPartner:Name")]
    public string Name { get; set; } = null!;
}
