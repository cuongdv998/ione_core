using System;
using System.ComponentModel.DataAnnotations;
using iOne.ResSequences;
using Volo.Abp.Application.Dtos;

namespace iOne.Master.ResSequences;

public class ResSequenceDto : FullAuditedEntityDto<Guid>
{
    [Display(Name = "ResSequence:Code")]
    public string Code { get; set; } = null!;

    [Display(Name = "ResSequence:Name")]
    public string Name { get; set; } = null!;

    [Display(Name = "ResSequence:Prefix")]
    public string? Prefix { get; set; }

    [Display(Name = "ResSequence:Suffix")]
    public string? Suffix { get; set; }

    [Display(Name = "ResSequence:Type")]
    public ResSequenceType Type { get; set; }

    [Display(Name = "ResSequence:Padding")]
    public int? Padding { get; set; }

    [Display(Name = "ResSequence:NumberNext")]
    public long NumberNext { get; set; }

    [Display(Name = "ResSequence:NumberIncrement")]
    public int NumberIncrement { get; set; }

    [Display(Name = "ResSequence:UseDateRange")]
    public ResSequenceUseDateRange UseDateRange { get; set; }

    [Display(Name = "ResSequence:DateRangeType")]
    public ResSequenceDateRangeType? DateRangeType { get; set; }

    [Display(Name = "ResSequence:Status")]
    public ResSequenceStatus Status { get; set; }
}

