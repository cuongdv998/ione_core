using System;
using System.ComponentModel.DataAnnotations;

namespace iOne.Policy.Policies;

public class UpdatePolicyDocumentInputDto
{
    [Required(ErrorMessage = "PolicyDocument:DocumentIdRequired")]
    [Display(Name = "PolicyDocument:DocumentId")]
    public Guid DocumentId { get; set; }
}

