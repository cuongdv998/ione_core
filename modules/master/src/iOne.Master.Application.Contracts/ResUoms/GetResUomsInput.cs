using System;
using iOne.ResUoms;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResUoms;

public class GetResUomsInput : PagedAndSortedResultRequestDto
{
    public Guid? ClassId { get; set; }

    public string? Code { get; set; }

    public string? Name { get; set; }

    public ResUomStatus? Status { get; set; }

    public ResUomType? Type { get; set; }
}
