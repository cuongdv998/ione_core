using System;
using iOne.ProProductTypes;
using Volo.Abp.Application.Dtos;

namespace iOne.Product.ProProductTypes;

public class GetProProductTypesInput : PagedAndSortedResultRequestDto
{
    public Guid? LobId { get; set; }

    public string? Code { get; set; }

    public string? Name { get; set; }

    public ProProductTypeStatus? Status { get; set; }
}
