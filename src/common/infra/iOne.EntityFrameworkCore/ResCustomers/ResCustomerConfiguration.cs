using System;
using iOne.ResCustomers;
using iOne.ResIndustries;
using iOne.ResProvinces;
using iOne.ResWards;
using iOne.ResOrganizationTypes;
using iOne.HrEmployees;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ResCustomers;

public class ResCustomerConfiguration : IEntityTypeConfiguration<ResCustomer>
{
    public void Configure(EntityTypeBuilder<ResCustomer> builder)
    {
        // ✅ Table name: snake_case (PostgreSQL convention)
        builder.ToTable("res_customer", t =>
        {
            t.HasComment("Bảng lưu thông tin khách hàng");
        });

        builder.ConfigureByConvention();

        // ✅ Column names: snake_case (PostgreSQL convention)
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(25);

        builder.Property(x => x.RefCode)
            .HasColumnName("ref_code")
            .HasMaxLength(50);

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(250)
            .IsRequired();

        // Status: Enum to string conversion (lowercase)
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(), // Convert "Active" → "active", "Deactive" → "deactive"
                v => Enum.Parse<ResCustomerStatus>(v, true) // Parse "active" → Active, "deactive" → Deactive
            )
            .HasMaxLength(10)
            .IsRequired()
            .HasComment("Trạng thái:\n- active: hoạt động\n- deactive: không hoạt động");

        // Sex: Enum to string conversion (uppercase: M, F)
        builder.Property(x => x.Sex)
            .HasColumnName("sex")
            .HasConversion<string>(
                v => v.HasValue ? (v.Value == ResCustomerSex.Male ? "M" : "F") : null,
                v => string.IsNullOrEmpty(v) ? null : (v == "M" ? ResCustomerSex.Male : ResCustomerSex.Female)
            )
            .HasMaxLength(15);

        // Foreign Keys
        builder.Property(x => x.IndustryId)
            .HasColumnName("industry_id");

        builder.Property(x => x.ProvinceId)
            .HasColumnName("province_id");

        builder.Property(x => x.WardId)
            .HasColumnName("ward_id");

        builder.Property(x => x.OrganizationTypeId)
            .HasColumnName("organization_type_id");

        builder.Property(x => x.InvoiceProvinceId)
            .HasColumnName("invoice_province_id");

        builder.Property(x => x.InvoiceWardId)
            .HasColumnName("invoice_ward_id");

        builder.Property(x => x.SaleId)
            .HasColumnName("sale_id");

        // Address Info
        builder.Property(x => x.Address)
            .HasColumnName("address")
            .HasMaxLength(250);

        builder.Property(x => x.FullAddress)
            .HasColumnName("full_address")
            .HasMaxLength(500);

        builder.Property(x => x.InvoiceAddress)
            .HasColumnName("invoice_address")
            .HasMaxLength(250);

        builder.Property(x => x.InvoiceFullAddress)
            .HasColumnName("invoice_full_address")
            .HasMaxLength(500);

        // Contact Info
        builder.Property(x => x.Phone)
            .HasColumnName("phone")
            .HasMaxLength(15)
            .IsRequired();

        builder.Property(x => x.Email)
            .HasColumnName("email")
            .HasMaxLength(50);

        builder.Property(x => x.Note)
            .HasColumnName("note")
            .HasMaxLength(500);

        // Tax and Identity Info
        builder.Property(x => x.Tin)
            .HasColumnName("tin")
            .HasMaxLength(50);

        builder.Property(x => x.IdNo)
            .HasColumnName("id_no")
            .HasMaxLength(25);

        builder.Property(x => x.PassportNo)
            .HasColumnName("passport_no")
            .HasMaxLength(25);

        builder.Property(x => x.Dob)
            .HasColumnName("dob")
            .HasColumnType("date");

        // Representative Info
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

        // Authorizer Info
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
            .HasColumnName("authorizer_date")
            .HasColumnType("date");

        builder.Property(x => x.AuthorizerTitle)
            .HasColumnName("authorizer_title")
            .HasMaxLength(50);

        builder.Property(x => x.BusinessNo)
            .HasColumnName("business_no")
            .HasMaxLength(25);

        // ✅ Audit columns: snake_case
        builder.Property(x => x.CreationTime).HasColumnName("creation_time");
        builder.Property(x => x.CreatorId).HasColumnName("creator_id");
        builder.Property(x => x.LastModificationTime).HasColumnName("last_modification_time");
        builder.Property(x => x.LastModifierId).HasColumnName("last_modifier_id");
        builder.Property(x => x.DeletionTime).HasColumnName("deletion_time");
        builder.Property(x => x.DeleterId).HasColumnName("deleter_id");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        builder.Property(x => x.ConcurrencyStamp).HasColumnName("concurrency_stamp");
        // Note: ResCustomer does not implement IMultiTenant, so no TenantId mapping

        // ✅ Foreign Keys với tên snake_case
        builder.HasOne(x => x.Industry)
            .WithMany()
            .HasForeignKey(x => x.IndustryId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_res_customer_industry_id");

        builder.HasOne(x => x.Province)
            .WithMany()
            .HasForeignKey(x => x.ProvinceId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_res_customer_province_id");

        builder.HasOne(x => x.Ward)
            .WithMany()
            .HasForeignKey(x => x.WardId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_res_customer_ward_id");

        builder.HasOne(x => x.OrganizationType)
            .WithMany()
            .HasForeignKey(x => x.OrganizationTypeId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_res_customer_organization_type_id");

        builder.HasOne(x => x.InvoiceProvince)
            .WithMany()
            .HasForeignKey(x => x.InvoiceProvinceId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_res_customer_invoice_province_id");

        builder.HasOne(x => x.InvoiceWard)
            .WithMany()
            .HasForeignKey(x => x.InvoiceWardId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_res_customer_invoice_ward_id");

        builder.HasOne(x => x.Sale)
            .WithMany()
            .HasForeignKey(x => x.SaleId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_res_customer_sale_id");

        // ✅ Index name: snake_case với prefix
        builder.HasIndex(x => x.Code, "ix_res_customer_code")
            .IsUnique();
    }
}

