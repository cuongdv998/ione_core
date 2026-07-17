using System;
using iOne.ProProductCategorys;
using Volo.Abp.Application.Dtos;

namespace iOne.Product.ProProductCategorys;

public class GetProProductCategorysInput : PagedAndSortedResultRequestDto
{
    public Guid? LobId { get; set; }

    public string? Code { get; set; }

    public string? Name { get; set; }

    public ProProductCategoryStatus? Status { get; set; }

    public Guid? ParentId { get; set; }
}
