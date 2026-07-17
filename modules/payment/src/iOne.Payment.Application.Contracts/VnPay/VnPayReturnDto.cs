using System.Collections.Generic;

namespace iOne.Payment.VnPay;

public class VnPayReturnDto
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public VnPayReturnDataDto? Data { get; set; }
}

public class VnPayReturnDataDto
{
    public string TxnRef { get; set; } = string.Empty;
    public string Amount { get; set; } = string.Empty;
    public string BankCode { get; set; } = string.Empty;
    public string BankTranNo { get; set; } = string.Empty;
    public string CardType { get; set; } = string.Empty;
    public string TransactionNo { get; set; } = string.Empty;
    public string PayDate { get; set; } = string.Empty;
    public string ResponseCode { get; set; } = string.Empty;
}
