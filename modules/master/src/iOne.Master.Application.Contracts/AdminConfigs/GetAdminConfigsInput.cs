using iOne.AdminConfigs;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.AdminConfigs;

public class GetAdminConfigsInput : PagedAndSortedResultRequestDto
{
    public string? Code { get; set; }
    public string? SubCode { get; set; }
    public string? Name { get; set; }
    public AdminConfigStatus? Status { get; set; }
}

