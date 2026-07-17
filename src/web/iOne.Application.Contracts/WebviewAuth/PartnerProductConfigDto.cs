using System;

namespace iOne.WebviewAuth;

public class PartnerProductConfigDto
{
    public Guid InsurerId { get; set; }
    public Guid ProductLobId { get; set; }
    public Guid ChannelId { get; set; }
    public Guid OrganizationTypeId { get; set; }
    public Guid ObjectTypeId { get; set; }
    public Guid PolicyTypeId { get; set; }
    public Guid? SellerId { get; set; }
}
