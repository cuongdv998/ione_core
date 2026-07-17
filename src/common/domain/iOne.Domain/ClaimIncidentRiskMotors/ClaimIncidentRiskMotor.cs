using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using iOne.ClaimIncidents;

namespace iOne.ClaimIncidentRiskMotors;

[Table("claim_incident_risk_motor")]
public class ClaimIncidentRiskMotor : FullAuditedAggregateRoot<Guid>
{
    // Foreign Keys
    public virtual Guid? IncidentObjectId { get; private set; }

    // Car Info
    [MaxLength(15)]
    public virtual string? CarPlate { get; private set; }

    [MaxLength(25)]
    public virtual string? Vin { get; private set; }

    [MaxLength(25)]
    public virtual string? EngineNumber { get; private set; }

    public virtual int? PersonsOnCar { get; private set; }

    // Driver Info
    [MaxLength(250)]
    public virtual string? DriverName { get; private set; }

    [MaxLength(15)]
    public virtual string? DriverPhone { get; private set; }

    [MaxLength(25)]
    public virtual string? DriverIdNo { get; private set; }

    [MaxLength(50)]
    public virtual string? DriverLicenseNo { get; private set; }

    [MaxLength(5)]
    public virtual string? DriverLicenseLevel { get; private set; }

    public virtual DateTime? DriverLicenseEffectDate { get; private set; }

    public virtual DateTime? DriverLicenseExpireDate { get; private set; }

    [MaxLength(15)]
    public virtual string? DriverSex { get; private set; } // M or F

    // Car Registry Info
    [MaxLength(50)]
    public virtual string? CarRegistryNo { get; private set; }

    public virtual DateTime? CarRegistryEffectDate { get; private set; }

    public virtual DateTime? CarRegistryExpireDate { get; private set; }

    // Navigation Properties
    public virtual ClaimIncident? IncidentObject { get; set; }

    protected ClaimIncidentRiskMotor()
    {
        // For ORM
    }

    public ClaimIncidentRiskMotor(
        Guid id,
        Guid? incidentObjectId = null,
        string? carPlate = null,
        string? vin = null,
        string? engineNumber = null,
        int? personsOnCar = null,
        string? driverName = null,
        string? driverPhone = null,
        string? driverIdNo = null,
        string? driverLicenseNo = null,
        string? driverLicenseLevel = null,
        DateTime? driverLicenseEffectDate = null,
        DateTime? driverLicenseExpireDate = null,
        string? driverSex = null,
        string? carRegistryNo = null,
        DateTime? carRegistryEffectDate = null,
        DateTime? carRegistryExpireDate = null)
        : base(id)
    {
        SetIncidentObjectId(incidentObjectId);
        SetCarPlate(carPlate);
        SetVin(vin);
        SetEngineNumber(engineNumber);
        SetPersonsOnCar(personsOnCar);
        SetDriverName(driverName);
        SetDriverPhone(driverPhone);
        SetDriverIdNo(driverIdNo);
        SetDriverLicenseNo(driverLicenseNo);
        SetDriverLicenseLevel(driverLicenseLevel);
        SetDriverLicenseEffectDate(driverLicenseEffectDate);
        SetDriverLicenseExpireDate(driverLicenseExpireDate);
        SetDriverSex(driverSex);
        SetCarRegistryNo(carRegistryNo);
        SetCarRegistryEffectDate(carRegistryEffectDate);
        SetCarRegistryExpireDate(carRegistryExpireDate);
    }

    private void SetIncidentObjectId(Guid? incidentObjectId)
    {
        IncidentObjectId = incidentObjectId;
    }

    private void SetCarPlate(string? carPlate)
    {
        if (!string.IsNullOrWhiteSpace(carPlate) && carPlate.Length > 15)
        {
            throw new ArgumentException("CarPlate cannot exceed 15 characters.", nameof(carPlate));
        }
        CarPlate = carPlate;
    }

    private void SetVin(string? vin)
    {
        if (!string.IsNullOrWhiteSpace(vin) && vin.Length > 25)
        {
            throw new ArgumentException("Vin cannot exceed 25 characters.", nameof(vin));
        }
        Vin = vin;
    }

    private void SetEngineNumber(string? engineNumber)
    {
        if (!string.IsNullOrWhiteSpace(engineNumber) && engineNumber.Length > 25)
        {
            throw new ArgumentException("EngineNumber cannot exceed 25 characters.", nameof(engineNumber));
        }
        EngineNumber = engineNumber;
    }

    private void SetPersonsOnCar(int? personsOnCar)
    {
        if (personsOnCar.HasValue && personsOnCar.Value < 0)
        {
            throw new ArgumentException("PersonsOnCar cannot be negative.", nameof(personsOnCar));
        }
        if (personsOnCar.HasValue && personsOnCar.Value > 999)
        {
            throw new ArgumentException("PersonsOnCar cannot exceed 999.", nameof(personsOnCar));
        }
        PersonsOnCar = personsOnCar;
    }

    private void SetDriverName(string? driverName)
    {
        if (!string.IsNullOrWhiteSpace(driverName) && driverName.Length > 250)
        {
            throw new ArgumentException("DriverName cannot exceed 250 characters.", nameof(driverName));
        }
        DriverName = driverName;
    }

    private void SetDriverPhone(string? driverPhone)
    {
        if (!string.IsNullOrWhiteSpace(driverPhone) && driverPhone.Length > 15)
        {
            throw new ArgumentException("DriverPhone cannot exceed 15 characters.", nameof(driverPhone));
        }
        DriverPhone = driverPhone;
    }

    private void SetDriverIdNo(string? driverIdNo)
    {
        if (!string.IsNullOrWhiteSpace(driverIdNo) && driverIdNo.Length > 25)
        {
            throw new ArgumentException("DriverIdNo cannot exceed 25 characters.", nameof(driverIdNo));
        }
        DriverIdNo = driverIdNo;
    }

    private void SetDriverLicenseNo(string? driverLicenseNo)
    {
        if (!string.IsNullOrWhiteSpace(driverLicenseNo) && driverLicenseNo.Length > 50)
        {
            throw new ArgumentException("DriverLicenseNo cannot exceed 50 characters.", nameof(driverLicenseNo));
        }
        DriverLicenseNo = driverLicenseNo;
    }

    private void SetDriverLicenseLevel(string? driverLicenseLevel)
    {
        if (!string.IsNullOrWhiteSpace(driverLicenseLevel) && driverLicenseLevel.Length > 5)
        {
            throw new ArgumentException("DriverLicenseLevel cannot exceed 5 characters.", nameof(driverLicenseLevel));
        }
        DriverLicenseLevel = driverLicenseLevel;
    }

    private void SetDriverLicenseEffectDate(DateTime? driverLicenseEffectDate)
    {
        DriverLicenseEffectDate = driverLicenseEffectDate;
    }

    private void SetDriverLicenseExpireDate(DateTime? driverLicenseExpireDate)
    {
        DriverLicenseExpireDate = driverLicenseExpireDate;
    }

    private void SetDriverSex(string? driverSex)
    {
        if (!string.IsNullOrWhiteSpace(driverSex))
        {
            var upperSex = driverSex.ToUpperInvariant();
            if (upperSex != "M" && upperSex != "F")
            {
                throw new ArgumentException("DriverSex must be 'M' or 'F'.", nameof(driverSex));
            }
            if (upperSex.Length > 15)
            {
                throw new ArgumentException("DriverSex cannot exceed 15 characters.", nameof(driverSex));
            }
            DriverSex = upperSex;
        }
        else
        {
            DriverSex = null;
        }
    }

    private void SetCarRegistryNo(string? carRegistryNo)
    {
        if (!string.IsNullOrWhiteSpace(carRegistryNo) && carRegistryNo.Length > 50)
        {
            throw new ArgumentException("CarRegistryNo cannot exceed 50 characters.", nameof(carRegistryNo));
        }
        CarRegistryNo = carRegistryNo;
    }

    private void SetCarRegistryEffectDate(DateTime? carRegistryEffectDate)
    {
        CarRegistryEffectDate = carRegistryEffectDate;
    }

    private void SetCarRegistryExpireDate(DateTime? carRegistryExpireDate)
    {
        CarRegistryExpireDate = carRegistryExpireDate;
    }

    // Update methods
    public virtual void UpdateIncidentObjectId(Guid? incidentObjectId)
    {
        SetIncidentObjectId(incidentObjectId);
    }

    public virtual void UpdateCarPlate(string? carPlate)
    {
        SetCarPlate(carPlate);
    }

    public virtual void UpdateVin(string? vin)
    {
        SetVin(vin);
    }

    public virtual void UpdateEngineNumber(string? engineNumber)
    {
        SetEngineNumber(engineNumber);
    }

    public virtual void UpdatePersonsOnCar(int? personsOnCar)
    {
        SetPersonsOnCar(personsOnCar);
    }

    public virtual void UpdateDriverName(string? driverName)
    {
        SetDriverName(driverName);
    }

    public virtual void UpdateDriverPhone(string? driverPhone)
    {
        SetDriverPhone(driverPhone);
    }

    public virtual void UpdateDriverIdNo(string? driverIdNo)
    {
        SetDriverIdNo(driverIdNo);
    }

    public virtual void UpdateDriverLicenseNo(string? driverLicenseNo)
    {
        SetDriverLicenseNo(driverLicenseNo);
    }

    public virtual void UpdateDriverLicenseLevel(string? driverLicenseLevel)
    {
        SetDriverLicenseLevel(driverLicenseLevel);
    }

    public virtual void UpdateDriverLicenseEffectDate(DateTime? driverLicenseEffectDate)
    {
        SetDriverLicenseEffectDate(driverLicenseEffectDate);
    }

    public virtual void UpdateDriverLicenseExpireDate(DateTime? driverLicenseExpireDate)
    {
        SetDriverLicenseExpireDate(driverLicenseExpireDate);
    }

    public virtual void UpdateDriverSex(string? driverSex)
    {
        SetDriverSex(driverSex);
    }

    public virtual void UpdateCarRegistryNo(string? carRegistryNo)
    {
        SetCarRegistryNo(carRegistryNo);
    }

    public virtual void UpdateCarRegistryEffectDate(DateTime? carRegistryEffectDate)
    {
        SetCarRegistryEffectDate(carRegistryEffectDate);
    }

    public virtual void UpdateCarRegistryExpireDate(DateTime? carRegistryExpireDate)
    {
        SetCarRegistryExpireDate(carRegistryExpireDate);
    }
}
