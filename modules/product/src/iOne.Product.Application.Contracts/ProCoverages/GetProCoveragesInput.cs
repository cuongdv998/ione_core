using System;
using iOne.ProCoverages;
using Volo.Abp.Application.Dtos;

namespace iOne.Product.ProCoverages;

public class GetProCoveragesInput : PagedAndSortedResultRequestDto
{
    public Guid? LobId { get; set; }

    public Guid? ObjectTypeId { get; set; }

    public Guid? CoverageGroupId { get; set; }

    public Guid? CoverageTypeId { get; set; }

    public ProCoverageTermType? Type { get; set; }

    public string? Code { get; set; }

    public string? Name { get; set; }

    public ProCoverageStatus? Status { get; set; }
}
