using System.ComponentModel.DataAnnotations;
using iOne.ResSequences;

namespace iOne.Master.ResSequences;

public class UpdateResSequenceDto
{
    // ⚠️ QUAN TRỌNG: Không có Code property - Code không được phép sửa

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
    public int NumberIncrement { get; set; }

    [Required(ErrorMessage = "ResSequence:UseDateRangeRequired")]
    [Display(Name = "ResSequence:UseDateRange")]
    public ResSequenceUseDateRange UseDateRange { get; set; }

    [Display(Name = "ResSequence:DateRangeType")]
    public ResSequenceDateRangeType? DateRangeType { get; set; }

    [Required(ErrorMessage = "ResSequence:StatusRequired")]
    [Display(Name = "ResSequence:Status")]
    public ResSequenceStatus Status { get; set; }
}

