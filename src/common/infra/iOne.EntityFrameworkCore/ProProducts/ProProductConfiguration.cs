using System;
using iOne.ProProducts;
using iOne.ProProductTypes;
using iOne.ResPartners;
using iOne.ProTableRates;
using iOne.ProLineOfBusinesses;
using iOne.ProProductCategorys;
using iOne.ResCurrencies;
using iOne.ProProductPlanDefinitions;
using iOne.ResDocuments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ProProducts;

public class ProProductConfiguration : IEntityTypeConfiguration<ProProduct>
{
    public void Configure(EntityTypeBuilder<ProProduct> builder)
    {
        // ✅ Table name: snake_case (PostgreSQL convention)
        builder.ToTable("pro_product", t =>
        {
            t.HasComment("Bảng định nghĩa sản phẩm");
        });

        builder.ConfigureByConvention();

        // ✅ Column names: snake_case (PostgreSQL convention)
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.ProductTypeId)
            .HasColumnName("product_type_id");

        builder.Property(x => x.PartnerId)
            .HasColumnName("partner_id");

        builder.Property(x => x.TableRateId)
            .HasColumnName("table_rate_id");

        builder.Property(x => x.RootProductId)
            .HasColumnName("root_product_id");

        builder.Property(x => x.IsRootProduct)
            .HasColumnName("is_root_product")
            .HasMaxLength(1)
            .IsRequired()
            .HasDefaultValue("Y");

        builder.Property(x => x.LobId)
            .HasColumnName("lob_id")
            .IsRequired();

        builder.Property(x => x.ProductCategoryId)
            .HasColumnName("product_category_id");

        builder.Property(x => x.CurrencyId)
            .HasColumnName("currency_id");

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.InsurerProductCode)
            .HasColumnName("insurer_product_code")
            .HasMaxLength(50);

        builder.Property(x => x.ShortName)
            .HasColumnName("short_name")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(x => x.InternalNote)
            .HasColumnName("internal_note")
            .HasMaxLength(250);

        builder.Property(x => x.RateType)
            .HasColumnName("rate_type")
            .HasMaxLength(15)
            .IsRequired()
            .HasDefaultValue("table_rate");

        // ✅ Status: Enum to string conversion (lowercase)
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(), // Convert "Active" → "active", "Deactive" → "deactive"
                v => Enum.Parse<ProProductStatus>(v, true) // Parse "active" → Active, "deactive" → Deactive
            )
            .HasMaxLength(15)
            .IsRequired()
            .HasComment("Trạng thái: active (hoạt động), deactive (không hoạt động), draft (nháp)");

        builder.Property(x => x.SeqNumber)
            .HasColumnName("seq_number")
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(x => x.EffectDate)
            .HasColumnName("effect_date")
            .IsRequired();

        builder.Property(x => x.ExpireDate)
            .HasColumnName("expire_date");

        builder.Property(x => x.PlanDefinitionId)
            .HasColumnName("plan_definition_id");

        builder.Property(x => x.IsPlan)
            .HasColumnName("is_plan")
            .HasMaxLength(1);

        builder.Property(x => x.ImageDocumentId)
            .HasColumnName("image_document_id");

        builder.Property(x => x.CertificateTemplateDocumentId)
            .HasColumnName("certificate_template_document_id");

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

        // Configure relationships
        // Many-to-one: ProProduct -> ProProductType
        builder.HasOne(e => e.ProductType)
            .WithMany(e => e.Products)
            .HasForeignKey(e => e.ProductTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Many-to-one: ProProduct -> ResPartner
        builder.HasOne(e => e.Partner)
            .WithMany(e => e.Products)
            .HasForeignKey(e => e.PartnerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Many-to-one: ProProduct -> ProLineOfBusiness
        builder.HasOne(e => e.Lob)
            .WithMany(e => e.Products)
            .HasForeignKey(e => e.LobId)
            .OnDelete(DeleteBehavior.Restrict);

        // Many-to-one: ProProduct -> ProProductCategory
        builder.HasOne(e => e.ProductCategory)
            .WithMany(e => e.Products)
            .HasForeignKey(e => e.ProductCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Many-to-one: ProProduct -> ResCurrency
        builder.HasOne(e => e.Currency)
            .WithMany()
            .HasForeignKey(e => e.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Self-joining: ProProduct -> ProProduct (RootProduct)
        builder.HasOne(e => e.RootProduct)
            .WithMany(e => e.ChildProducts)
            .HasForeignKey(e => e.RootProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Many-to-one: ProProduct -> ResDocument (Image)
        builder.HasOne(e => e.ImageDocument)
            .WithMany()
            .HasForeignKey(e => e.ImageDocumentId)
            .OnDelete(DeleteBehavior.SetNull);

        // Many-to-one: ProProduct -> ResDocument (certificate template)
        builder.HasOne(e => e.CertificateTemplateDocument)
            .WithMany()
            .HasForeignKey(e => e.CertificateTemplateDocumentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
