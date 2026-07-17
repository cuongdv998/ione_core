using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using iOne.AccountPaymentRequests;
using iOne.Claims;
using iOne.HrEmployees;
using iOne.ResCurrencies;
using iOne.ResPartners;
using iOne.ResPaymentMethods;
using iOne.ResPaymentTypes;
using iOne.ResReasons;
using iOne.ResCustomers;

namespace iOne.AccountPaymentRequests;

[Table("account_payment_request")]
public class AccountPaymentRequest : AuditedAggregateRoot<Guid>
{
    /// <summary>Khách hàng nhận thanh toán</summary>
    public virtual Guid? CustomerId { get; private set; }

    /// <summary>Đối tác nhận thanh toán</summary>
    public virtual Guid? PartnerId { get; private set; }

    [MaxLength(1000)]
    public virtual string? Description { get; private set; }

    /// <summary>Ngày thanh toán</summary>
    [Required]
    public virtual DateTime IssueDate { get; private set; }

    /// <summary>Phương thức thanh toán</summary>
    [Required]
    public virtual Guid PaymentMethodId { get; private set; }

    /// <summary>Loại thanh toán (Tạm ứng, Thanh toán công nợ)</summary>
    [Required]
    public virtual Guid PaymentTypeId { get; private set; }

    /// <summary>Tiền tệ</summary>
    [Required]
    public virtual Guid CurrencyId { get; private set; }

    /// <summary>ID của claim folder liên quan</summary>
    public virtual Guid? ClaimFolderId { get; private set; }

    /// <summary>ID đơn bảo hiểm liên quan (thanh toán công nợ)</summary>
    public virtual Guid? PolicyId { get; private set; }

    /// <summary>Hạn thanh toán</summary>
    [Required]
    public virtual DateTime DueDate { get; private set; }

    /// <summary>Tổng giá trị thanh toán</summary>
    [Required]
    public virtual decimal Amount { get; private set; }

    [Required]
    public virtual AccountPaymentRequestStatus Status { get; private set; }

    /// <summary>Ngày trình</summary>
    public virtual DateTime? SubmittedDate { get; private set; }

    /// <summary>Người trình</summary>
    public virtual Guid? SubmitterId { get; private set; }

    /// <summary>Người duyệt</summary>
    public virtual Guid? ApproveEmpId { get; private set; }

    /// <summary>Ngày duyệt</summary>
    public virtual DateTime? ApprovedDate { get; private set; }

    /// <summary>Lý do (khi từ chối duyệt)</summary>
    public virtual Guid? ReasonId { get; private set; }

    /// <summary>Mô tả khi chọn lý do</summary>
    [MaxLength(250)]
    public virtual string? ReasonDescription { get; private set; }

    /// <summary>Mã tham chiếu giao dịch (payment gateway / đối tác).</summary>
    [MaxLength(255)]
    public virtual string? TransRef { get; private set; }

    /// <summary>Nhà cung cấp thanh toán (VNPAY, MOMO, ...).</summary>
    [MaxLength(255)]
    public virtual string? PaymentProvider { get; private set; }

    // Navigation properties
    public virtual ResCustomer? Customer { get; set; }
    public virtual ResPartner? Partner { get; set; }
    public virtual ResPaymentMethod? PaymentMethod { get; set; }
    public virtual ResPaymentType? PaymentType { get; set; }
    public virtual ResCurrency? Currency { get; set; }
    public virtual Claim? ClaimFolder { get; set; }
    public virtual HrEmployee? Submitter { get; set; }
    public virtual HrEmployee? ApproveEmp { get; set; }
    public virtual ResReason? Reason { get; set; }

    protected AccountPaymentRequest()
    {
    }

    public AccountPaymentRequest(
        Guid id,
        DateTime issueDate,
        Guid paymentMethodId,
        Guid paymentTypeId,
        Guid currencyId,
        DateTime dueDate,
        decimal amount,
        AccountPaymentRequestStatus status,
        Guid? customerId = null,
        Guid? partnerId = null,
        string? description = null,
        Guid? claimFolderId = null,
        Guid? policyId = null,
        DateTime? submittedDate = null,
        Guid? submitterId = null,
        Guid? approveEmpId = null,
        DateTime? approvedDate = null,
        Guid? reasonId = null,
        string? reasonDescription = null,
        string? transRef = null,
        string? paymentProvider = null)
        : base(id)
    {
        CustomerId = customerId;
        PartnerId = partnerId;
        Description = description;
        IssueDate = issueDate;
        PaymentMethodId = paymentMethodId;
        PaymentTypeId = paymentTypeId;
        CurrencyId = currencyId;
        ClaimFolderId = claimFolderId;
        PolicyId = policyId;
        DueDate = dueDate;
        Amount = amount;
        Status = status;
        SubmittedDate = submittedDate;
        SubmitterId = submitterId;
        ApproveEmpId = approveEmpId;
        ApprovedDate = approvedDate;
        ReasonId = reasonId;
        ReasonDescription = reasonDescription;
        TransRef = transRef;
        PaymentProvider = paymentProvider;
    }

    public virtual void UpdateDescription(string? description)
    {
        Description = description;
    }

    public virtual void UpdateIssueDate(DateTime issueDate)
    {
        IssueDate = issueDate;
    }

    public virtual void UpdateDueDate(DateTime dueDate)
    {
        DueDate = dueDate;
    }

    public virtual void UpdateAmount(decimal amount)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative.", nameof(amount));
        Amount = amount;
    }

    public virtual void UpdateStatus(AccountPaymentRequestStatus status)
    {
        Status = status;
    }

    public virtual void UpdateTransRef(string? transRef)
    {
        if (transRef != null && transRef.Length > 255)
        {
            throw new ArgumentException("TransRef cannot exceed 255 characters.", nameof(transRef));
        }

        TransRef = string.IsNullOrWhiteSpace(transRef) ? null : transRef.Trim();
    }

    public virtual void UpdatePaymentProvider(string? paymentProvider)
    {
        if (paymentProvider != null && paymentProvider.Length > 255)
        {
            throw new ArgumentException("PaymentProvider cannot exceed 255 characters.", nameof(paymentProvider));
        }

        PaymentProvider = string.IsNullOrWhiteSpace(paymentProvider) ? null : paymentProvider.Trim();
    }

    public virtual void Submit(DateTime submittedDate, Guid submitterId)
    {
        SubmittedDate = submittedDate;
        SubmitterId = submitterId;
        Status = AccountPaymentRequestStatus.PendingApproval;
    }

    public virtual void Approve(DateTime approvedDate, Guid approveEmpId)
    {
        ApprovedDate = approvedDate;
        ApproveEmpId = approveEmpId;
        Status = AccountPaymentRequestStatus.Approved;
    }

    public virtual void Reject(Guid? reasonId = null, string? reasonDescription = null)
    {
        ReasonId = reasonId;
        ReasonDescription = reasonDescription;
        Status = AccountPaymentRequestStatus.Rejected;
    }

    public virtual void Cancel()
    {
        Status = AccountPaymentRequestStatus.Cancelled;
    }
}
