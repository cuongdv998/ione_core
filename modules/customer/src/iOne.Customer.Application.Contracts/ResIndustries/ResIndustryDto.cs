using System;
using Volo.Abp.Application.Dtos;
using iOne.ResIndustries;

namespace iOne.Customer.ResIndustries;

public class ResIndustryDto : FullAuditedEntityDto<Guid>
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public ResIndustryStatus Status { get; set; }
}

