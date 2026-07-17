using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResCountries;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResCountries;

public class ResCountryDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ResCountry:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "ResCountry:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "ResCountry:Status")]
    public ResCountryStatus Status { get; set; }
}

