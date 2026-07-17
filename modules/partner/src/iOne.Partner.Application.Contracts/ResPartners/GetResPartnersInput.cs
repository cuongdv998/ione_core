using System;
using iOne.ResPartners;
using Volo.Abp.Application.Dtos;

namespace iOne.Partner.ResPartners;

public class GetResPartnersInput : PagedAndSortedResultRequestDto
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public Guid? PartnerTypeId { get; set; }
    public string? PartnerTypeCode { get; set; }
    public Guid? OrganizationTypeId { get; set; }
    public Guid? ProvinceId { get; set; }
    public Guid? WardId { get; set; }
    public ResPartnerStatus? Status { get; set; }
    public string? PartnerRole { get; set; }
}

