using System;
using iOne.ProTableRates;
using Volo.Abp.Application.Dtos;

namespace iOne.Product.ProTableRates;

public class GetProTableRatesInput : PagedAndSortedResultRequestDto
{
    public string? Code { get; set; }

    public string? Name { get; set; }

    public ProTableRateStatus? Status { get; set; }

    public Guid? LobId { get; set; }

    public Guid? InsurerId { get; set; }

    public Guid? ProductId { get; set; }
}
