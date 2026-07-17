using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResDocumentTypes;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResDocumentTypes;

public class ResDocumentTypeDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "Master::ResDocumentType:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "Master::ResDocumentType:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "Master::ResDocumentType:Description")]
    public string? Description { get; set; }

    [Display(Name = "Master::ResDocumentType:Status")]
    public ResDocumentTypeStatus Status { get; set; }

    [Display(Name = "Master::ResDocumentType:Bucket")]
    public string? Bucket { get; set; }

    [Display(Name = "Master::ResDocumentType:DocumentGroupCode")]
    public string? DocumentGroupCode { get; set; }
}

