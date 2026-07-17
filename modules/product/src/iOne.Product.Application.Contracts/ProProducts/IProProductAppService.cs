using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace iOne.Product.ProProducts;

public interface IProProductAppService : ICrudAppService<
    ProProductDto,
    Guid,
    GetProProductsInput,
    CreateProProductDto,
    UpdateProProductDto>
{
    /// <summary>
    /// Returns a lightweight product list for tree/hierarchy display (no nested attributes, coverages, etc.).
    /// </summary>
    Task<PagedResultDto<ProProductListDto>> GetTreeListAsync(GetProProductsInput input);

    /// <param name="applyChannelDistributionFilter">
    /// When true (default), requires <paramref name="channelId"/> and filters products by active <c>pro_product_distribution</c> for that channel,
    /// and by employee role: a product must have a channel distribution row for the given channel and at least one role-only
    /// distribution row matching a currently valid <c>hr_employee_role_rel</c> for the current user.
    /// When false, returns all active products for the LOB + partner (no channel or role filter). Used e.g. for policy import Excel template.
    /// </param>
    /// <param name="insurerCode">
    /// Mã đối tác (res_partner.code). Chỉ dùng khi <paramref name="partnerId"/> rỗng: tra Id đối tác theo mã rồi lọc sản phẩm như cũ.
    /// </param>
    Task<List<ProProductDto>> GetByLobIdAndPartnerIdAsync(
        Guid lobId,
        Guid partnerId,
        Guid? channelId = null,
        Guid? appChannelId = null,
        bool applyChannelDistributionFilter = true,
        string? insurerCode = null);

    Task<List<ProProductDto>> GetByRootProductIdAndPlanDefinitionIdAsync(Guid rootProductId, Guid planDefinitionId);

    /// <summary>
    /// Load deductible (mức miễn thường) options per product coverage for the given product IDs.
    /// Used by policy create/update to avoid embedding full level/term data in by-lob-partner response.
    /// </summary>
    Task<List<DeductibleOptionsForCoverageDto>> GetDeductibleOptionsAsync(GetDeductibleOptionsInput input);
}
