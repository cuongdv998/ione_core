using System;
using Volo.Abp.Application.Dtos;

namespace iOne.ApiKeys;

public class ApiKeyDto : EntityDto<Guid>
{
    public string Name { get; set; } = null!;
    public string Prefix { get; set; } = null!; // Only prefix is exposed, never the full key
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreationTime { get; set; }
}
