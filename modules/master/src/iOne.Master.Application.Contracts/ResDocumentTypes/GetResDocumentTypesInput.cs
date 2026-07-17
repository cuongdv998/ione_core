using iOne.ResDocumentTypes;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResDocumentTypes;

public class GetResDocumentTypesInput : PagedAndSortedResultRequestDto
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public ResDocumentTypeStatus? Status { get; set; }
}

