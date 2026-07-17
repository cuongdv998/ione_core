using System;
using System.Collections.Generic;

namespace iOne.Policy.Payments;

/// <summary>
/// DTO cho cấu hình thanh toán - dùng cho GET /api/payment/config
/// </summary>
public class PaymentConfigDto
{
    /// <summary>
    /// Danh sách hình thức thanh toán (res_payment_method), sắp xếp theo name A-Z
    /// </summary>
    public List<PaymentMethodSelectDto> PaymentMethods { get; set; } = new();

    /// <summary>
    /// Số tiền cần thanh toán: tổng đơn hàng - tổng đã thanh toán
    /// </summary>
    public decimal AmountToPay { get; set; }

    /// <summary>
    /// Loại thanh toán công nợ từ res_payment_type
    /// </summary>
    public List<PaymentTypeSelectDto> PaymentTypes { get; set; } = new();
}

public class PaymentMethodSelectDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
}

public class PaymentTypeSelectDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
}
