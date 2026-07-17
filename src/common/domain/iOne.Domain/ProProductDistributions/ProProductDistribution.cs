using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using iOne.ProProducts;
using iOne.ResChannels;
using iOne.ResAppChannels;
using iOne.HrEmployeeRoles;

namespace iOne.ProProductDistributions;

[Table("PROPRODUCTDISTRIBUTION")]
public class ProProductDistribution : FullAuditedAggregateRoot<Guid>
{
    [Required]
    public virtual Guid ProductId { get; private set; }

    public virtual Guid? ChannelId { get; private set; }

    public virtual Guid? AppChannelId { get; private set; }

    public virtual Guid? EmployeeRoleId { get; private set; }

    [Required]
    public virtual ProProductDistributionStatus Status { get; private set; }

    // Navigation property for many-to-one relationship with ProProduct
    public virtual ProProduct? Product { get; set; }

    // Navigation property for many-to-one relationship with ResChannel
    public virtual ResChannel? Channel { get; set; }

    // Navigation property for many-to-one relationship with ResAppChannel
    public virtual ResAppChannel? AppChannel { get; set; }

    // Navigation property for many-to-one relationship with HrEmployeeRole
    public virtual HrEmployeeRole? EmployeeRole { get; set; }

    protected ProProductDistribution()
    {
        // For ORM
    }

    public ProProductDistribution(
        Guid id,
        Guid productId,
        ProProductDistributionStatus status,
        Guid? channelId = null,
        Guid? appChannelId = null,
        Guid? employeeRoleId = null)
        : base(id)
    {
        SetProductId(productId);
        SetChannelId(channelId);
        SetAppChannelId(appChannelId);
        SetEmployeeRoleId(employeeRoleId);
        SetStatus(status);
    }

    private void SetProductId(Guid productId)
    {
        if (productId == Guid.Empty)
        {
            throw new ArgumentException("ProductId cannot be empty.", nameof(productId));
        }

        ProductId = productId;
    }

    private void SetChannelId(Guid? channelId)
    {
        ChannelId = channelId;
    }

    private void SetAppChannelId(Guid? appChannelId)
    {
        AppChannelId = appChannelId;
    }

    private void SetEmployeeRoleId(Guid? employeeRoleId)
    {
        EmployeeRoleId = employeeRoleId;
    }

    private void SetStatus(ProProductDistributionStatus status)
    {
        Status = status;
    }

    // Update methods
    public virtual void UpdateProductId(Guid productId)
    {
        SetProductId(productId);
    }

    public virtual void UpdateChannelId(Guid? channelId)
    {
        SetChannelId(channelId);
    }

    public virtual void UpdateAppChannelId(Guid? appChannelId)
    {
        SetAppChannelId(appChannelId);
    }

    public virtual void UpdateEmployeeRoleId(Guid? employeeRoleId)
    {
        SetEmployeeRoleId(employeeRoleId);
    }

    public virtual void UpdateStatus(ProProductDistributionStatus status)
    {
        SetStatus(status);
    }
}
