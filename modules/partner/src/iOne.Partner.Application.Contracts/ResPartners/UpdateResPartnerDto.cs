using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using iOne.ResPartners;

namespace iOne.Partner.ResPartners;

public class UpdateResPartnerDto
{
    // ⚠️ QUAN TRỌNG: KHÔNG có Code field - Code immutable

    [Display(Name = "Partner:ResPartner:Channel")]
    public Guid? ChannelId { get; set; }

    [Required(ErrorMessage = "Partner:ResPartner:PartnerTypeRequired")]
    [Display(Name = "Partner:ResPartner:PartnerType")]
    public Guid PartnerTypeId { get; set; }

    [StringLength(15, ErrorMessage = "Partner:ResPartner:PartnerRoleMaxLength")]
    [Display(Name = "Partner:ResPartner:PartnerRole")]
    public string? PartnerRole { get; set; }

    [Required(ErrorMessage = "Partner:ResPartner:NameRequired")]
    [StringLength(250, ErrorMessage = "Partner:ResPartner:NameMaxLength")]
    [Display(Name = "Partner:ResPartner:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "Partner:ResPartner:OrganizationType")]
    public Guid? OrganizationTypeId { get; set; }

    [Required(ErrorMessage = "Partner:ResPartner:ProvinceRequired")]
    [Display(Name = "Partner:ResPartner:Province")]
    public Guid ProvinceId { get; set; }

    [Required(ErrorMessage = "Partner:ResPartner:WardRequired")]
    [Display(Name = "Partner:ResPartner:Ward")]
    public Guid WardId { get; set; }

    [Required(ErrorMessage = "Partner:ResPartner:AddressRequired")]
    [StringLength(250, ErrorMessage = "Partner:ResPartner:AddressMaxLength")]
    [Display(Name = "Partner:ResPartner:Address")]
    public string Address { get; set; } = null!;

    [StringLength(50, ErrorMessage = "Partner:ResPartner:EmailMaxLength")]
    [Display(Name = "Partner:ResPartner:Email")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Partner:ResPartner:PhoneRequired")]
    [StringLength(15, ErrorMessage = "Partner:ResPartner:PhoneMaxLength")]
    [Display(Name = "Partner:ResPartner:Phone")]
    public string Phone { get; set; } = null!;

    [StringLength(500, ErrorMessage = "Partner:ResPartner:NoteMaxLength")]
    [Display(Name = "Partner:ResPartner:Note")]
    public string? Note { get; set; }

    [Required(ErrorMessage = "Partner:ResPartner:StatusRequired")]
    [Display(Name = "Partner:ResPartner:Status")]
    public ResPartnerStatus Status { get; set; }

    [Display(Name = "Partner:ResPartner:InvoiceProvince")]
    public Guid? InvoiceProvinceId { get; set; }

    [Display(Name = "Partner:ResPartner:InvoiceWard")]
    public Guid? InvoiceWardId { get; set; }

    [StringLength(250, ErrorMessage = "Partner:ResPartner:InvoiceAddressMaxLength")]
    [Display(Name = "Partner:ResPartner:InvoiceAddress")]
    public string? InvoiceAddress { get; set; }

    // CN (Cá nhân) fields
    [StringLength(25, ErrorMessage = "Partner:ResPartner:IdNoMaxLength")]
    [Display(Name = "Partner:ResPartner:IdNo")]
    public string? IdNo { get; set; }

    // TC (Tổ chức) fields
    [StringLength(50, ErrorMessage = "Partner:ResPartner:TinMaxLength")]
    [Display(Name = "Partner:ResPartner:Tin")]
    public string? Tin { get; set; }

    [StringLength(250, ErrorMessage = "Partner:ResPartner:RepNameMaxLength")]
    [Display(Name = "Partner:ResPartner:RepName")]
    public string? RepName { get; set; }

    [StringLength(50, ErrorMessage = "Partner:ResPartner:RepEmailMaxLength")]
    [Display(Name = "Partner:ResPartner:RepEmail")]
    public string? RepEmail { get; set; }

    [StringLength(15, ErrorMessage = "Partner:ResPartner:RepPhoneMaxLength")]
    [Display(Name = "Partner:ResPartner:RepPhone")]
    public string? RepPhone { get; set; }

    [StringLength(25, ErrorMessage = "Partner:ResPartner:RepIdNoMaxLength")]
    [Display(Name = "Partner:ResPartner:RepIdNo")]
    public string? RepIdNo { get; set; }

    [StringLength(250, ErrorMessage = "Partner:ResPartner:RepTitleMaxLength")]
    [Display(Name = "Partner:ResPartner:RepTitle")]
    public string? RepTitle { get; set; }

    [StringLength(50, ErrorMessage = "Partner:ResPartner:AuthorizerMaxLength")]
    [Display(Name = "Partner:ResPartner:Authorizer")]
    public string? Authorizer { get; set; }

    [StringLength(15, ErrorMessage = "Partner:ResPartner:AuthorizerPhoneMaxLength")]
    [Display(Name = "Partner:ResPartner:AuthorizerPhone")]
    public string? AuthorizerPhone { get; set; }

    [StringLength(50, ErrorMessage = "Partner:ResPartner:AuthorizerEmailMaxLength")]
    [Display(Name = "Partner:ResPartner:AuthorizerEmail")]
    public string? AuthorizerEmail { get; set; }

    [StringLength(25, ErrorMessage = "Partner:ResPartner:AuthorizerNoMaxLength")]
    [Display(Name = "Partner:ResPartner:AuthorizerNo")]
    public string? AuthorizerNo { get; set; }

    [Display(Name = "Partner:ResPartner:AuthorizerDate")]
    public DateTime? AuthorizerDate { get; set; }

    [StringLength(50, ErrorMessage = "Partner:ResPartner:AuthorizerTitleMaxLength")]
    [Display(Name = "Partner:ResPartner:AuthorizerTitle")]
    public string? AuthorizerTitle { get; set; }

    [StringLength(25, ErrorMessage = "Partner:ResPartner:BusinessNoMaxLength")]
    [Display(Name = "Partner:ResPartner:BusinessNo")]
    public string? BusinessNo { get; set; }

    // Agreements
    public List<UpdateResPartnerAgreementDto>? Agreements { get; set; }
}

