namespace iOne.Payment.VnPay;

public class VnPayQueryDrResponseDto
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsSignatureValid { get; set; }
    public string? ResponseId { get; set; }
    public string? Command { get; set; }
    public string? TmnCode { get; set; }
    public string? TxnRef { get; set; }
    public string? Amount { get; set; }
    public string? OrderInfo { get; set; }
    public string? ResponseCode { get; set; }
    public string? VnpMessage { get; set; }
    public string? BankCode { get; set; }
    public string? PayDate { get; set; }
    public string? TransactionNo { get; set; }
    public string? TransactionType { get; set; }
    public string? TransactionStatus { get; set; }
    public string? PromotionCode { get; set; }
    public string? PromotionAmount { get; set; }
}
