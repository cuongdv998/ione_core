using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Volo.Abp.Domain.Entities.Auditing;

namespace iOne.ResSequences;

[Table("res_sequence")]
public class ResSequence : FullAuditedAggregateRoot<Guid>
{
    [Required]
    [MaxLength(25)]
    public virtual string Code { get; private set; } = null!;

    [Required]
    [MaxLength(250)]
    public virtual string Name { get; private set; } = null!;

    [MaxLength(150)]
    public virtual string? Prefix { get; private set; }

    [MaxLength(150)]
    public virtual string? Suffix { get; private set; }

    [Required]
    public virtual ResSequenceType Type { get; private set; }

    [Range(0, 99)]
    public virtual int? Padding { get; private set; }

    [Required]
    [Range(0, 9999999999)]
    public virtual long NumberNext { get; private set; }

    [Required]
    [Range(1, 9)]
    public virtual int NumberIncrement { get; private set; }

    [Required]
    public virtual ResSequenceUseDateRange UseDateRange { get; private set; } = ResSequenceUseDateRange.No;

    public virtual ResSequenceDateRangeType? DateRangeType { get; private set; }

    [Required]
    public virtual ResSequenceStatus Status { get; private set; }

    protected ResSequence()
    {
        // For ORM
    }

    public ResSequence(
        Guid id,
        string code,
        string name,
        string? prefix,
        string? suffix,
        ResSequenceType type,
        int? padding,
        long numberNext,
        int numberIncrement,
        ResSequenceUseDateRange useDateRange,
        ResSequenceDateRangeType? dateRangeType,
        ResSequenceStatus status)
        : base(id)
    {
        SetCode(code);
        SetName(name);
        SetPrefix(prefix);
        SetSuffix(suffix);
        SetType(type);
        SetPadding(padding);
        SetNumberNext(numberNext);
        SetNumberIncrement(numberIncrement);
        SetUseDateRange(useDateRange);
        SetDateRangeType(dateRangeType);
        SetStatus(status);
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

    private void SetPrefix(string? prefix)
    {
        if (prefix != null && prefix.Length > 150)
        {
            throw new ArgumentException("Prefix cannot exceed 150 characters.", nameof(prefix));
        }

        Prefix = prefix;
    }

    private void SetSuffix(string? suffix)
    {
        if (suffix != null && suffix.Length > 150)
        {
            throw new ArgumentException("Suffix cannot exceed 150 characters.", nameof(suffix));
        }

        Suffix = suffix;
    }

    private void SetType(ResSequenceType type)
    {
        Type = type;
    }

    private void SetPadding(int? padding)
    {
        if (padding.HasValue && (padding.Value < 0 || padding.Value > 99))
        {
            throw new ArgumentException("Padding must be between 0 and 99.", nameof(padding));
        }

        Padding = padding;
    }

    private void SetNumberNext(long numberNext)
    {
        if (numberNext < 0 || numberNext > 9999999999)
        {
            throw new ArgumentException("NumberNext must be between 0 and 9999999999.", nameof(numberNext));
        }

        NumberNext = numberNext;
    }

    private void SetNumberIncrement(int numberIncrement)
    {
        if (numberIncrement < 1 || numberIncrement > 9)
        {
            throw new ArgumentException("NumberIncrement must be between 1 and 9.", nameof(numberIncrement));
        }

        NumberIncrement = numberIncrement;
    }

    private void SetUseDateRange(ResSequenceUseDateRange useDateRange)
    {
        UseDateRange = useDateRange;
    }

    private void SetDateRangeType(ResSequenceDateRangeType? dateRangeType)
    {
        DateRangeType = dateRangeType;
    }

    private void SetStatus(ResSequenceStatus status)
    {
        Status = status;
    }

    // ⚠️ QUAN TRỌNG: Không có method UpdateCode() - Code không được phép sửa
    public virtual void UpdateName(string name)
    {
        SetName(name);
    }

    public virtual void UpdatePrefix(string? prefix)
    {
        SetPrefix(prefix);
    }

    public virtual void UpdateSuffix(string? suffix)
    {
        SetSuffix(suffix);
    }

    public virtual void UpdateType(ResSequenceType type)
    {
        SetType(type);
    }

    public virtual void UpdatePadding(int? padding)
    {
        SetPadding(padding);
    }

    public virtual void UpdateNumberNext(long numberNext)
    {
        SetNumberNext(numberNext);
    }

    public virtual void UpdateNumberIncrement(int numberIncrement)
    {
        SetNumberIncrement(numberIncrement);
    }

    public virtual void UpdateUseDateRange(ResSequenceUseDateRange useDateRange)
    {
        SetUseDateRange(useDateRange);
    }

    public virtual void UpdateDateRangeType(ResSequenceDateRangeType? dateRangeType)
    {
        SetDateRangeType(dateRangeType);
    }

    public virtual void UpdateStatus(ResSequenceStatus status)
    {
        SetStatus(status);
    }
}

