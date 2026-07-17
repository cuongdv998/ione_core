using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Volo.Abp.Domain.Entities.Auditing;
using iOne.ResCarBrands;
using iOne.ResCarModels;
using iOne.ResCarCategories;

namespace iOne.ResCarCategories;

[Table("res_car_category")]
public class ResCarCategory : FullAuditedAggregateRoot<Guid>
{
    [Required]
    public virtual Guid CarBrandId { get; private set; }

    public virtual Guid? CarModelId { get; private set; }

    [Required]
    public virtual Guid MotorClassId { get; private set; }

    public virtual Guid? CarLineId { get; private set; }

    [Required]
    [MaxLength(50)]
    public virtual string Code { get; private set; } = null!;

    [Required]
    [MaxLength(250)]
    public virtual string Name { get; private set; } = null!;

    [Required]
    public virtual int SeatNumber { get; private set; }

    [MaxLength(500)]
    public virtual string? Description { get; private set; }

    [Required]
    public virtual ResCarCategoryStatus Status { get; private set; }

    // Navigation properties
    public virtual ResCarBrand CarBrand { get; private set; } = null!;
    public virtual ResCarModel? CarModel { get; private set; }

    protected ResCarCategory()
    {
        // For ORM
    }

    public ResCarCategory(
        Guid id,
        Guid carBrandId,
        Guid? carModelId,
        Guid motorClassId,
        Guid? carLineId,
        string code,
        string name,
        int seatNumber,
        string? description,
        ResCarCategoryStatus status)
        : base(id)
    {
        SetCarBrandId(carBrandId);
        SetCarModelId(carModelId);
        SetMotorClassId(motorClassId);
        SetCarLineId(carLineId);
        SetCode(code);
        SetName(name);
        SetSeatNumber(seatNumber);
        SetDescription(description);
        SetStatus(status);
    }

    private void SetCarBrandId(Guid carBrandId)
    {
        if (carBrandId == Guid.Empty)
        {
            throw new ArgumentException("CarBrandId cannot be empty.", nameof(carBrandId));
        }

        CarBrandId = carBrandId;
    }

    private void SetCarModelId(Guid? carModelId)
    {
        if (carModelId.HasValue && carModelId.Value == Guid.Empty)
        {
            throw new ArgumentException("CarModelId cannot be empty if provided.", nameof(carModelId));
        }

        CarModelId = carModelId;
    }

    private void SetMotorClassId(Guid motorClassId)
    {
        if (motorClassId == Guid.Empty)
        {
            throw new ArgumentException("MotorClassId cannot be empty.", nameof(motorClassId));
        }

        MotorClassId = motorClassId;
    }

    private void SetCarLineId(Guid? carLineId)
    {
        if (carLineId.HasValue && carLineId.Value == Guid.Empty)
        {
            throw new ArgumentException("CarLineId cannot be empty if provided.", nameof(carLineId));
        }

        CarLineId = carLineId;
    }

    private void SetCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Code cannot be null or empty.", nameof(code));
        }

        if (code.Length > 50)
        {
            throw new ArgumentException("Code cannot exceed 50 characters.", nameof(code));
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

    private void SetSeatNumber(int seatNumber)
    {
        if (seatNumber < 0 || seatNumber > 99)
        {
            throw new ArgumentException("SeatNumber must be between 0 and 99.", nameof(seatNumber));
        }

        SeatNumber = seatNumber;
    }

    private void SetDescription(string? description)
    {
        if (description != null && description.Length > 500)
        {
            throw new ArgumentException("Description cannot exceed 500 characters.", nameof(description));
        }

        Description = description;
    }

    private void SetStatus(ResCarCategoryStatus status)
    {
        Status = status;
    }

    // ⚠️ QUAN TRỌNG: Không có method UpdateCode() - Code không được phép sửa
    // ⚠️ QUAN TRỌNG: Không có method UpdateCarBrandId() - CarBrandId không được phép sửa
    // ⚠️ QUAN TRỌNG: Không có method UpdateCarModelId() - CarModelId không được phép sửa
    // ⚠️ QUAN TRỌNG: Không có method UpdateMotorClassId() - MotorClassId không được phép sửa
    // ⚠️ QUAN TRỌNG: Không có method UpdateCarLineId() - CarLineId không được phép sửa
    public virtual void UpdateName(string name)
    {
        SetName(name);
    }

    public virtual void UpdateSeatNumber(int seatNumber)
    {
        SetSeatNumber(seatNumber);
    }

    public virtual void UpdateDescription(string? description)
    {
        SetDescription(description);
    }

    public virtual void UpdateStatus(ResCarCategoryStatus status)
    {
        SetStatus(status);
    }
}

