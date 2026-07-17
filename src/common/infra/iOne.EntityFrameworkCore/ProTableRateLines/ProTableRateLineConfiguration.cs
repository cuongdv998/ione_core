using System;
using iOne.ProTableRateLines;
using iOne.ProTableRates;
using iOne.ProCoverages;
using iOne.ResChannels;
using iOne.ResPartners;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ProTableRateLines;

public class ProTableRateLineConfiguration : IEntityTypeConfiguration<ProTableRateLine>
{
    public void Configure(EntityTypeBuilder<ProTableRateLine> builder)
    {
        // ✅ Table name: snake_case (PostgreSQL convention)
        builder.ToTable("pro_table_rate_line", t =>
        {
            t.HasComment("Bảng Định nghĩa phí bảo hiểm");
        });

        builder.ConfigureByConvention();

        // ✅ Column names: snake_case (PostgreSQL convention)
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.TableRateId)
            .HasColumnName("table_rate_id")
            .IsRequired();

        builder.Property(x => x.CoverageId)
            .HasColumnName("coverage_id");

        builder.Property(x => x.ChannelId)
            .HasColumnName("channel_id");

        builder.Property(x => x.PartnerId)
            .HasColumnName("partner_id");

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(250);

        builder.Property(x => x.Condition)
            .HasColumnName("condition")
            .HasColumnType("jsonb")  // Changed from "text" to support PostgreSQL jsonb operators
            .IsRequired();

        builder.Property(x => x.MinimumRate)
            .HasColumnName("minimum_rate")
            .HasColumnType("numeric(15,3)");

        builder.Property(x => x.BaseRate)
            .HasColumnName("base_rate")
            .HasColumnType("numeric(15,3)");

        builder.Property(x => x.FlatRate)
            .HasColumnName("flat_rate")
            .HasColumnType("numeric(15,3)");

        builder.Property(x => x.MaxDiscount)
            .HasColumnName("max_discount")
            .HasColumnType("numeric(15,3)");

        builder.Property(x => x.LoadingRate)
            .HasColumnName("loading_rate")
            .HasColumnType("numeric(15,3)");

        builder.Property(x => x.Loading)
            .HasColumnName("loading")
            .HasColumnType("numeric(15,3)");

        builder.Property(x => x.EffectDate)
            .HasColumnName("effect_date")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(x => x.ExpireDate)
            .HasColumnName("expire_date")
            .HasColumnType("date");

        // ✅ ExtraProperties: snake_case (ABP convention)
        builder.Property(x => x.ExtraProperties).HasColumnName("extra_properties");

        // ✅ Audit columns: snake_case (PostgreSQL standard)
        builder.Property(x => x.CreationTime).HasColumnName("creation_time");
        builder.Property(x => x.CreatorId).HasColumnName("creator_id");
        builder.Property(x => x.LastModificationTime).HasColumnName("last_modification_time");
        builder.Property(x => x.LastModifierId).HasColumnName("last_modifier_id");
        builder.Property(x => x.DeletionTime).HasColumnName("deletion_time");
        builder.Property(x => x.DeleterId).HasColumnName("deleter_id");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        builder.Property(x => x.ConcurrencyStamp).HasColumnName("concurrency_stamp");

        // ✅ Primary key
        builder.HasKey(e => e.Id).HasName("pk_pro_table_rate_line");

        // Configure TableRateId foreign key relationship
        builder.HasOne(e => e.TableRate)
            .WithMany(t => t.Lines)
            .HasForeignKey(e => e.TableRateId)
            .HasConstraintName("fk_pro_table_rate_line_pro_table_rate_table_rate_id")
            .OnDelete(DeleteBehavior.Restrict);

        // Configure CoverageId foreign key relationship
        builder.HasOne(e => e.Coverage)
            .WithMany(c => c.TableRateLines)
            .HasForeignKey(e => e.CoverageId)
            .HasConstraintName("fk_pro_table_rate_line_pro_coverage_coverage_id")
            .OnDelete(DeleteBehavior.Restrict);

        // Configure ChannelId foreign key relationship
        builder.HasOne(e => e.Channel)
            .WithMany()
            .HasForeignKey(e => e.ChannelId)
            .HasConstraintName("fk_pro_table_rate_line_res_channel_channel_id")
            .OnDelete(DeleteBehavior.Restrict);

        // Configure PartnerId foreign key relationship
        builder.HasOne(e => e.Partner)
            .WithMany()
            .HasForeignKey(e => e.PartnerId)
            .HasConstraintName("fk_pro_table_rate_line_res_partner_partner_id")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
