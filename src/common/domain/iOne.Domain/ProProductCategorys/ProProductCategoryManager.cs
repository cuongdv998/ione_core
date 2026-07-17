using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ProProductCategorys;

public class ProProductCategoryManager : DomainService
{
    protected IProProductCategoryRepository Repository { get; }
    protected ProLineOfBusinesses.IProLineOfBusinessRepository LobRepository { get; }

    public ProProductCategoryManager(
        IProProductCategoryRepository repository,
        ProLineOfBusinesses.IProLineOfBusinessRepository lobRepository)
    {
        Repository = repository;
        LobRepository = lobRepository;
    }

    public virtual async Task CreateAsync(ProProductCategory productCategory)
    {
        // Check code uniqueness
        if (await Repository.IsCodeExistsAsync(productCategory.Code))
        {
            throw new BusinessException("ProProductCategory:CodeExists")
                .WithData("Code", productCategory.Code);
        }

        // Validate LobId exists
        var lob = await LobRepository.FindAsync(productCategory.LobId);
        if (lob == null)
        {
            throw new BusinessException("ProProductCategory:LobNotFound")
                .WithData("LobId", productCategory.LobId);
        }

        // Validate ParentId if provided
        if (productCategory.ParentId.HasValue)
        {
            var parent = await Repository.FindAsync(productCategory.ParentId.Value);
            if (parent == null)
            {
                throw new BusinessException("ProProductCategory:ParentNotFound")
                    .WithData("ParentId", productCategory.ParentId.Value);
            }

            // Ensure parent belongs to the same LOB
            if (parent.LobId != productCategory.LobId)
            {
                throw new BusinessException("ProProductCategory:ParentLobMismatch");
            }
        }

        await Repository.InsertAsync(productCategory);
    }

    public virtual async Task UpdateAsync(
        ProProductCategory productCategory,
        Guid lobId,
        string name,
        ProProductCategoryStatus status,
        Guid? parentId = null,
        string? description = null)
    {
        // ⚠️ QUAN TRỌNG: Không có parameter code - Code không được phép sửa

        // Validate LobId exists
        var lob = await LobRepository.FindAsync(lobId);
        if (lob == null)
        {
            throw new BusinessException("ProProductCategory:LobNotFound")
                .WithData("LobId", lobId);
        }

        // Validate ParentId if provided
        if (parentId.HasValue)
        {
            // Cannot set self as parent (already validated in entity, but double-check)
            if (parentId.Value == productCategory.Id)
            {
                throw new BusinessException("ProProductCategory:CannotSetSelfAsParent");
            }

            // Check if parent exists
            var parent = await Repository.FindAsync(parentId.Value);
            if (parent == null)
            {
                throw new BusinessException("ProProductCategory:ParentNotFound")
                    .WithData("ParentId", parentId.Value);
            }

            // Ensure parent belongs to the same LOB
            if (parent.LobId != lobId)
            {
                throw new BusinessException("ProProductCategory:ParentLobMismatch");
            }

            // Prevent circular references: check if the new parent is a descendant of this entity
            if (await IsDescendantAsync(productCategory.Id, parentId.Value))
            {
                throw new BusinessException("ProProductCategory:CircularReferenceNotAllowed");
            }
        }

        productCategory.UpdateLobId(lobId);
        productCategory.UpdateName(name);
        productCategory.UpdateStatus(status);
        productCategory.UpdateDescription(description);
        productCategory.UpdateParentId(parentId);
        await Repository.UpdateAsync(productCategory);
    }

    /// <summary>
    /// Checks if targetId is a descendant of ancestorId (to prevent circular references)
    /// </summary>
    private async Task<bool> IsDescendantAsync(Guid ancestorId, Guid targetId)
    {
        var current = await Repository.FindAsync(targetId);
        if (current == null)
        {
            return false;
        }

        // Traverse up the hierarchy
        while (current.ParentId.HasValue)
        {
            if (current.ParentId.Value == ancestorId)
            {
                return true; // Found circular reference
            }

            current = await Repository.FindAsync(current.ParentId.Value);
            if (current == null)
            {
                break;
            }
        }

        return false;
    }
}
