using System.Collections.Generic;

namespace iOne.Policy.Policies;

/// <summary>
/// Result of get-attribute-required-spec API.
/// For each product attribute: parameter name (from DataPath), display name, and whether it is required (from ProProductAttribute.IsRequired).
/// </summary>
public class GetAttributeRequiredSpecResultDto
{
    public List<AttributeRequiredSpecItemDto> Items { get; set; } = new();
}

public class AttributeRequiredSpecItemDto
{
    /// <summary>
    /// Attribute code (from product attribute definition).
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Parameter name extracted from DataPath (last segment, e.g. "carPlate" from "riskObject.riskObjectMotor.carPlate").
    /// Used to bind form fields.
    /// </summary>
    public string ParameterName { get; set; } = string.Empty;

    /// <summary>
    /// Display name (from ProAttribute.Name or input attribute name).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Whether this attribute is required for the product (from ProProductAttribute.IsRequired: "Y" = true).
    /// </summary>
    public bool IsRequired { get; set; }
}
