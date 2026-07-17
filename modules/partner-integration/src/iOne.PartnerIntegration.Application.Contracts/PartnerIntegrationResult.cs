namespace iOne.PartnerIntegration;

public class PartnerIntegrationResult
{
    public string ProductCode { get; set; } = "";

    public bool IsSuccess { get; set; }

    public string? ErrorMessage { get; set; }
}
