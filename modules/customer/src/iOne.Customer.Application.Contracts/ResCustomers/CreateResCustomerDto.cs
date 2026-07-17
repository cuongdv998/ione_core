using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResCustomers;

namespace iOne.Customer.ResCustomers;

public class CreateResCustomerDto
{
    [MaxLength(25)]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "Code can only contain uppercase letters (A-Z), numbers (0-9) and underscore (_)")]
    public string? Code { get; set; }

    [MaxLength(50)]
    public string? RefCode { get; set; }

    [Required]
    [MaxLength(250)]
    public string Name { get; set; } = null!;

    public Guid? IndustryId { get; set; }

    public Guid? ProvinceId { get; set; }

    public Guid? WardId { get; set; }

    [MaxLength(250)]
    public string? Address { get; set; }

    [MaxLength(50)]
    [EmailAddress]
    public string? Email { get; set; }

    [Required]
    [MaxLength(15)]
    [RegularExpression(@"^[0-9]+$", ErrorMessage = "Customer:ResCustomer:PhoneInvalid")]
    public string Phone { get; set; } = null!;

    [MaxLength(500)]
    public string? Note { get; set; }

    [Required]
    public ResCustomerStatus Status { get; set; }

    [MaxLength(50)]
    public string? Tin { get; set; }

    [MaxLength(25)]
    public string? IdNo { get; set; }

    [MaxLength(25)]
    public string? PassportNo { get; set; }

    public DateTime? Dob { get; set; }

    public ResCustomerSex? Sex { get; set; }

    [MaxLength(250)]
    public string? RepName { get; set; }

    [MaxLength(50)]
    [EmailAddress]
    public string? RepEmail { get; set; }

    [MaxLength(15)]
    [RegularExpression(@"^[0-9]*$", ErrorMessage = "Customer:ResCustomer:PhoneInvalid")]
    public string? RepPhone { get; set; }

    [MaxLength(25)]
    public string? RepIdNo { get; set; }

    [MaxLength(250)]
    public string? RepTitle { get; set; }

    [MaxLength(50)]
    public string? Authorizer { get; set; }

    [MaxLength(15)]
    [RegularExpression(@"^[0-9]*$", ErrorMessage = "Customer:ResCustomer:PhoneInvalid")]
    public string? AuthorizerPhone { get; set; }

    [MaxLength(50)]
    [EmailAddress]
    public string? AuthorizerEmail { get; set; }

    [MaxLength(25)]
    public string? AuthorizerNo { get; set; }

    public DateTime? AuthorizerDate { get; set; }

    [MaxLength(50)]
    public string? AuthorizerTitle { get; set; }

    [MaxLength(25)]
    public string? BusinessNo { get; set; }

    public Guid? OrganizationTypeId { get; set; }

    public Guid? InvoiceProvinceId { get; set; }

    public Guid? InvoiceWardId { get; set; }

    [MaxLength(250)]
    public string? InvoiceAddress { get; set; }

    public Guid? SaleId { get; set; }
}

