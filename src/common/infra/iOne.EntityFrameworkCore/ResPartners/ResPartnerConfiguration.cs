using System;
using iOne.ResPartners;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ResPartners;

public class ResPartnerConfiguration : IEntityTypeConfiguration<ResPartner>
{
    public void Configure(EntityTypeBuilder<ResPartner> builder)
    {
        builder.ToTable("res_partner", t =>
        {
            t.HasComment("Bảng thông tin đối tác");
        });

        builder.ConfigureByConvention();

        // ✅ Column names: snake_case
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.ChannelId)
            .HasColumnName("channel_id");

        builder.Property(x => x.PartnerTypeId)
            .HasColumnName("partner_type_id")
            .IsRequired();

        builder.Property(x => x.PartnerRole)
            .HasColumnName("partner_role")
            .HasMaxLength(15);

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(25);

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x => x.OrganizationTypeId)
            .HasColumnName("organization_type_id");

        builder.Property(x => x.ProvinceId)
            .HasColumnName("province_id")
            .IsRequired();

        builder.Property(x => x.WardId)
            .HasColumnName("ward_id")
            .IsRequired();

        builder.Property(x => x.Address)
            .HasColumnName("address")
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x => x.FullAddress)
            .HasColumnName("full_address")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Email)
            .HasColumnName("email")
            .HasMaxLength(50);

        builder.Property(x => x.Phone)
            .HasColumnName("phone")
            .HasMaxLength(15)
            .IsRequired();

        builder.Property(x => x.Note)
            .HasColumnName("note")
            .HasMaxLength(500);

        // ✅ Status: Enum to string conversion (lowercase)
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<ResPartnerStatus>(v, true)
            )
            .HasMaxLength(10)
            .IsRequired()
            .HasComment("Trạng thái:\n- active: hoạt động\n- deactive: không hoạt động");

        // Invoice Address
        builder.Property(x => x.InvoiceProvinceId)
            .HasColumnName("invoice_province_id");

        builder.Property(x => x.InvoiceWardId)
            .HasColumnName("invoice_ward_id");

        builder.Property(x => x.InvoiceAddress)
            .HasColumnName("invoice_address")
            .HasMaxLength(250);

        builder.Property(x => x.InvoiceFullAddress)
            .HasColumnName("invoice_full_address")
            .HasMaxLength(500);

        // CN (Cá nhân) fields
        builder.Property(x => x.IdNo)
            .HasColumnName("id_no")
            .HasMaxLength(25);

        // TC (Tổ chức) fields
        builder.Property(x => x.Tin)
            .HasColumnName("tin")
            .HasMaxLength(50);

        builder.Property(x => x.RepName)
            .HasColumnName("rep_name")
            .HasMaxLength(250);

        builder.Property(x => x.RepEmail)
            .HasColumnName("rep_email")
            .HasMaxLength(50);

        builder.Property(x => x.RepPhone)
            .HasColumnName("rep_phone")
            .HasMaxLength(15);

        builder.Property(x => x.RepIdNo)
            .HasColumnName("rep_id_no")
            .HasMaxLength(25);

        builder.Property(x => x.RepTitle)
            .HasColumnName("rep_title")
            .HasMaxLength(250);

        builder.Property(x => x.Authorizer)
            .HasColumnName("authorizer")
            .HasMaxLength(50);

        builder.Property(x => x.AuthorizerPhone)
            .HasColumnName("authorizer_phone")
            .HasMaxLength(15);

        builder.Property(x => x.AuthorizerEmail)
            .HasColumnName("authorizer_email")
            .HasMaxLength(50);

        builder.Property(x => x.AuthorizerNo)
            .HasColumnName("authorizer_no")
            .HasMaxLength(25);

        builder.Property(x => x.AuthorizerDate)
            .HasColumnName("authorizer_date");

        builder.Property(x => x.AuthorizerTitle)
            .HasColumnName("authorizer_title")
            .HasMaxLength(50);

        builder.Property(x => x.BusinessNo)
            .HasColumnName("business_no")
            .HasMaxLength(25);

        // ✅ Audit columns: snake_case
        builder.Property(x => x.ExtraProperties).HasColumnName("extra_properties");
        builder.Property(x => x.CreationTime).HasColumnName("creation_time");
        builder.Property(x => x.CreatorId).HasColumnName("creator_id");
        builder.Property(x => x.LastModificationTime).HasColumnName("last_modification_time");
        builder.Property(x => x.LastModifierId).HasColumnName("last_modifier_id");
        builder.Property(x => x.DeletionTime).HasColumnName("deletion_time");
        builder.Property(x => x.DeleterId).HasColumnName("deleter_id");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        builder.Property(x => x.ConcurrencyStamp).HasColumnName("concurrency_stamp");

        // ✅ Foreign Keys với OnDelete(DeleteBehavior.Restrict)
        builder.HasOne(x => x.Channel)
            .WithMany()
            .HasForeignKey(x => x.ChannelId)
            .HasConstraintName("fk_res_partner_res_channel_channel_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PartnerType)
            .WithMany()
            .HasForeignKey(x => x.PartnerTypeId)
            .HasConstraintName("fk_res_partner_res_partner_type_partner_type_id")
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasOne(x => x.OrganizationType)
            .WithMany()
            .HasForeignKey(x => x.OrganizationTypeId)
            .HasConstraintName("fk_res_partner_res_organization_type_organization_type_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Province)
            .WithMany()
            .HasForeignKey(x => x.ProvinceId)
            .HasConstraintName("fk_res_partner_res_province_province_id")
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasOne(x => x.Ward)
            .WithMany()
            .HasForeignKey(x => x.WardId)
            .HasConstraintName("fk_res_partner_res_ward_ward_id")
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasOne(x => x.InvoiceProvince)
            .WithMany()
            .HasForeignKey(x => x.InvoiceProvinceId)
            .HasConstraintName("fk_res_partner_res_province_invoice_province_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.InvoiceWard)
            .WithMany()
            .HasForeignKey(x => x.InvoiceWardId)
            .HasConstraintName("fk_res_partner_res_ward_invoice_ward_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.ChannelId, "ix_res_partner_channel_id");
        builder.HasIndex(e => e.PartnerTypeId, "ix_res_partner_partner_type_id");
        builder.HasIndex(e => e.OrganizationTypeId, "ix_res_partner_organization_type_id");
        builder.HasIndex(e => e.ProvinceId, "ix_res_partner_province_id");
        builder.HasIndex(e => e.WardId, "ix_res_partner_ward_id");
        builder.HasIndex(e => e.InvoiceProvinceId, "ix_res_partner_invoice_province_id");
        builder.HasIndex(e => e.InvoiceWardId, "ix_res_partner_invoice_ward_id");
    }
}

