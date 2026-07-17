using System;
using Volo.Abp.Application.Dtos;
using iOne.ResCustomers;

namespace iOne.Customer.ResCustomers;

public class GetResCustomersInput : PagedAndSortedResultRequestDto
{
    /// <summary>
    /// Tìm theo tên khách hàng (contains, không phân biệt hoa thường).
    /// </summary>
    public string? Keyword { get; set; }

    public string? Filter { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public Guid? IndustryId { get; set; }
    public Guid? ProvinceId { get; set; }
    public Guid? WardId { get; set; }
    public Guid? OrganizationTypeId { get; set; }
    public Guid? SaleId { get; set; }
    public ResCustomerStatus? Status { get; set; }
}

