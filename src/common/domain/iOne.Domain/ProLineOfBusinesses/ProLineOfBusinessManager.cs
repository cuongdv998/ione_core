using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.ProLineOfBusinesses;

public class ProLineOfBusinessManager : DomainService
{
    protected IProLineOfBusinessRepository Repository { get; }

    public ProLineOfBusinessManager(IProLineOfBusinessRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(ProLineOfBusiness lineOfBusiness)
    {
        // Check code uniqueness
        if (await Repository.IsCodeExistsAsync(lineOfBusiness.Code))
        {
            throw new BusinessException("ProLineOfBusiness:CodeExists")
                .WithData("Code", lineOfBusiness.Code);
        }

        // Validate ParentId if provided
        if (lineOfBusiness.ParentId.HasValue)
        {
            var parent = await Repository.FindAsync(lineOfBusiness.ParentId.Value);
            if (parent == null)
            {
                throw new BusinessException("ProLineOfBusiness:ParentNotFound")
                    .WithData("ParentId", lineOfBusiness.ParentId.Value);
            }
        }

        await Repository.InsertAsync(lineOfBusiness);
    }

    public virtual async Task UpdateAsync(
        ProLineOfBusiness lineOfBusiness,
        string name,
        ProLineOfBusinessStatus status,
        Guid? parentId = null,
        string? description = null)
    {
        // ⚠️ QUAN TRỌNG: Không có parameter code - Code không được phép sửa

        // Validate ParentId if provided
        if (parentId.HasValue)
        {
            // Cannot set self as parent (already validated in entity, but double-check)
            if (parentId.Value == lineOfBusiness.Id)
            {
                throw new BusinessException("ProLineOfBusiness:CannotSetSelfAsParent");
            }

            // Check if parent exists
            var parent = await Repository.FindAsync(parentId.Value);
            if (parent == null)
            {
                throw new BusinessException("ProLineOfBusiness:ParentNotFound")
                    .WithData("ParentId", parentId.Value);
            }

            // Prevent circular references: check if the new parent is a descendant of this entity
            if (await IsDescendantAsync(lineOfBusiness.Id, parentId.Value))
            {
                throw new BusinessException("ProLineOfBusiness:CircularReferenceNotAllowed");
            }
        }

        lineOfBusiness.UpdateName(name);
        lineOfBusiness.UpdateStatus(status);
        lineOfBusiness.UpdateDescription(description);
        lineOfBusiness.UpdateParentId(parentId);
        await Repository.UpdateAsync(lineOfBusiness);
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




