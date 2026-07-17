using System;
using System.ComponentModel.DataAnnotations;
using iOne.ProAttributes;
using Volo.Abp.Application.Dtos;

namespace iOne.Product.ProProducts;

public class ProProductAttributeDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ProProductAttribute:ProductId")]
    public Guid ProductId { get; set; }

    [Display(Name = "ProProductAttribute:AttributeId")]
    public Guid AttributeId { get; set; }

    [Display(Name = "ProProductAttribute:IsRequired")]
    public string IsRequired { get; set; } = "N";

    [Display(Name = "ProProductAttribute:Status")]
    public string? Status { get; set; }

    // Joined attribute info from pro_attribute (flattened; all optional for FE/proxy compatibility)
    public string? AttributeCode { get; set; }
    public string? AttributeName { get; set; }
    public ProAttributeStatus? AttributeStatus { get; set; }
    public ProAttributeSpec? AttributeSpec { get; set; }
    public string? AttributeDescription { get; set; }
    public string? AttributeDataPath { get; set; }
    public ProAttributeDataType? AttributeDataType { get; set; }
    public string? AttributeComputeScript { get; set; }
    public string? AttributeClearDataScript { get; set; }
}
