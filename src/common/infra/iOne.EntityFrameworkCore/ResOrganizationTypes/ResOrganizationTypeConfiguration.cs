using System;
using iOne.ResOrganizationTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ResOrganizationTypes;

public class ResOrganizationTypeConfiguration : IEntityTypeConfiguration<ResOrganizationType>
{
    public void Configure(EntityTypeBuilder<ResOrganizationType> builder)
    {
        // ✅ Table name: snake_case (PostgreSQL convention)
        builder.ToTable("res_organization_type", t =>
        {
            t.HasComment("Loại tổ chức, định nghĩa các loại như: cá nhân, doanh nghiệp, tổ chức khác ...");
        });

        builder.ConfigureByConvention();

        // ✅ Column names: snake_case (PostgreSQL convention)
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(25);
        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(250);
        
        // ✅ Status: Enum to string conversion (lowercase)
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<ResOrganizationTypeStatus>(v, true)
            )
            .HasMaxLength(10)
            .HasComment("Trạng thái:\n- active: Hoạt động\n- deactive: Không hoạt động");
        
        // ✅ Type: Enum to string conversion (uppercase: TC, CN)
        builder.Property(x => x.Type)
            .HasColumnName("type")
            .HasConversion<string>(
                v => v.ToString(), // TC hoặc CN
                v => Enum.Parse<OrganizationTypeType>(v, true)
            )
            .HasMaxLength(15)
            .IsRequired()
            .HasDefaultValue(OrganizationTypeType.TC)
            .HasComment("Loại tổ chức:\n- TC: Tổ chức\n- CN: Cá nhân");
        
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
}

