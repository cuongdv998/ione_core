namespace iOne.PartnerIntegration.Pti;

/// <summary>
/// <c>insurer_dictionary.business_name</c> values used when building PTI motor requests.
/// Partner-specific codes live in <c>insurer_code</c>/<c>extra_data</c> per row (scoped by <c>insurer_id</c>).
/// </summary>
public static class PtiInsurerDictionaryCategories
{
    /// <summary>Motor vehicle type — aligns with motor combobox (<c>RES_MORTOR_TYPE</c>).</summary>
    public const string MotorVehicleType = "RES_MORTOR_TYPE";

    /// <summary>Motor vehicle condition — Phụ lục Bảng 3.</summary>
    public const string MotorVehicleStatus = "MOTOR_VEHICLE_STATUS";

    /// <summary>Buyer customer category — Phụ lục Bảng 1.</summary>
    public const string MotorCustomerType = "MOTOR_CUSTOMER_TYPE";

    /// <summary>Enterprise org subtype — Phụ lục Bảng 4.</summary>
    public const string MotorOrgTypeDn = "MOTOR_ORG_TYPE_DN";

    /// <summary>Non-enterprise org subtype — Phụ lục Bảng 5.</summary>
    public const string MotorOrgTypePdn = "MOTOR_ORG_TYPE_PDN";

    /// <summary>Invoice recipient — Phụ lục Bảng 6.</summary>
    public const string MotorBillActor = "MOTOR_BILL_ACTOR";

    /// <summary>
    /// Fallback <c>own_code</c> when policy motor risk is missing or has no <c>CarTypeCode</c>.
    /// Seed a dictionary row for this key pointing at the partner default type.
    /// </summary>
    public const string MotorVehicleTypeFallbackOwnCode = "DEFAULT";
}
