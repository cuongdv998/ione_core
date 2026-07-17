using System;
using iOne.ResProvinces;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResProvinces;

public class GetResProvincesInput : PagedAndSortedResultRequestDto
{
    public Guid? CountryId { get; set; }

    public string? Code { get; set; }

    public string? Name { get; set; }

    public ResProvinceStatus? Status { get; set; }
}

