namespace iOne.Payment.VnPay;

public class VnPayOptions
{
    public string TmnCode { get; set; } = string.Empty;
    public string HashSecret { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string QueryApiUrl { get; set; } = "https://sandbox.vnpayment.vn/merchant_webapi/api/transaction";
    public string ReturnUrl { get; set; } = string.Empty;
    public string Version { get; set; } = "2.1.0";
    public string Command { get; set; } = "pay";
}
