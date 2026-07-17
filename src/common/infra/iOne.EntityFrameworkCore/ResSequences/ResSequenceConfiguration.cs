using System;
using iOne.ResSequences;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ResSequences;

public class ResSequenceConfiguration : IEntityTypeConfiguration<ResSequence>
{
    public void Configure(EntityTypeBuilder<ResSequence> builder)
    {
        // ✅ Table name: snake_case (PostgreSQL convention)
        builder.ToTable("res_sequence", t =>
        {
            t.HasComment("Bảng cấu hình các mã tự sinh của hệ thống");
        });

        builder.ConfigureByConvention();

        // ✅ Column names: snake_case (BẮT BUỘC override)
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(25)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x => x.Prefix)
            .HasColumnName("prefix")
            .HasMaxLength(150);

        builder.Property(x => x.Suffix)
            .HasColumnName("suffix")
            .HasMaxLength(150);

        // ✅ Type: Enum to string conversion (lowercase với underscore)
        builder.Property(x => x.Type)
            .HasColumnName("type")
            .HasConversion<string>(
                v => v == ResSequenceType.Normal ? "normal" : "no_gap", // Normal → "normal", NoGap → "no_gap"
                v => v == "normal" ? ResSequenceType.Normal : ResSequenceType.NoGap // "normal" → Normal, "no_gap" → NoGap
            )
            .HasMaxLength(10)
            .IsRequired()
            .HasComment("Phân loại:\n- normal: thông thường (có thể nhảy cóc mà ko cần quan tâm thứ tự cấp phát số tăng dần)\n- no_gap: cần lock transaction để cấp phát số theo thứ tự");

        builder.Property(x => x.Padding)
            .HasColumnName("padding")
            .HasComment("Số ký tự tối đa của số sinh tự động, nếu số nhỏ hơn số này thì sẽ thêm số 0 ở đầu để đủ số ký tự");

        builder.Property(x => x.NumberNext)
            .HasColumnName("number_next")
            .IsRequired()
            .HasComment("Số tiếp theo sẽ được sinh ra");

        builder.Property(x => x.NumberIncrement)
            .HasColumnName("number_increment")
            .IsRequired()
            .HasComment("Số bước nhảy, thường là 1");

        // ✅ UseDateRange: Enum to string conversion ("Y"/"N")
        builder.Property(x => x.UseDateRange)
            .HasColumnName("use_date_range")
            .HasConversion<string>(
                v => v == ResSequenceUseDateRange.Yes ? "Y" : "N",
                v => v == "Y" ? ResSequenceUseDateRange.Yes : ResSequenceUseDateRange.No
            )
            .HasMaxLength(10)
            .IsRequired()
            .HasComment("Số sẽ được reset sau một khoảng thời gian:\n- Y: Có\n- N: Không (mặc định)");

        // ✅ DateRangeType: Enum to string conversion (lowercase, lưu ý "quater" không phải "quarter")
        builder.Property(x => x.DateRangeType)
            .HasColumnName("date_range_type")
            .HasConversion<string>(
                v => ConvertDateRangeTypeToString(v),
                v => ConvertStringToDateRangeType(v)
            )
            .HasMaxLength(10)
            .HasComment("Loại thời gian mà số tự sinh sẽ được reset:\n- week: sang tuần mới sẽ reset\n- month: sang tháng mới sẽ reset\n- quater: sang quý mới sẽ reset\n- half: sang nửa năm tiếp theo sẽ reset\n- year: sang năm mới sẽ reset");

        // ✅ Status: Enum to string conversion (lowercase)
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(), // "Active" → "active", "Deactive" → "deactive"
                v => Enum.Parse<ResSequenceStatus>(v, true) // "active" → Active, "deactive" → Deactive
            )
            .HasMaxLength(10)
            .IsRequired()
            .HasComment("Trạng thái:\n- active: Hoạt động\n- deactive: Không hoạt động");

        // ✅ ExtraProperties: snake_case
        builder.Property(x => x.ExtraProperties).HasColumnName("extra_properties");

        // ✅ Audit columns: snake_case
        builder.Property(x => x.CreationTime).HasColumnName("creation_time");
        builder.Property(x => x.CreatorId).HasColumnName("creator_id");
        builder.Property(x => x.LastModificationTime).HasColumnName("last_modification_time");
        builder.Property(x => x.LastModifierId).HasColumnName("last_modifier_id");
        builder.Property(x => x.DeletionTime).HasColumnName("deletion_time");
        builder.Property(x => x.DeleterId).HasColumnName("deleter_id");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        builder.Property(x => x.ConcurrencyStamp).HasColumnName("concurrency_stamp");
    }

    private static string? ConvertDateRangeTypeToString(ResSequenceDateRangeType? value)
    {
        if (!value.HasValue)
            return null;

        return value.Value switch
        {
            ResSequenceDateRangeType.Week => "week",
            ResSequenceDateRangeType.Month => "month",
            ResSequenceDateRangeType.Quarter => "quater", // Lưu ý: "quater" không phải "quarter" theo DLL
            ResSequenceDateRangeType.Half => "half",
            ResSequenceDateRangeType.Year => "year",
            _ => null
        };
    }

    private static ResSequenceDateRangeType? ConvertStringToDateRangeType(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return null;

        return value switch
        {
            "week" => ResSequenceDateRangeType.Week,
            "month" => ResSequenceDateRangeType.Month,
            "quater" => ResSequenceDateRangeType.Quarter, // Lưu ý: "quater" không phải "quarter" theo DLL
            "half" => ResSequenceDateRangeType.Half,
            "year" => ResSequenceDateRangeType.Year,
            _ => null
        };
    }
}

