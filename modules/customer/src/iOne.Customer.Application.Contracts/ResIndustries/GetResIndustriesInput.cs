using System;
using Volo.Abp.Application.Dtos;
using iOne.ResIndustries;

namespace iOne.Customer.ResIndustries;

public class GetResIndustriesInput : PagedAndSortedResultRequestDto
{
    public string? Keyword { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public ResIndustryStatus? Status { get; set; }
}


