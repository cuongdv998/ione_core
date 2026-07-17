using System;

namespace iOne.Policy.Payments;

/// <summary>
/// DTO trả về khi tạo payment request thành công
/// </summary>
public class AccountPaymentRequestDto
{
    public Guid Id { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? PolicyId { get; set; }
    public DateTime IssueDate { get; set; }
    public Guid PaymentMethodId { get; set; }
    public Guid PaymentTypeId { get; set; }
    public Guid CurrencyId { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = null!;
    public DateTime? SubmittedDate { get; set; }
    public Guid? SubmitterId { get; set; }
    public string? TransRef { get; set; }
    public string? PaymentProvider { get; set; }
}
