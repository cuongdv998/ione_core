using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using iOne.ResDocumentTypes;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.ResDocuments;

[Table("res_document")]
public class ResDocument : AuditedAggregateRoot<Guid>
{
    [MaxLength(50)]
    public virtual string? GroupCode { get; private set; }

    [Required]
    public virtual Guid DocumentTypeId { get; private set; }

    // Navigation property
    public virtual ResDocumentType? DocumentType { get; set; }

    [Required]
    public virtual long FileSize { get; private set; }

    [Required]
    [MaxLength(250)]
    public virtual string FileName { get; private set; } = null!;

    [Required]
    [MaxLength(250)]
    public virtual string StoreFileName { get; private set; } = null!;

    [MaxLength(500)]
    public virtual string? Url { get; private set; }

    [MaxLength(250)]
    public virtual string? ThumbnailUrl { get; private set; }

    [MaxLength(40)]
    public virtual string? Checksum { get; private set; }

    [Required]
    [MaxLength(200)]
    public virtual string MimeType { get; private set; } = null!;

    [Required]
    [MaxLength(50)]
    public virtual string BucketName { get; private set; } = null!;

    [MaxLength(150)]
    public virtual string? VersionId { get; private set; }

    protected ResDocument()
    {
        // For ORM
    }

    public ResDocument(
        Guid id,
        Guid documentTypeId,
        long fileSize,
        string fileName,
        string storeFileName,
        string mimeType,
        string bucketName,
        string? groupCode = null,
        string? url = null,
        string? thumbnailUrl = null,
        string? checksum = null,
        string? versionId = null)
        : base(id)
    {
        SetDocumentTypeId(documentTypeId);
        SetFileSize(fileSize);
        SetFileName(fileName);
        SetStoreFileName(storeFileName);
        SetMimeType(mimeType);
        SetBucketName(bucketName);
        SetGroupCode(groupCode);
        SetUrl(url);
        SetThumbnailUrl(thumbnailUrl);
        SetChecksum(checksum);
        SetVersionId(versionId);
    }

    private void SetGroupCode(string? groupCode)
    {
        if (!string.IsNullOrWhiteSpace(groupCode) && groupCode.Length > 50)
        {
            throw new ArgumentException("GroupCode cannot exceed 50 characters.", nameof(groupCode));
        }

        GroupCode = groupCode;
    }

    private void SetDocumentTypeId(Guid documentTypeId)
    {
        if (documentTypeId == Guid.Empty)
        {
            throw new ArgumentException("DocumentTypeId cannot be empty.", nameof(documentTypeId));
        }

        DocumentTypeId = documentTypeId;
    }

    private void SetFileSize(long fileSize)
    {
        if (fileSize < 0)
        {
            throw new ArgumentException("FileSize cannot be negative.", nameof(fileSize));
        }

        FileSize = fileSize;
    }

    private void SetFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("FileName cannot be null or empty.", nameof(fileName));
        }

        if (fileName.Length > 250)
        {
            throw new ArgumentException("FileName cannot exceed 250 characters.", nameof(fileName));
        }

        FileName = fileName;
    }

    private void SetStoreFileName(string storeFileName)
    {
        if (string.IsNullOrWhiteSpace(storeFileName))
        {
            throw new ArgumentException("StoreFileName cannot be null or empty.", nameof(storeFileName));
        }

        if (storeFileName.Length > 250)
        {
            throw new ArgumentException("StoreFileName cannot exceed 250 characters.", nameof(storeFileName));
        }

        StoreFileName = storeFileName;
    }

    private void SetUrl(string? url)
    {
        if (!string.IsNullOrWhiteSpace(url) && url.Length > 500)
        {
            throw new ArgumentException("Url cannot exceed 500 characters.", nameof(url));
        }

        Url = url;
    }

    private void SetThumbnailUrl(string? thumbnailUrl)
    {
        if (!string.IsNullOrWhiteSpace(thumbnailUrl) && thumbnailUrl.Length > 250)
        {
            throw new ArgumentException("ThumbnailUrl cannot exceed 250 characters.", nameof(thumbnailUrl));
        }

        ThumbnailUrl = thumbnailUrl;
    }

    private void SetChecksum(string? checksum)
    {
        if (!string.IsNullOrWhiteSpace(checksum) && checksum.Length > 40)
        {
            throw new ArgumentException("Checksum cannot exceed 40 characters.", nameof(checksum));
        }

        Checksum = checksum;
    }

    private void SetMimeType(string mimeType)
    {
        if (string.IsNullOrWhiteSpace(mimeType))
        {
            throw new ArgumentException("MimeType cannot be null or empty.", nameof(mimeType));
        }

        if (mimeType.Length > 200)
        {
            throw new ArgumentException("MimeType cannot exceed 200 characters.", nameof(mimeType));
        }

        MimeType = mimeType;
    }

    private void SetBucketName(string bucketName)
    {
        if (string.IsNullOrWhiteSpace(bucketName))
        {
            throw new ArgumentException("BucketName cannot be null or empty.", nameof(bucketName));
        }

        if (bucketName.Length > 50)
        {
            throw new ArgumentException("BucketName cannot exceed 50 characters.", nameof(bucketName));
        }

        BucketName = bucketName;
    }

    private void SetVersionId(string? versionId)
    {
        if (!string.IsNullOrWhiteSpace(versionId) && versionId.Length > 150)
        {
            throw new ArgumentException("VersionId cannot exceed 150 characters.", nameof(versionId));
        }

        VersionId = versionId;
    }

    // Update methods
    public virtual void UpdateGroupCode(string? groupCode)
    {
        SetGroupCode(groupCode);
    }

    public virtual void UpdateDocumentTypeId(Guid documentTypeId)
    {
        SetDocumentTypeId(documentTypeId);
    }

    public virtual void UpdateFileSize(long fileSize)
    {
        SetFileSize(fileSize);
    }

    public virtual void UpdateFileName(string fileName)
    {
        SetFileName(fileName);
    }

    public virtual void UpdateStoreFileName(string storeFileName)
    {
        SetStoreFileName(storeFileName);
    }

    public virtual void UpdateUrl(string? url)
    {
        SetUrl(url);
    }

    public virtual void UpdateThumbnailUrl(string? thumbnailUrl)
    {
        SetThumbnailUrl(thumbnailUrl);
    }

    public virtual void UpdateChecksum(string? checksum)
    {
        SetChecksum(checksum);
    }

    public virtual void UpdateMimeType(string mimeType)
    {
        SetMimeType(mimeType);
    }

    public virtual void UpdateBucketName(string bucketName)
    {
        SetBucketName(bucketName);
    }

    public virtual void UpdateVersionId(string? versionId)
    {
        SetVersionId(versionId);
    }
}

