using System;
using iOne.ProProducts;
using Volo.Abp.Application.Dtos;

namespace iOne.Product.ProProducts;

public class GetProProductsInput : PagedAndSortedResultRequestDto
{
    public Guid? ProductTypeId { get; set; }

    public Guid? PartnerId { get; set; }

    public Guid? LobId { get; set; }

    public Guid? ProductCategoryId { get; set; }

    public Guid? CurrencyId { get; set; }

    public string? Code { get; set; }

    public string? Name { get; set; }

    public string? ShortName { get; set; }

    public ProProductStatus? Status { get; set; }
}
