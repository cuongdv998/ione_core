using System.ComponentModel.DataAnnotations;

namespace iOne.Setting;

public class SendTestEmailInput
{
    [Required]
    [EmailAddress]
    public string To { get; set; } = string.Empty;
}
