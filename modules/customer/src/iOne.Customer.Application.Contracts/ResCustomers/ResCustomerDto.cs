using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;
using iOne.ResCustomers;

namespace iOne.Customer.ResCustomers;

public class ResCustomerDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "Customer:ResCustomer:Code")]
    public string? Code { get; set; }

    [Display(Name = "Customer:ResCustomer:RefCode")]
    public string? RefCode { get; set; }

    [Display(Name = "Customer:ResCustomer:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "Customer:ResCustomer:IndustryId")]
    public Guid? IndustryId { get; set; }

    [Display(Name = "Customer:ResCustomer:IndustryName")]
    public string? IndustryName { get; set; }

    [Display(Name = "Customer:ResCustomer:ProvinceId")]
    public Guid? ProvinceId { get; set; }

    [Display(Name = "Customer:ResCustomer:ProvinceName")]
    public string? ProvinceName { get; set; }

    [Display(Name = "Customer:ResCustomer:WardId")]
    public Guid? WardId { get; set; }

    [Display(Name = "Customer:ResCustomer:WardName")]
    public string? WardName { get; set; }

    [Display(Name = "Customer:ResCustomer:Address")]
    public string? Address { get; set; }

    [Display(Name = "Customer:ResCustomer:FullAddress")]
    public string? FullAddress { get; set; }

    [Display(Name = "Customer:ResCustomer:Email")]
    public string? Email { get; set; }

    [Display(Name = "Customer:ResCustomer:Phone")]
    public string Phone { get; set; } = null!;

    [Display(Name = "Customer:ResCustomer:Note")]
    public string? Note { get; set; }

    [Display(Name = "Customer:ResCustomer:Status")]
    public ResCustomerStatus Status { get; set; }

    [Display(Name = "Customer:ResCustomer:Tin")]
    public string? Tin { get; set; }

    [Display(Name = "Customer:ResCustomer:IdNo")]
    public string? IdNo { get; set; }

    [Display(Name = "Customer:ResCustomer:PassportNo")]
    public string? PassportNo { get; set; }

    [Display(Name = "Customer:ResCustomer:Dob")]
    public DateTime? Dob { get; set; }

    [Display(Name = "Customer:ResCustomer:Sex")]
    public ResCustomerSex? Sex { get; set; }

    [Display(Name = "Customer:ResCustomer:RepName")]
    public string? RepName { get; set; }

    [Display(Name = "Customer:ResCustomer:RepEmail")]
    public string? RepEmail { get; set; }

    [Display(Name = "Customer:ResCustomer:RepPhone")]
    public string? RepPhone { get; set; }

    [Display(Name = "Customer:ResCustomer:RepIdNo")]
    public string? RepIdNo { get; set; }

    [Display(Name = "Customer:ResCustomer:RepTitle")]
    public string? RepTitle { get; set; }

    [Display(Name = "Customer:ResCustomer:Authorizer")]
    public string? Authorizer { get; set; }

    [Display(Name = "Customer:ResCustomer:AuthorizerPhone")]
    public string? AuthorizerPhone { get; set; }

    [Display(Name = "Customer:ResCustomer:AuthorizerEmail")]
    public string? AuthorizerEmail { get; set; }

    [Display(Name = "Customer:ResCustomer:AuthorizerNo")]
    public string? AuthorizerNo { get; set; }

    [Display(Name = "Customer:ResCustomer:AuthorizerDate")]
    public DateTime? AuthorizerDate { get; set; }

    [Display(Name = "Customer:ResCustomer:AuthorizerTitle")]
    public string? AuthorizerTitle { get; set; }

    [Display(Name = "Customer:ResCustomer:BusinessNo")]
    public string? BusinessNo { get; set; }

    [Display(Name = "Customer:ResCustomer:OrganizationTypeId")]
    public Guid? OrganizationTypeId { get; set; }

    [Display(Name = "Customer:ResCustomer:OrganizationTypeName")]
    public string? OrganizationTypeName { get; set; }

    [Display(Name = "Customer:ResCustomer:OrganizationTypeType")]
    public string? OrganizationTypeType { get; set; } // "CN" hoặc "TC"

    [Display(Name = "Customer:ResCustomer:InvoiceProvinceId")]
    public Guid? InvoiceProvinceId { get; set; }

    [Display(Name = "Customer:ResCustomer:InvoiceProvinceName")]
    public string? InvoiceProvinceName { get; set; }

    [Display(Name = "Customer:ResCustomer:InvoiceWardId")]
    public Guid? InvoiceWardId { get; set; }

    [Display(Name = "Customer:ResCustomer:InvoiceWardName")]
    public string? InvoiceWardName { get; set; }

    [Display(Name = "Customer:ResCustomer:InvoiceAddress")]
    public string? InvoiceAddress { get; set; }

    [Display(Name = "Customer:ResCustomer:InvoiceFullAddress")]
    public string? InvoiceFullAddress { get; set; }

    [Display(Name = "Customer:ResCustomer:SaleId")]
    public Guid? SaleId { get; set; }

    [Display(Name = "Customer:ResCustomer:SaleName")]
    public string? SaleName { get; set; }
}

