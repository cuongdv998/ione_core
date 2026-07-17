using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.Reports
{
    public class ReportTemplateParameterConfiguration : IEntityTypeConfiguration<ReportTemplateParameter>
    {
        public void Configure(EntityTypeBuilder<ReportTemplateParameter> builder)
        {
            builder.ToTable("report_template_parameter");

            builder.ConfigureByConvention();

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id");

            builder.Property(x => x.ReportTemplateId)
                .HasColumnName("report_template_id")
                .IsRequired();

            builder.Property(x => x.Code)
                .HasColumnName("code")
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Name)
                .HasColumnName("name")
                .IsRequired()
                .HasMaxLength(250);

            builder.Property(x => x.Status)
                .HasColumnName("status")
                .IsRequired()
                .HasMaxLength(15)
                .HasDefaultValue("active");

            builder.Property(x => x.Description)
                .HasColumnName("description")
                .HasMaxLength(250);

            builder.Property(x => x.DataType)
                .HasColumnName("data_type")
                .IsRequired()
                .HasMaxLength(10);

            // ===== Audited (snake_case) =====
            builder.Property(x => x.CreationTime).HasColumnName("creation_time");
            builder.Property(x => x.CreatorId).HasColumnName("creator_id");
            builder.Property(x => x.LastModificationTime).HasColumnName("last_modification_time");
            builder.Property(x => x.LastModifierId).HasColumnName("last_modifier_id");
            builder.Property(x => x.DeletionTime).HasColumnName("deletion_time");
            builder.Property(x => x.DeleterId).HasColumnName("deleter_id");
            builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
            builder.Property(x => x.ConcurrencyStamp).HasColumnName("concurrency_stamp");
            builder.Property(x => x.ExtraProperties).HasColumnName("extra_properties");

            builder.HasIndex(x => new { x.ReportTemplateId, x.Code })
                .IsUnique();

            // ===== FK =====
            builder.HasOne(x => x.ReportTemplate)
                .WithMany()
                .HasForeignKey(x => x.ReportTemplateId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
