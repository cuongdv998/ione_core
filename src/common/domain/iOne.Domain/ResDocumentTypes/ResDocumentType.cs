using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.ResDocumentTypes;

[Table("res_document_type")]
public class ResDocumentType : FullAuditedAggregateRoot<Guid>
{
    [Required]
    [MaxLength(25)]
    public virtual string Code { get; private set; } = null!;

    [Required]
    [MaxLength(250)]
    public virtual string Name { get; private set; } = null!;

    [MaxLength(500)]
    public virtual string? Description { get; private set; }

    [Required]
    public virtual ResDocumentTypeStatus Status { get; private set; }

    [MaxLength(50)]
    public virtual string? Bucket { get; private set; }

    [MaxLength(50)]
    public virtual string? DocumentGroupCode { get; private set; }

    protected ResDocumentType()
    {
        // For ORM
    }

    public ResDocumentType(
        Guid id,
        string code,
        string name,
        ResDocumentTypeStatus status,
        string? description = null,
        string? bucket = null,
        string? documentGroupCode = null)
        : base(id)
    {
        SetCode(code);
        SetName(name);
        SetStatus(status);
        SetDescription(description);
        SetBucket(bucket);
        SetDocumentGroupCode(documentGroupCode);
    }

    private void SetCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Code cannot be null or empty.", nameof(code));
        }

        if (code.Length > 25)
        {
            throw new ArgumentException("Code cannot exceed 25 characters.", nameof(code));
        }

        // Convert to uppercase and validate format: only A-Z, 0-9, and underscore
        var upperCode = code.ToUpperInvariant();
        if (!Regex.IsMatch(upperCode, @"^[A-Z0-9_]+$"))
        {
            throw new ArgumentException("Code can only contain letters (A-Z), numbers (0-9), and underscore (_).", nameof(code));
        }

        Code = upperCode;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));
        }

        if (name.Length > 250)
        {
            throw new ArgumentException("Name cannot exceed 250 characters.", nameof(name));
        }

        Name = name;
    }

    private void SetDescription(string? description)
    {
        if (!string.IsNullOrWhiteSpace(description) && description.Length > 500)
        {
            throw new ArgumentException("Description cannot exceed 500 characters.", nameof(description));
        }

        Description = description;
    }

    private void SetStatus(ResDocumentTypeStatus status)
    {
        Status = status;
    }

    private void SetBucket(string? bucket)
    {
        if (!string.IsNullOrWhiteSpace(bucket) && bucket.Length > 50)
        {
            throw new ArgumentException("Bucket cannot exceed 50 characters.", nameof(bucket));
        }

        Bucket = bucket;
    }

    private void SetDocumentGroupCode(string? documentGroupCode)
    {
        if (string.IsNullOrWhiteSpace(documentGroupCode))
        {
            DocumentGroupCode = null;
            return;
        }

        var trimmed = documentGroupCode.Trim();
        if (trimmed.Length > 50)
        {
            throw new ArgumentException("Document group code cannot exceed 50 characters.", nameof(documentGroupCode));
        }

        DocumentGroupCode = trimmed;
    }

    // ⚠️ QUAN TRỌNG: Không có method UpdateCode() - Code không được phép sửa
    public virtual void UpdateName(string name)
    {
        SetName(name);
    }

    public virtual void UpdateDescription(string? description)
    {
        SetDescription(description);
    }

    public virtual void UpdateStatus(ResDocumentTypeStatus status)
    {
        SetStatus(status);
    }

    public virtual void UpdateBucket(string? bucket)
    {
        SetBucket(bucket);
    }

    public virtual void UpdateDocumentGroupCode(string? documentGroupCode)
    {
        SetDocumentGroupCode(documentGroupCode);
    }
}

