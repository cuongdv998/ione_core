using System.ComponentModel.DataAnnotations;
using iOne.ResSequences;

namespace iOne.Master.ResSequences;

public class CreateResSequenceDto
{
    [Required(ErrorMessage = "ResSequence:CodeRequired")]
    [StringLength(25, ErrorMessage = "ResSequence:CodeMaxLength")]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "ResSequence:CodeInvalid")]
    [Display(Name = "ResSequence:Code")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "ResSequence:NameRequired")]
    [StringLength(250, ErrorMessage = "ResSequence:NameMaxLength")]
    [Display(Name = "ResSequence:Name")]
    public string Name { get; set; } = null!;

    [StringLength(150, ErrorMessage = "ResSequence:PrefixMaxLength")]
    [Display(Name = "ResSequence:Prefix")]
    public string? Prefix { get; set; }

    [StringLength(150, ErrorMessage = "ResSequence:SuffixMaxLength")]
    [Display(Name = "ResSequence:Suffix")]
    public string? Suffix { get; set; }

    [Required(ErrorMessage = "ResSequence:TypeRequired")]
    [Display(Name = "ResSequence:Type")]
    public ResSequenceType Type { get; set; }

    [Range(0, 99, ErrorMessage = "ResSequence:PaddingRange")]
    [Display(Name = "ResSequence:Padding")]
    public int? Padding { get; set; }

    [Required(ErrorMessage = "ResSequence:NumberNextRequired")]
    [Range(0, 9999999999, ErrorMessage = "ResSequence:NumberNextRange")]
    [Display(Name = "ResSequence:NumberNext")]
    public long NumberNext { get; set; }

    [Required(ErrorMessage = "ResSequence:NumberIncrementRequired")]
    [Range(1, 9, ErrorMessage = "ResSequence:NumberIncrementRange")]
    [Display(Name = "ResSequence:NumberIncrement")]
    public int NumberIncrement { get; set; } = 1;

    [Required(ErrorMessage = "ResSequence:UseDateRangeRequired")]
    [Display(Name = "ResSequence:UseDateRange")]
    public ResSequenceUseDateRange UseDateRange { get; set; } = ResSequenceUseDateRange.No;

    [Display(Name = "ResSequence:DateRangeType")]
    public ResSequenceDateRangeType? DateRangeType { get; set; }

    [Required(ErrorMessage = "ResSequence:StatusRequired")]
    [Display(Name = "ResSequence:Status")]
    public ResSequenceStatus Status { get; set; } = ResSequenceStatus.Active;
}

