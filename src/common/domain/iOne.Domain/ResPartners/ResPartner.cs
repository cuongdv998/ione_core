using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.ResPartners;

[Table("res_partner")]
public class ResPartner : FullAuditedAggregateRoot<Guid>
{
    // Foreign Keys
    public virtual Guid? ChannelId { get; private set; }
    public virtual Guid PartnerTypeId { get; private set; }
    public virtual string? PartnerRole { get; private set; } // Từ AdminConfig (sub_code)
    public virtual Guid? OrganizationTypeId { get; private set; }
    public virtual Guid ProvinceId { get; private set; }
    public virtual Guid WardId { get; private set; }
    public virtual Guid? InvoiceProvinceId { get; private set; }
    public virtual Guid? InvoiceWardId { get; private set; }

    // Basic Info
    public virtual string? Code { get; private set; } // Unique, immutable
    public virtual string Name { get; private set; } = null!;
    public virtual string Address { get; private set; } = null!;
    public virtual string FullAddress { get; private set; } = null!; // Computed: Address + Ward Name + Province Name
    public virtual string? Email { get; private set; }
    public virtual string Phone { get; private set; } = null!;
    public virtual string? Note { get; private set; }
    public virtual ResPartnerStatus Status { get; private set; }

    // Invoice Address
    public virtual string? InvoiceAddress { get; private set; }
    public virtual string? InvoiceFullAddress { get; private set; } // Computed: InvoiceAddress + InvoiceWard Name + InvoiceProvince Name

    // CN (Cá nhân) fields
    public virtual string? IdNo { get; private set; } // CCCD

    // TC (Tổ chức) fields
    public virtual string? Tin { get; private set; } // Mã số thuế
    public virtual string? RepName { get; private set; }
    public virtual string? RepEmail { get; private set; }
    public virtual string? RepPhone { get; private set; }
    public virtual string? RepIdNo { get; private set; }
    public virtual string? RepTitle { get; private set; }
    public virtual string? Authorizer { get; private set; }
    public virtual string? AuthorizerPhone { get; private set; }
    public virtual string? AuthorizerEmail { get; private set; }
    public virtual string? AuthorizerNo { get; private set; }
    public virtual DateTime? AuthorizerDate { get; private set; }
    public virtual string? AuthorizerTitle { get; private set; }
    public virtual string? BusinessNo { get; private set; }

    // Navigation Properties
    public virtual ResChannels.ResChannel? Channel { get; set; }
    public virtual ResPartnerTypes.ResPartnerType PartnerType { get; set; } = null!;
    public virtual ResOrganizationTypes.ResOrganizationType? OrganizationType { get; set; }
    public virtual ResProvinces.ResProvince Province { get; set; } = null!;
    public virtual ResWards.ResWard Ward { get; set; } = null!;
    public virtual ResProvinces.ResProvince? InvoiceProvince { get; set; }
    public virtual ResWards.ResWard? InvoiceWard { get; set; }

    // Collection
    public virtual ICollection<ResPartnerAgreement> Agreements { get; set; } = new List<ResPartnerAgreement>();
    public virtual ICollection<ProTableRates.ProTableRate> TableRates { get; set; } = new List<ProTableRates.ProTableRate>();
    public virtual ICollection<PolicyContracts.PolicyContract> PolicyContracts { get; set; } = new List<PolicyContracts.PolicyContract>();
    public virtual ICollection<Policies.Policy> Policies { get; set; } = new List<Policies.Policy>();
    public virtual ICollection<ProProducts.ProProduct> Products { get; set; } = new List<ProProducts.ProProduct>();

    protected ResPartner()
    {
        // For ORM
    }

    public ResPartner(
        Guid id,
        Guid partnerTypeId,
        Guid provinceId,
        Guid wardId,
        string name,
        string address,
        string fullAddress,
        string phone,
        ResPartnerStatus status,
        Guid? channelId = null,
        string? partnerRole = null,
        Guid? organizationTypeId = null,
        string? code = null,
        string? email = null,
        string? note = null,
        Guid? invoiceProvinceId = null,
        Guid? invoiceWardId = null,
        string? invoiceAddress = null,
        string? invoiceFullAddress = null,
        // CN fields
        string? idNo = null,
        // TC fields
        string? tin = null,
        string? repName = null,
        string? repEmail = null,
        string? repPhone = null,
        string? repIdNo = null,
        string? repTitle = null,
        string? authorizer = null,
        string? authorizerPhone = null,
        string? authorizerEmail = null,
        string? authorizerNo = null,
        DateTime? authorizerDate = null,
        string? authorizerTitle = null,
        string? businessNo = null)
        : base(id)
    {
        SetPartnerTypeId(partnerTypeId);
        SetProvinceId(provinceId);
        SetWardId(wardId);
        SetName(name);
        SetAddress(address);
        SetFullAddress(fullAddress);
        SetPhone(phone);
        SetStatus(status);
        SetChannelId(channelId);
        SetPartnerRole(partnerRole);
        SetOrganizationTypeId(organizationTypeId);
        SetCode(code);
        SetEmail(email);
        SetNote(note);
        SetInvoiceProvinceId(invoiceProvinceId);
        SetInvoiceWardId(invoiceWardId);
        SetInvoiceAddress(invoiceAddress);
        SetInvoiceFullAddress(invoiceFullAddress);
        // CN fields
        SetIdNo(idNo);
        // TC fields
        SetTin(tin);
        SetRepName(repName);
        SetRepEmail(repEmail);
        SetRepPhone(repPhone);
        SetRepIdNo(repIdNo);
        SetRepTitle(repTitle);
        SetAuthorizer(authorizer);
        SetAuthorizerPhone(authorizerPhone);
        SetAuthorizerEmail(authorizerEmail);
        SetAuthorizerNo(authorizerNo);
        SetAuthorizerDate(authorizerDate);
        SetAuthorizerTitle(authorizerTitle);
        SetBusinessNo(businessNo);
    }

    private void SetChannelId(Guid? channelId)
    {
        ChannelId = channelId;
    }

    private void SetPartnerTypeId(Guid partnerTypeId)
    {
        if (partnerTypeId == Guid.Empty)
        {
            throw new ArgumentException("PartnerTypeId cannot be empty.", nameof(partnerTypeId));
        }
        PartnerTypeId = partnerTypeId;
    }

    private void SetPartnerRole(string? partnerRole)
    {
        if (!string.IsNullOrWhiteSpace(partnerRole) && partnerRole.Length > 15)
        {
            throw new ArgumentException("PartnerRole cannot exceed 15 characters.", nameof(partnerRole));
        }
        PartnerRole = partnerRole;
    }

    private void SetOrganizationTypeId(Guid? organizationTypeId)
    {
        OrganizationTypeId = organizationTypeId;
    }

    private void SetProvinceId(Guid provinceId)
    {
        if (provinceId == Guid.Empty)
        {
            throw new ArgumentException("ProvinceId cannot be empty.", nameof(provinceId));
        }
        ProvinceId = provinceId;
    }

    private void SetWardId(Guid wardId)
    {
        if (wardId == Guid.Empty)
        {
            throw new ArgumentException("WardId cannot be empty.", nameof(wardId));
        }
        WardId = wardId;
    }

    private void SetCode(string? code)
    {
        if (!string.IsNullOrWhiteSpace(code))
        {
            if (code.Length > 25)
            {
                throw new ArgumentException("Code cannot exceed 25 characters.", nameof(code));
            }

            var upperCode = code.ToUpperInvariant();
            if (!Regex.IsMatch(upperCode, @"^[A-Z0-9_]+$"))
            {
                throw new ArgumentException("Code can only contain letters (A-Z), numbers (0-9), and underscore (_).", nameof(code));
            }

            Code = upperCode;
        }
        else
        {
            Code = null;
        }
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));
        }

        if (name.Length > 250)
        {
            throw new ArgumentException("Name cannot exceed 250 characters.", nameof(name));
        }

        Name = name;
    }

    private void SetAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            throw new ArgumentException("Address cannot be null or empty.", nameof(address));
        }

        if (address.Length > 250)
        {
            throw new ArgumentException("Address cannot exceed 250 characters.", nameof(address));
        }

        Address = address;
    }

    private void SetFullAddress(string fullAddress)
    {
        if (string.IsNullOrWhiteSpace(fullAddress))
        {
            throw new ArgumentException("FullAddress cannot be null or empty.", nameof(fullAddress));
        }

        if (fullAddress.Length > 500)
        {
            throw new ArgumentException("FullAddress cannot exceed 500 characters.", nameof(fullAddress));
        }

        FullAddress = fullAddress;
    }

    private void SetEmail(string? email)
    {
        if (!string.IsNullOrWhiteSpace(email) && email.Length > 50)
        {
            throw new ArgumentException("Email cannot exceed 50 characters.", nameof(email));
        }
        Email = email;
    }

    private void SetPhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            throw new ArgumentException("Phone cannot be null or empty.", nameof(phone));
        }

        if (phone.Length > 15)
        {
            throw new ArgumentException("Phone cannot exceed 15 characters.", nameof(phone));
        }

        Phone = phone;
    }

    private void SetNote(string? note)
    {
        if (!string.IsNullOrWhiteSpace(note) && note.Length > 500)
        {
            throw new ArgumentException("Note cannot exceed 500 characters.", nameof(note));
        }
        Note = note;
    }

    private void SetStatus(ResPartnerStatus status)
    {
        Status = status;
    }

    private void SetInvoiceProvinceId(Guid? invoiceProvinceId)
    {
        InvoiceProvinceId = invoiceProvinceId;
    }

    private void SetInvoiceWardId(Guid? invoiceWardId)
    {
        InvoiceWardId = invoiceWardId;
    }

    private void SetInvoiceAddress(string? invoiceAddress)
    {
        if (!string.IsNullOrWhiteSpace(invoiceAddress) && invoiceAddress.Length > 250)
        {
            throw new ArgumentException("InvoiceAddress cannot exceed 250 characters.", nameof(invoiceAddress));
        }
        InvoiceAddress = invoiceAddress;
    }

    private void SetInvoiceFullAddress(string? invoiceFullAddress)
    {
        if (!string.IsNullOrWhiteSpace(invoiceFullAddress) && invoiceFullAddress.Length > 500)
        {
            throw new ArgumentException("InvoiceFullAddress cannot exceed 500 characters.", nameof(invoiceFullAddress));
        }
        InvoiceFullAddress = invoiceFullAddress;
    }

    // CN (Cá nhân) fields setters
    private void SetIdNo(string? idNo)
    {
        if (!string.IsNullOrWhiteSpace(idNo) && idNo.Length > 25)
        {
            throw new ArgumentException("IdNo cannot exceed 25 characters.", nameof(idNo));
        }
        IdNo = idNo;
    }

    // TC (Tổ chức) fields setters
    private void SetTin(string? tin)
    {
        if (!string.IsNullOrWhiteSpace(tin) && tin.Length > 50)
        {
            throw new ArgumentException("Tin cannot exceed 50 characters.", nameof(tin));
        }
        Tin = tin;
    }

    private void SetRepName(string? repName)
    {
        if (!string.IsNullOrWhiteSpace(repName) && repName.Length > 250)
        {
            throw new ArgumentException("RepName cannot exceed 250 characters.", nameof(repName));
        }
        RepName = repName;
    }

    private void SetRepEmail(string? repEmail)
    {
        if (!string.IsNullOrWhiteSpace(repEmail) && repEmail.Length > 50)
        {
            throw new ArgumentException("RepEmail cannot exceed 50 characters.", nameof(repEmail));
        }
        RepEmail = repEmail;
    }

    private void SetRepPhone(string? repPhone)
    {
        if (!string.IsNullOrWhiteSpace(repPhone) && repPhone.Length > 15)
        {
            throw new ArgumentException("RepPhone cannot exceed 15 characters.", nameof(repPhone));
        }
        RepPhone = repPhone;
    }

    private void SetRepIdNo(string? repIdNo)
    {
        if (!string.IsNullOrWhiteSpace(repIdNo) && repIdNo.Length > 25)
        {
            throw new ArgumentException("RepIdNo cannot exceed 25 characters.", nameof(repIdNo));
        }
        RepIdNo = repIdNo;
    }

    private void SetRepTitle(string? repTitle)
    {
        if (!string.IsNullOrWhiteSpace(repTitle) && repTitle.Length > 250)
        {
            throw new ArgumentException("RepTitle cannot exceed 250 characters.", nameof(repTitle));
        }
        RepTitle = repTitle;
    }

    private void SetAuthorizer(string? authorizer)
    {
        if (!string.IsNullOrWhiteSpace(authorizer) && authorizer.Length > 50)
        {
            throw new ArgumentException("Authorizer cannot exceed 50 characters.", nameof(authorizer));
        }
        Authorizer = authorizer;
    }

    private void SetAuthorizerPhone(string? authorizerPhone)
    {
        if (!string.IsNullOrWhiteSpace(authorizerPhone) && authorizerPhone.Length > 15)
        {
            throw new ArgumentException("AuthorizerPhone cannot exceed 15 characters.", nameof(authorizerPhone));
        }
        AuthorizerPhone = authorizerPhone;
    }

    private void SetAuthorizerEmail(string? authorizerEmail)
    {
        if (!string.IsNullOrWhiteSpace(authorizerEmail) && authorizerEmail.Length > 50)
        {
            throw new ArgumentException("AuthorizerEmail cannot exceed 50 characters.", nameof(authorizerEmail));
        }
        AuthorizerEmail = authorizerEmail;
    }

    private void SetAuthorizerNo(string? authorizerNo)
    {
        if (!string.IsNullOrWhiteSpace(authorizerNo) && authorizerNo.Length > 25)
        {
            throw new ArgumentException("AuthorizerNo cannot exceed 25 characters.", nameof(authorizerNo));
        }
        AuthorizerNo = authorizerNo;
    }

    private void SetAuthorizerDate(DateTime? authorizerDate)
    {
        AuthorizerDate = authorizerDate;
    }

    private void SetAuthorizerTitle(string? authorizerTitle)
    {
        if (!string.IsNullOrWhiteSpace(authorizerTitle) && authorizerTitle.Length > 50)
        {
            throw new ArgumentException("AuthorizerTitle cannot exceed 50 characters.", nameof(authorizerTitle));
        }
        AuthorizerTitle = authorizerTitle;
    }

    private void SetBusinessNo(string? businessNo)
    {
        if (!string.IsNullOrWhiteSpace(businessNo) && businessNo.Length > 25)
        {
            throw new ArgumentException("BusinessNo cannot exceed 25 characters.", nameof(businessNo));
        }
        BusinessNo = businessNo;
    }

    // ⚠️ QUAN TRỌNG: Không có UpdateCode() - Code immutable
    public virtual void UpdateName(string name)
    {
        SetName(name);
    }

    public virtual void UpdateAddress(string address)
    {
        SetAddress(address);
    }

    public virtual void UpdateFullAddress(string fullAddress)
    {
        SetFullAddress(fullAddress);
    }

    public virtual void UpdateEmail(string? email)
    {
        SetEmail(email);
    }

    public virtual void UpdatePhone(string phone)
    {
        SetPhone(phone);
    }

    public virtual void UpdateNote(string? note)
    {
        SetNote(note);
    }

    public virtual void UpdateStatus(ResPartnerStatus status)
    {
        SetStatus(status);
    }

    public virtual void UpdateChannelId(Guid? channelId)
    {
        SetChannelId(channelId);
    }

    public virtual void UpdatePartnerTypeId(Guid partnerTypeId)
    {
        SetPartnerTypeId(partnerTypeId);
    }

    public virtual void UpdatePartnerRole(string? partnerRole)
    {
        SetPartnerRole(partnerRole);
    }

    public virtual void UpdateOrganizationTypeId(Guid? organizationTypeId)
    {
        SetOrganizationTypeId(organizationTypeId);
    }

    public virtual void UpdateProvinceId(Guid provinceId)
    {
        SetProvinceId(provinceId);
    }

    public virtual void UpdateWardId(Guid wardId)
    {
        SetWardId(wardId);
    }

    public virtual void UpdateInvoiceProvinceId(Guid? invoiceProvinceId)
    {
        SetInvoiceProvinceId(invoiceProvinceId);
    }

    public virtual void UpdateInvoiceWardId(Guid? invoiceWardId)
    {
        SetInvoiceWardId(invoiceWardId);
    }

    public virtual void UpdateInvoiceAddress(string? invoiceAddress)
    {
        SetInvoiceAddress(invoiceAddress);
    }

    public virtual void UpdateInvoiceFullAddress(string? invoiceFullAddress)
    {
        SetInvoiceFullAddress(invoiceFullAddress);
    }

    // CN fields update methods
    public virtual void UpdateIdNo(string? idNo)
    {
        SetIdNo(idNo);
    }

    // TC fields update methods
    public virtual void UpdateTin(string? tin)
    {
        SetTin(tin);
    }

    public virtual void UpdateRepName(string? repName)
    {
        SetRepName(repName);
    }

    public virtual void UpdateRepEmail(string? repEmail)
    {
        SetRepEmail(repEmail);
    }

    public virtual void UpdateRepPhone(string? repPhone)
    {
        SetRepPhone(repPhone);
    }

    public virtual void UpdateRepIdNo(string? repIdNo)
    {
        SetRepIdNo(repIdNo);
    }

    public virtual void UpdateRepTitle(string? repTitle)
    {
        SetRepTitle(repTitle);
    }

    public virtual void UpdateAuthorizer(string? authorizer)
    {
        SetAuthorizer(authorizer);
    }

    public virtual void UpdateAuthorizerPhone(string? authorizerPhone)
    {
        SetAuthorizerPhone(authorizerPhone);
    }

    public virtual void UpdateAuthorizerEmail(string? authorizerEmail)
    {
        SetAuthorizerEmail(authorizerEmail);
    }

    public virtual void UpdateAuthorizerNo(string? authorizerNo)
    {
        SetAuthorizerNo(authorizerNo);
    }

    public virtual void UpdateAuthorizerDate(DateTime? authorizerDate)
    {
        SetAuthorizerDate(authorizerDate);
    }

    public virtual void UpdateAuthorizerTitle(string? authorizerTitle)
    {
        SetAuthorizerTitle(authorizerTitle);
    }

    public virtual void UpdateBusinessNo(string? businessNo)
    {
        SetBusinessNo(businessNo);
    }

    // Agreements management
    public virtual void AddAgreement(ResPartnerAgreement agreement)
    {
        if (agreement == null)
        {
            throw new ArgumentNullException(nameof(agreement));
        }
        Agreements.Add(agreement);
    }

    public virtual void RemoveAgreement(ResPartnerAgreement agreement)
    {
        if (agreement == null)
        {
            throw new ArgumentNullException(nameof(agreement));
        }
        Agreements.Remove(agreement);
    }
}

