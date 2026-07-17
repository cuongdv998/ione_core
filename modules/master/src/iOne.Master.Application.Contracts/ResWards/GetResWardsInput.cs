using System;
using iOne.ResWards;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResWards;

public class GetResWardsInput : PagedAndSortedResultRequestDto
{
    public Guid? ProvinceId { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public ResWardStatus? Status { get; set; }
}

