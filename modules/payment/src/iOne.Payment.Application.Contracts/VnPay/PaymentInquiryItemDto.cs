using System;
using iOne.AccountPaymentRequests;

namespace iOne.Payment.VnPay;

public class PaymentInquiryItemDto
{
    public Guid PaymentId { get; set; }

    public Guid? PolicyId { get; set; }

    public decimal Amount { get; set; }

    public DateTime IssueDate { get; set; }

    public DateTime DueDate { get; set; }

    public DateTime CreationTime { get; set; }

    public AccountPaymentRequestStatus Status { get; set; }

    public string? PaymentProvider { get; set; }

    public string? TransRef { get; set; }
}
