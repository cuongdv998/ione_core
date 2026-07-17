using System;
using System.Threading.Tasks;
using iOne.Localization;
using Microsoft.Extensions.Localization;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ProProducts;

public class ProProductManager : DomainService
{
    protected IProProductRepository Repository { get; }
    protected IStringLocalizer<iOneResource> L { get; }

    public ProProductManager(
        IProProductRepository repository,
        IStringLocalizer<iOneResource> localizer)
    {
        Repository = repository;
        L = localizer;
    }

    public virtual async Task CreateAsync(ProProduct product)
    {
        // Check code uniqueness
        if (await Repository.IsCodeExistsAsync(product.Code))
        {
            throw new UserFriendlyException(
                L["Product:ProProduct:CodeExists"].Value.Replace("{Code}", product.Code));
        }

        await Repository.InsertAsync(product);
    }

    public virtual async Task UpdateAsync(
        ProProduct product,
        Guid? productTypeId,
        Guid? partnerId,
        Guid? tableRateId,
        Guid? rootProductId,
        string isRootProduct,
        Guid lobId,
        Guid? productCategoryId,
        Guid? currencyId,
        string shortName,
        string name,
        string? description,
        string? internalNote,
        string? insurerProductCode,
        string rateType,
        ProProductStatus status,
        int seqNumber,
        DateTime effectDate,
        DateTime? expireDate,
        Guid? planDefinitionId,
        string? isPlan,
        Guid? imageDocumentId = null,
        Guid? certificateTemplateDocumentId = null)
    {
        product.UpdateProductTypeId(productTypeId);
        product.UpdatePartnerId(partnerId);
        product.UpdateTableRateId(tableRateId);
        product.UpdateRootProductId(rootProductId);
        product.UpdateIsRootProduct(isRootProduct);
        product.UpdateLobId(lobId);
        product.UpdateProductCategoryId(productCategoryId);
        product.UpdateCurrencyId(currencyId);
        product.UpdateShortName(shortName);
        product.UpdateName(name);
        product.UpdateDescription(description);
        product.UpdateInternalNote(internalNote);
        product.UpdateInsurerProductCode(insurerProductCode);
        product.UpdateRateType(rateType);
        product.UpdateStatus(status);
        product.UpdateSeqNumber(seqNumber);
        product.UpdateEffectDate(effectDate);
        product.UpdateExpireDate(expireDate);
        product.UpdatePlanDefinitionId(planDefinitionId);
        product.UpdateIsPlan(isPlan);
        product.UpdateImageDocumentId(imageDocumentId);
        product.UpdateCertificateTemplateDocumentId(certificateTemplateDocumentId);
        await Repository.UpdateAsync(product);
    }
}
