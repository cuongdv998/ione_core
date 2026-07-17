using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace iOne.ResCustomers;

public interface IResCustomerRepository : IRepository<ResCustomer, Guid>
{
    /// <summary>
    /// Ki?m tra xem có Customer nào ?ang s? d?ng Industry này không
    /// </summary>
    Task<bool> AnyByIndustryIdAsync(Guid industryId);

    /// <summary>
    /// Ki?m tra xem có Customer nào ?ang s? d?ng Province này không (bao g?m c? InvoiceProvinceId)
    /// </summary>
    Task<bool> AnyByProvinceIdAsync(Guid provinceId);

    /// <summary>
    /// Ki?m tra xem có Customer nào ?ang s? d?ng Ward này không (bao g?m c? InvoiceWardId)
    /// </summary>
    Task<bool> AnyByWardIdAsync(Guid wardId);

    /// <summary>
    /// Ki?m tra xem có Customer nào ?ang s? d?ng OrganizationType này không
    /// </summary>
    Task<bool> AnyByOrganizationTypeIdAsync(Guid organizationTypeId);
}

