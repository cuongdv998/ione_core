using System;
using System.ComponentModel.DataAnnotations;

namespace iOne.Policy.Policies;

public class UpdatePrevEffectDateInputDto
{
    [Required]
    public Guid PolicyVersionId { get; set; }
}

