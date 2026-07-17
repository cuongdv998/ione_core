using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace iOne.Master.ResDocuments;

public class UploadMultipleFilesDto
{
    [Required]
    [Display(Name = "ResDocument:DocumentTypeId")]
    public Guid DocumentTypeId { get; set; }

    [Display(Name = "ResDocument:GroupCode")]
    [MaxLength(50)]
    public string? GroupCode { get; set; }
}
