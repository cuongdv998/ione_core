using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using iOne.HrDepartmentTypes;
using iOne.ResPartners;
using iOne.ResPartnerTypes;
using iOne.ResProvinces;
using iOne.ResWards;
using iOne.ResBanks;

namespace iOne.HrDepartments;

public class HrDepartmentManager : DomainService
{
    protected IHrDepartmentRepository Repository { get; }
    protected IResPartnerRepository PartnerRepository { get; }
    protected IResPartnerTypeRepository PartnerTypeRepository { get; }
    protected IRepository<ResProvince, Guid> ProvinceRepository { get; }
    protected IRepository<ResWard, Guid> WardRepository { get; }
    protected IRepository<HrDepartmentType, Guid> DepartmentTypeRepository { get; }
    protected IResBankRepository BankRepository { get; }
    protected ResPartnerManager PartnerManager { get; }
    protected ResPartnerTypeManager PartnerTypeManager { get; }

    public HrDepartmentManager(
        IHrDepartmentRepository repository,
        IResPartnerRepository partnerRepository,
        IResPartnerTypeRepository partnerTypeRepository,
        IRepository<ResProvince, Guid> provinceRepository,
        IRepository<ResWard, Guid> wardRepository,
        IRepository<HrDepartmentType, Guid> departmentTypeRepository,
        IResBankRepository bankRepository,
        ResPartnerManager partnerManager,
        ResPartnerTypeManager partnerTypeManager)
    {
        Repository = repository;
        PartnerRepository = partnerRepository;
        PartnerTypeRepository = partnerTypeRepository;
        ProvinceRepository = provinceRepository;
        WardRepository = wardRepository;
        DepartmentTypeRepository = departmentTypeRepository;
        BankRepository = bankRepository;
        PartnerManager = partnerManager;
        PartnerTypeManager = partnerTypeManager;
    }

    public virtual async Task CreateAsync(HrDepartment department)
    {
        // Check code uniqueness
        if (await Repository.IsCodeExistsAsync(department.Code))
        {
            throw new BusinessException("Hr:HrDepartment:CodeExists")
                .WithData("Code", department.Code);
        }

        // Validate parent exists (if provided)
        if (department.ParentId.HasValue)
        {
            var parent = await Repository.FirstOrDefaultAsync(x => x.Id == department.ParentId.Value);
            if (parent == null)
            {
                throw new BusinessException("Hr:HrDepartment:ParentNotFound");
            }
        }

        // Validate org exists (if provided)
        if (department.OrgId.HasValue)
        {
            var org = await Repository.FirstOrDefaultAsync(x => x.Id == department.OrgId.Value);
            if (org == null)
            {
                throw new BusinessException("Hr:HrDepartment:OrgNotFound");
            }
        }

        // Validate type exists (if provided)
        if (department.TypeId.HasValue)
        {
            var type = await DepartmentTypeRepository.FirstOrDefaultAsync(x => x.Id == department.TypeId.Value);
            if (type == null)
            {
                throw new BusinessException("Hr:HrDepartment:TypeNotFound");
            }
        }

        // Validate province exists (if provided)
        if (department.ProvinceId.HasValue)
        {
            var province = await ProvinceRepository.FirstOrDefaultAsync(x => x.Id == department.ProvinceId.Value);
            if (province == null)
            {
                throw new BusinessException("Hr:HrDepartment:ProvinceNotFound");
            }
        }

        // Validate ward exists (if provided)
        if (department.WardId.HasValue)
        {
            var ward = await WardRepository.FirstOrDefaultAsync(x => x.Id == department.WardId.Value);
            if (ward == null)
            {
                throw new BusinessException("Hr:HrDepartment:WardNotFound");
            }
        }

        // Validate bank exists (if provided)
        if (department.BankId.HasValue)
        {
            var bank = await BankRepository.FirstOrDefaultAsync(x => x.Id == department.BankId.Value);
            if (bank == null)
            {
                throw new BusinessException("Hr:HrDepartment:BankNotFound");
            }
        }

        // Note: Partner validation is skipped here because partner is created in the same transaction
        // via CreatePartnerForDepartmentAsync. If partner doesn't exist, foreign key constraint will fail.
        // Insert department (Partner should already be created in the same transaction)
        await Repository.InsertAsync(department);
    }

    public virtual async Task UpdateAsync(
        HrDepartment department,
        string name,
        string? description,
        HrDepartmentStatus status,
        HrDepartmentLevel deptLevel,
        Guid? parentId,
        Guid? orgId,
        Guid? typeId,
        Guid? provinceId,
        Guid? wardId,
        string? address,
        string? fullAddress,
        Guid? bankId,
        string? bankNo)
    {
        // ⚠️ QUAN TRỌNG: Không có parameter code - Code không được phép sửa

        // Validate parent exists (if provided)
        if (parentId.HasValue)
        {
            var parent = await Repository.FirstOrDefaultAsync(x => x.Id == parentId.Value);
            if (parent == null)
            {
                throw new BusinessException("Hr:HrDepartment:ParentNotFound");
            }

            // Check circular reference: parent cannot be itself
            if (parentId.Value == department.Id)
            {
                throw new BusinessException("Hr:HrDepartment:ParentCannotBeItself");
            }

            // Only check circular reference if parent is being changed
            // If parentId is the same as current ParentId, skip the check
            if (parentId.Value != department.ParentId)
            {
                // Check circular reference: parent cannot be a descendant
                if (await IsDescendantOfAsync(department.Id, parentId.Value))
                {
                    throw new BusinessException("Hr:HrDepartment:ParentCannotBeDescendant");
                }
            }
        }

        // Validate org exists (if provided)
        if (orgId.HasValue)
        {
            var org = await Repository.FirstOrDefaultAsync(x => x.Id == orgId.Value);
            if (org == null)
            {
                throw new BusinessException("Hr:HrDepartment:OrgNotFound");
            }

            // Check circular reference: org cannot be itself
            if (orgId.Value == department.Id)
            {
                throw new BusinessException("Hr:HrDepartment:OrgCannotBeItself");
            }

            // Only check circular reference if org is being changed
            // If orgId is the same as current OrgId, skip the check
            if (orgId.Value != department.OrgId)
            {
                // Check circular reference: org cannot be a descendant
                if (await IsDescendantOfAsync(department.Id, orgId.Value))
                {
                    throw new BusinessException("Hr:HrDepartment:OrgCannotBeDescendant");
                }
            }
        }

        // Validate type exists (if provided)
        if (typeId.HasValue)
        {
            var type = await DepartmentTypeRepository.FirstOrDefaultAsync(x => x.Id == typeId.Value);
            if (type == null)
            {
                throw new BusinessException("Hr:HrDepartment:TypeNotFound");
            }
        }

        // Validate province exists (if provided)
        if (provinceId.HasValue)
        {
            var province = await ProvinceRepository.FirstOrDefaultAsync(x => x.Id == provinceId.Value);
            if (province == null)
            {
                throw new BusinessException("Hr:HrDepartment:ProvinceNotFound");
            }
        }

        // Validate ward exists (if provided)
        if (wardId.HasValue)
        {
            var ward = await WardRepository.FirstOrDefaultAsync(x => x.Id == wardId.Value);
            if (ward == null)
            {
                throw new BusinessException("Hr:HrDepartment:WardNotFound");
            }
        }

        // Validate bank exists (if provided)
        if (bankId.HasValue)
        {
            var bank = await BankRepository.FirstOrDefaultAsync(x => x.Id == bankId.Value);
            if (bank == null)
            {
                throw new BusinessException("Hr:HrDepartment:BankNotFound");
            }
        }

        // Update department fields
        department.UpdateName(name);
        department.UpdateDescription(description);
        department.UpdateStatus(status);
        department.UpdateDeptLevel(deptLevel);
        department.UpdateParentId(parentId);
        department.UpdateOrgId(orgId);
        department.UpdateTypeId(typeId);
        department.UpdateProvinceId(provinceId);
        department.UpdateWardId(wardId);
        department.UpdateAddress(address);
        department.UpdateFullAddress(fullAddress);
        department.UpdateBankId(bankId);
        department.UpdateBankNo(bankNo);

        await Repository.UpdateAsync(department);
    }

    /// <summary>
    /// Create Partner for Department with PartnerType code = "DIRECT"
    /// </summary>
    public virtual async Task<Guid> CreatePartnerForDepartmentAsync(
        string departmentCode,
        string departmentName,
        Guid? provinceId,
        Guid? wardId,
        string? address)
    {
        // Find or create ResPartnerType with code = "DIRECT"
        var partnerType = await PartnerTypeRepository.FirstOrDefaultAsync(x => x.Code == "DIRECT");
        if (partnerType == null)
        {
            // Auto-create "DIRECT" partner type if it doesn't exist
            partnerType = new ResPartnerType(
                GuidGenerator.Create(),
                "DIRECT",
                "Trực tiếp",
                ResPartnerTypeStatus.Active
            );
            await PartnerTypeManager.CreateAsync(partnerType);
        }

        // Load Province and Ward if provided
        string? provinceName = null;
        string? wardName = null;

        if (provinceId.HasValue)
        {
            var province = await ProvinceRepository.FirstOrDefaultAsync(x => x.Id == provinceId.Value);
            if (province != null)
            {
                provinceName = province.Name;
            }
        }

        if (wardId.HasValue)
        {
            var ward = await WardRepository.FirstOrDefaultAsync(x => x.Id == wardId.Value);
            if (ward != null)
            {
                wardName = ward.Name;
            }
        }

        // Compute FullAddress
        var addressValue = address ?? "";
        var fullAddress = PartnerManager.ComputeFullAddress(addressValue, wardName ?? "", provinceName ?? "");

        // Partner requires ProvinceId and WardId - use provided values or throw exception
        if (!provinceId.HasValue || !wardId.HasValue)
        {
            throw new BusinessException("Hr:HrDepartment:ProvinceAndWardRequiredForPartner")
                .WithData("Reason", "Partner requires ProvinceId and WardId, but Department does not have them");
        }

        // Create ResPartner with department code
        var partner = new ResPartner(
            GuidGenerator.Create(),
            partnerType.Id,
            provinceId.Value,
            wardId.Value,
            departmentName,
            addressValue,
            fullAddress,
            "0000000000", // Default phone (required field)
            ResPartnerStatus.Active,
            code: departmentCode, // Use department code as partner code
            email: null,
            note: null
        );

        await PartnerManager.CreateAsync(partner);

        return partner.Id;
    }

    /// <summary>
    /// Check if departmentId is a descendant of ancestorId (recursive check)
    /// </summary>
    private async Task<bool> IsDescendantOfAsync(Guid departmentId, Guid ancestorId)
    {
        var current = await Repository.FirstOrDefaultAsync(x => x.Id == departmentId);
        if (current == null)
        {
            return false;
        }

        // Traverse up the tree
        var visited = new System.Collections.Generic.HashSet<Guid>();
        while (current.ParentId.HasValue)
        {
            if (visited.Contains(current.Id))
            {
                // Circular reference detected
                break;
            }

            visited.Add(current.Id);

            if (current.ParentId.Value == ancestorId)
            {
                return true;
            }

            current = await Repository.FirstOrDefaultAsync(x => x.Id == current.ParentId.Value);
            if (current == null)
            {
                break;
            }
        }

        return false;
    }
}

