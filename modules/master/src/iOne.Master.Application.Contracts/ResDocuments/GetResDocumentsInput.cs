using System;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResDocuments;

public class GetResDocumentsInput : PagedAndSortedResultRequestDto
{
    public Guid? DocumentTypeId { get; set; }
    public string? GroupCode { get; set; }
}
