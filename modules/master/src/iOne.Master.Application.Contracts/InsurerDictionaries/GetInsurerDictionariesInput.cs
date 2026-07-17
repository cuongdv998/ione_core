using System;
using iOne.InsurerDictionaries;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.InsurerDictionaries;

public class GetInsurerDictionariesInput : PagedAndSortedResultRequestDto
{
    public string? BusinessName { get; set; }

    public Guid? InsurerId { get; set; }

    public string? OwnCode { get; set; }

    public string? InsurerCode { get; set; }

    public InsurerDictionaryStatus? Status { get; set; }
}
