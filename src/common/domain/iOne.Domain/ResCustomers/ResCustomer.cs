using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Volo.Abp.Domain.Entities.Auditing;
using iOne.ResIndustries;
using iOne.ResProvinces;
using iOne.ResWards;
using iOne.ResOrganizationTypes;
using iOne.HrEmployees;

namespace iOne.ResCustomers;

[Table("res_customer")]
public class ResCustomer : FullAuditedAggregateRoot<Guid>
{
    // Basic Info
    [MaxLength(25)]
    public virtual string? Code { get; private set; }

    [MaxLength(50)]
    public virtual string? RefCode { get; private set; }

    [Required]
    [MaxLength(250)]
    public virtual string Name { get; private set; } = null!;

    [Required]
    public virtual ResCustomerStatus Status { get; private set; }

    // Foreign Keys
    public virtual Guid? IndustryId { get; private set; }
    public virtual Guid? ProvinceId { get; private set; }
    public virtual Guid? WardId { get; private set; }
    public virtual Guid? OrganizationTypeId { get; private set; }
    public virtual Guid? InvoiceProvinceId { get; private set; }
    public virtual Guid? InvoiceWardId { get; private set; }
    public virtual Guid? SaleId { get; private set; }

    // Address Info
    [MaxLength(250)]
    public virtual string? Address { get; private set; }

    [MaxLength(500)]
    public virtual string? FullAddress { get; private set; }

    [MaxLength(250)]
    public virtual string? InvoiceAddress { get; private set; }

    [MaxLength(500)]
    public virtual string? InvoiceFullAddress { get; private set; }

    // Contact Info
    [Required]
    [MaxLength(15)]
    public virtual string Phone { get; private set; } = null!;

    [MaxLength(50)]
    public virtual string? Email { get; private set; }

    [MaxLength(500)]
    public virtual string? Note { get; private set; }

    // Tax and Identity Info
    [MaxLength(50)]
    public virtual string? Tin { get; private set; }

    [MaxLength(25)]
    public virtual string? IdNo { get; private set; }

    [MaxLength(25)]
    public virtual string? PassportNo { get; private set; }

    public virtual DateTime? Dob { get; private set; }

    public virtual ResCustomerSex? Sex { get; private set; }

    // Representative Info (for Organization Type = TC)
    [MaxLength(250)]
    public virtual string? RepName { get; private set; }

    [MaxLength(50)]
    public virtual string? RepEmail { get; private set; }

    [MaxLength(15)]
    public virtual string? RepPhone { get; private set; }

    [MaxLength(25)]
    public virtual string? RepIdNo { get; private set; }

    [MaxLength(250)]
    public virtual string? RepTitle { get; private set; }

    // Authorizer Info (for Organization Type = TC)
    [MaxLength(50)]
    public virtual string? Authorizer { get; private set; }

    [MaxLength(15)]
    public virtual string? AuthorizerPhone { get; private set; }

    [MaxLength(50)]
    public virtual string? AuthorizerEmail { get; private set; }

    [MaxLength(25)]
    public virtual string? AuthorizerNo { get; private set; }

    public virtual DateTime? AuthorizerDate { get; private set; }

    [MaxLength(50)]
    public virtual string? AuthorizerTitle { get; private set; }

    [MaxLength(25)]
    public virtual string? BusinessNo { get; private set; }

    // Navigation Properties
    public virtual ResIndustry? Industry { get; set; }
    public virtual ResProvince? Province { get; set; }
    public virtual ResWard? Ward { get; set; }
    public virtual ResOrganizationType? OrganizationType { get; set; }
    public virtual ResProvince? InvoiceProvince { get; set; }
    public virtual ResWard? InvoiceWard { get; set; }
    public virtual HrEmployee? Sale { get; set; }

    // Collection
    public virtual ICollection<PolicyContracts.PolicyContract> PolicyContracts { get; set; } = new List<PolicyContracts.PolicyContract>();

    protected ResCustomer()
    {
        // For ORM
    }

    public ResCustomer(
        Guid id,
        string? code,
        string name,
        Guid? provinceId,
        Guid? wardId,
        string? address,
        string phone,
        ResCustomerStatus status,
        Guid? industryId = null,
        string? email = null,
        string? note = null,
        Guid? organizationTypeId = null,
        Guid? invoiceProvinceId = null,
        Guid? invoiceWardId = null,
        string? invoiceAddress = null,
        Guid? saleId = null,
        string? tin = null,
        string? idNo = null,
        string? passportNo = null,
        DateTime? dob = null,
        ResCustomerSex? sex = null,
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
        string? businessNo = null,
        string? refCode = null)
        : base(id)
    {
        SetCode(code);
        SetRefCode(refCode);
        SetName(name);
        SetProvinceId(provinceId);
        SetWardId(wardId);
        SetAddress(address);
        SetPhone(phone);
        SetStatus(status);
        SetIndustryId(industryId);
        SetEmail(email);
        SetNote(note);
        SetOrganizationTypeId(organizationTypeId);
        SetInvoiceProvinceId(invoiceProvinceId);
        SetInvoiceWardId(invoiceWardId);
        SetInvoiceAddress(invoiceAddress);
        SetSaleId(saleId);
        SetTin(tin);
        SetIdNo(idNo);
        SetPassportNo(passportNo);
        SetDob(dob);
        SetSex(sex);
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
        
        // FullAddress sẽ được set sau khi có ward và province names
        FullAddress = address; // Temporary, sẽ được update trong AppService
    }

    private void SetCode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            Code = null;
            return;
        }

        if (code.Length > 25)
        {
            throw new ArgumentException("Code cannot exceed 25 characters.", nameof(code));
        }

        // Validate format: A-Z, 0-9, _ (uppercase)
        var upperCode = code.ToUpperInvariant();
        if (!Regex.IsMatch(upperCode, @"^[A-Z0-9_]+$"))
        {
            throw new ArgumentException("Code can only contain uppercase letters (A-Z), numbers (0-9) and underscore (_).", nameof(code));
        }

        Code = upperCode;
    }

    private void SetRefCode(string? refCode)
    {
        if (string.IsNullOrWhiteSpace(refCode))
        {
            RefCode = null;
            return;
        }

        var trimmed = refCode.Trim();
        if (trimmed.Length > 50)
        {
            throw new ArgumentException("RefCode cannot exceed 50 characters.", nameof(refCode));
        }

        RefCode = trimmed;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be empty.", nameof(name));
        }

        if (name.Length > 250)
        {
            throw new ArgumentException("Name cannot exceed 250 characters.", nameof(name));
        }

        Name = name;
    }

    private void SetStatus(ResCustomerStatus status)
    {
        Status = status;
    }

    private void SetIndustryId(Guid? industryId)
    {
        IndustryId = industryId;
    }

    private void SetProvinceId(Guid? provinceId)
    {
        ProvinceId = provinceId is null || provinceId == Guid.Empty ? null : provinceId;
    }

    private void SetWardId(Guid? wardId)
    {
        WardId = wardId is null || wardId == Guid.Empty ? null : wardId;
    }

    private void SetAddress(string? address)
    {
        if (!string.IsNullOrWhiteSpace(address) && address.Length > 250)
        {
            throw new ArgumentException("Address cannot exceed 250 characters.", nameof(address));
        }

        Address = address;
    }

    private void SetPhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            throw new ArgumentException("Phone cannot be empty.", nameof(phone));
        }

        if (phone.Length > 15)
        {
            throw new ArgumentException("Phone cannot exceed 15 characters.", nameof(phone));
        }

        Phone = phone;
    }

    private void SetEmail(string? email)
    {
        if (!string.IsNullOrWhiteSpace(email) && email.Length > 50)
        {
            throw new ArgumentException("Email cannot exceed 50 characters.", nameof(email));
        }

        Email = email;
    }

    private void SetNote(string? note)
    {
        if (!string.IsNullOrWhiteSpace(note) && note.Length > 500)
        {
            throw new ArgumentException("Note cannot exceed 500 characters.", nameof(note));
        }

        Note = note;
    }

    private void SetOrganizationTypeId(Guid? organizationTypeId)
    {
        OrganizationTypeId = organizationTypeId;
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

    private void SetSaleId(Guid? saleId)
    {
        SaleId = saleId;
    }

    private void SetTin(string? tin)
    {
        if (!string.IsNullOrWhiteSpace(tin) && tin.Length > 50)
        {
            throw new ArgumentException("Tin cannot exceed 50 characters.", nameof(tin));
        }

        Tin = tin;
    }

    private void SetIdNo(string? idNo)
    {
        if (!string.IsNullOrWhiteSpace(idNo) && idNo.Length > 25)
        {
            throw new ArgumentException("IdNo cannot exceed 25 characters.", nameof(idNo));
        }

        IdNo = idNo;
    }

    private void SetPassportNo(string? passportNo)
    {
        if (!string.IsNullOrWhiteSpace(passportNo) && passportNo.Length > 25)
        {
            throw new ArgumentException("PassportNo cannot exceed 25 characters.", nameof(passportNo));
        }

        PassportNo = passportNo;
    }

    private void SetDob(DateTime? dob)
    {
        Dob = dob;
    }

    private void SetSex(ResCustomerSex? sex)
    {
        Sex = sex;
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

    // Public update methods (KHÔNG có UpdateCode - Code is immutable)
    public virtual void UpdateRefCode(string? refCode)
    {
        SetRefCode(refCode);
    }

    public virtual void UpdateName(string name)
    {
        SetName(name);
    }

    public virtual void UpdateStatus(ResCustomerStatus status)
    {
        SetStatus(status);
    }

    public virtual void UpdateIndustryId(Guid? industryId)
    {
        SetIndustryId(industryId);
    }

    public virtual void UpdateProvinceId(Guid? provinceId)
    {
        SetProvinceId(provinceId);
    }

    public virtual void UpdateWardId(Guid? wardId)
    {
        SetWardId(wardId);
    }

    public virtual void UpdateAddress(string? address)
    {
        SetAddress(address);
    }

    public virtual void UpdateFullAddress(string? wardName, string? provinceName)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(Address))
        {
            parts.Add(Address);
        }
        if (!string.IsNullOrWhiteSpace(wardName))
        {
            parts.Add(wardName);
        }
        if (!string.IsNullOrWhiteSpace(provinceName))
        {
            parts.Add(provinceName);
        }
        
        if (parts.Count == 0)
        {
            FullAddress = null;
            return;
        }

        var fullAddress = string.Join(", ", parts);
        
        if (fullAddress.Length > 500)
        {
            throw new ArgumentException("FullAddress cannot exceed 500 characters.", nameof(fullAddress));
        }

        FullAddress = fullAddress;
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

    public virtual void UpdateInvoiceFullAddress(string? invoiceWardName, string? invoiceProvinceName)
    {
        if (string.IsNullOrWhiteSpace(InvoiceAddress))
        {
            InvoiceFullAddress = null;
            return;
        }

        var parts = new List<string> { InvoiceAddress };
        if (!string.IsNullOrWhiteSpace(invoiceWardName))
        {
            parts.Add(invoiceWardName);
        }
        if (!string.IsNullOrWhiteSpace(invoiceProvinceName))
        {
            parts.Add(invoiceProvinceName);
        }
        var invoiceFullAddress = string.Join(", ", parts);
        
        if (invoiceFullAddress.Length > 500)
        {
            throw new ArgumentException("InvoiceFullAddress cannot exceed 500 characters.", nameof(invoiceFullAddress));
        }

        InvoiceFullAddress = invoiceFullAddress;
    }

    public virtual void UpdatePhone(string phone)
    {
        SetPhone(phone);
    }

    public virtual void UpdateEmail(string? email)
    {
        SetEmail(email);
    }

    public virtual void UpdateNote(string? note)
    {
        SetNote(note);
    }

    public virtual void UpdateOrganizationTypeId(Guid? organizationTypeId)
    {
        SetOrganizationTypeId(organizationTypeId);
    }

    public virtual void UpdateSaleId(Guid? saleId)
    {
        SetSaleId(saleId);
    }

    public virtual void UpdateTin(string? tin)
    {
        SetTin(tin);
    }

    public virtual void UpdateIdNo(string? idNo)
    {
        SetIdNo(idNo);
    }

    public virtual void UpdatePassportNo(string? passportNo)
    {
        SetPassportNo(passportNo);
    }

    public virtual void UpdateDob(DateTime? dob)
    {
        SetDob(dob);
    }

    public virtual void UpdateSex(ResCustomerSex? sex)
    {
        SetSex(sex);
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
}

