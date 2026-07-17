using System;
using iOne.ProProducts;
using Volo.Abp.Application.Dtos;

namespace iOne.Product.ProProducts;

/// <summary>
/// Lightweight product DTO for tree/hierarchy listing. Excludes nested collections (attributes, coverages, etc.).
/// </summary>
public class ProProductListDto : EntityDto<Guid>
{
    public Guid? ProductTypeId { get; set; }

    public string? ProductTypeCode { get; set; }

    public Guid? PartnerId { get; set; }

    public Guid? RootProductId { get; set; }

    public string IsRootProduct { get; set; } = "Y";

    public Guid LobId { get; set; }

    public Guid? ProductCategoryId { get; set; }

    public string Code { get; set; } = null!;

    public string? InsurerProductCode { get; set; }

    public string ShortName { get; set; } = null!;

    public string Name { get; set; } = null!;

    public ProProductStatus Status { get; set; }

    public int SeqNumber { get; set; } = 1;

    public DateTime EffectDate { get; set; }

    public DateTime? ExpireDate { get; set; }

    public string? IsPlan { get; set; }
}
