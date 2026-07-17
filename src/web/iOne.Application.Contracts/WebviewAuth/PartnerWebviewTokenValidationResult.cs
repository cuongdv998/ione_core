namespace iOne.WebviewAuth;

public class PartnerWebviewTokenValidationResult
{
    public bool Success { get; set; }
    public string? CustomerCode { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerAddress { get; set; }

    public static PartnerWebviewTokenValidationResult Failed() => new() { Success = false };
}
