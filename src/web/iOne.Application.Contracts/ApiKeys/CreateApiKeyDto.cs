using System;
using System.ComponentModel.DataAnnotations;

namespace iOne.ApiKeys;

public class CreateApiKeyDto
{
    [Required]
    [StringLength(256)]
    public string Name { get; set; } = null!;

    public DateTime? ExpiresAt { get; set; }
}
