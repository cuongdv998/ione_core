using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResDocuments;

public class ResDocumentDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ResDocument:GroupCode")]
    public string? GroupCode { get; set; }

    [Display(Name = "ResDocument:DocumentTypeId")]
    public Guid DocumentTypeId { get; set; }

    [Display(Name = "ResDocument:FileSize")]
    public long FileSize { get; set; }

    [Display(Name = "ResDocument:FileName")]
    public string FileName { get; set; } = null!;

    [Display(Name = "ResDocument:StoreFileName")]
    public string StoreFileName { get; set; } = null!;

    [Display(Name = "ResDocument:Url")]
    public string? Url { get; set; }

    [Display(Name = "ResDocument:ThumbnailUrl")]
    public string? ThumbnailUrl { get; set; }

    [Display(Name = "ResDocument:Checksum")]
    public string? Checksum { get; set; }

    [Display(Name = "ResDocument:MimeType")]
    public string MimeType { get; set; } = null!;

    [Display(Name = "ResDocument:BucketName")]
    public string BucketName { get; set; } = null!;

    [Display(Name = "ResDocument:VersionId")]
    public string? VersionId { get; set; }
}
