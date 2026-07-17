using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using iOne.ResPartners;
using Volo.Abp.Application.Dtos;

namespace iOne.Partner.ResPartners;

public class ResPartnerDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "Partner:ResPartner:Channel")]
    public Guid? ChannelId { get; set; }

    [Display(Name = "Partner:ResPartner:ChannelName")]
    public string? ChannelName { get; set; }

    [Display(Name = "Partner:ResPartner:PartnerType")]
    public Guid PartnerTypeId { get; set; }

    [Display(Name = "Partner:ResPartner:PartnerTypeName")]
    public string PartnerTypeName { get; set; } = null!;

    [Display(Name = "Partner:ResPartner:PartnerRole")]
    public string? PartnerRole { get; set; }

    [Display(Name = "Partner:ResPartner:Code")]
    public string? Code { get; set; }

    [Display(Name = "Partner:ResPartner:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "Partner:ResPartner:OrganizationType")]
    public Guid? OrganizationTypeId { get; set; }

    [Display(Name = "Partner:ResPartner:OrganizationTypeName")]
    public string? OrganizationTypeName { get; set; }

    [Display(Name = "Partner:ResPartner:Province")]
    public Guid ProvinceId { get; set; }

    [Display(Name = "Partner:ResPartner:ProvinceName")]
    public string ProvinceName { get; set; } = null!;

    [Display(Name = "Partner:ResPartner:Ward")]
    public Guid WardId { get; set; }

    [Display(Name = "Partner:ResPartner:WardName")]
    public string WardName { get; set; } = null!;

    [Display(Name = "Partner:ResPartner:Address")]
    public string Address { get; set; } = null!;

    [Display(Name = "Partner:ResPartner:FullAddress")]
    public string FullAddress { get; set; } = null!;

    [Display(Name = "Partner:ResPartner:Email")]
    public string? Email { get; set; }

    [Display(Name = "Partner:ResPartner:Phone")]
    public string Phone { get; set; } = null!;

    [Display(Name = "Partner:ResPartner:Note")]
    public string? Note { get; set; }

    [Display(Name = "Partner:ResPartner:Status")]
    public ResPartnerStatus Status { get; set; }

    [Display(Name = "Partner:ResPartner:InvoiceProvince")]
    public Guid? InvoiceProvinceId { get; set; }

    [Display(Name = "Partner:ResPartner:InvoiceProvinceName")]
    public string? InvoiceProvinceName { get; set; }

    [Display(Name = "Partner:ResPartner:InvoiceWard")]
    public Guid? InvoiceWardId { get; set; }

    [Display(Name = "Partner:ResPartner:InvoiceWardName")]
    public string? InvoiceWardName { get; set; }

    [Display(Name = "Partner:ResPartner:InvoiceAddress")]
    public string? InvoiceAddress { get; set; }

    [Display(Name = "Partner:ResPartner:InvoiceFullAddress")]
    public string? InvoiceFullAddress { get; set; }

    // CN (Cá nhân) fields
    [Display(Name = "Partner:ResPartner:IdNo")]
    public string? IdNo { get; set; }

    // TC (Tổ chức) fields
    [Display(Name = "Partner:ResPartner:Tin")]
    public string? Tin { get; set; }

    [Display(Name = "Partner:ResPartner:RepName")]
    public string? RepName { get; set; }

    [Display(Name = "Partner:ResPartner:RepEmail")]
    public string? RepEmail { get; set; }

    [Display(Name = "Partner:ResPartner:RepPhone")]
    public string? RepPhone { get; set; }

    [Display(Name = "Partner:ResPartner:RepIdNo")]
    public string? RepIdNo { get; set; }

    [Display(Name = "Partner:ResPartner:RepTitle")]
    public string? RepTitle { get; set; }

    [Display(Name = "Partner:ResPartner:Authorizer")]
    public string? Authorizer { get; set; }

    [Display(Name = "Partner:ResPartner:AuthorizerPhone")]
    public string? AuthorizerPhone { get; set; }

    [Display(Name = "Partner:ResPartner:AuthorizerEmail")]
    public string? AuthorizerEmail { get; set; }

    [Display(Name = "Partner:ResPartner:AuthorizerNo")]
    public string? AuthorizerNo { get; set; }

    [Display(Name = "Partner:ResPartner:AuthorizerDate")]
    public DateTime? AuthorizerDate { get; set; }

    [Display(Name = "Partner:ResPartner:AuthorizerTitle")]
    public string? AuthorizerTitle { get; set; }

    [Display(Name = "Partner:ResPartner:BusinessNo")]
    public string? BusinessNo { get; set; }

    // Agreements
    public List<ResPartnerAgreementDto> Agreements { get; set; } = new List<ResPartnerAgreementDto>();
}

