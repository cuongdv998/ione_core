using System;
using iOne.ProLineOfBusinesses;
using Volo.Abp.Application.Dtos;

namespace iOne.Product.ProLineOfBusinesses;

public class GetProLineOfBusinessesInput : PagedAndSortedResultRequestDto
{
    public string? Code { get; set; }

    public string? Name { get; set; }

    public ProLineOfBusinessStatus? Status { get; set; }

    public Guid? ParentId { get; set; }
}




