using System;
using System.ComponentModel.DataAnnotations;

namespace iOne.Policy.Payments;

/// <summary>
/// Input cho POST /api/payment/create
/// </summary>
public class CreatePaymentRequestInput
{
    /// <summary>
    /// Phương thức thanh toán (res_payment_method). Dùng khi không gửi <see cref="PaymentMethodCode"/> hoặc code trống.
    /// </summary>
    public Guid? PaymentMethodId { get; set; }

    /// <summary>
    /// Mã phương thức thanh toán (res_payment_method.code). Nếu có giá trị (sau trim) thì được ưu tiên hơn <see cref="PaymentMethodId"/>.
    /// </summary>
    public string? PaymentMethodCode { get; set; }

    [Required]
    [Range(0.001, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }

    [Required]
    public DateTime PaymentDate { get; set; }

    /// <summary>
    /// Loại thanh toán (res_payment_type). Dùng khi không gửi <see cref="PaymentTypeCode"/> hoặc code trống.
    /// </summary>
    public Guid? PaymentTypeId { get; set; }

    /// <summary>
    /// Mã loại thanh toán (res_payment_type.code). Nếu có giá trị (sau trim) thì được ưu tiên hơn <see cref="PaymentTypeId"/>.
    /// </summary>
    public string? PaymentTypeCode { get; set; }

    /// <summary>
    /// Mã tham chiếu giao dịch (lưu account_payment_request.trans_ref).
    /// </summary>
    [MaxLength(255)]
    public string? TransRef { get; set; }

    /// <summary>
    /// Nhà cung cấp thanh toán (lưu account_payment_request.payment_provider).
    /// </summary>
    [MaxLength(255)]
    public string? PaymentProvider { get; set; }

    /// <summary>
    /// Thanh toán online: tính tổng phải thu và FIFO trên <c>policy_amount</c> thuộc phiên bản đơn <b>Active</b> và <b>Draft</b>.
    /// Mặc định false — chỉ phiên bản Active.
    /// </summary>
    public bool IsPaymentOnline { get; set; }
}
