using System;
using System.ComponentModel.DataAnnotations;

namespace iOne.Payment.Payments;

public class PaymentInquiryByIdInput
{
    [Required]
    public Guid PaymentId { get; set; }
}
