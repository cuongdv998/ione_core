using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace iOne.Reports
{
    internal class ReportTemplateSqlParameterConfiguration : IEntityTypeConfiguration<ReportTemplateSqlParameter>
    {
        public void Configure(EntityTypeBuilder<ReportTemplateSqlParameter> builder)
        {
            builder.ToTable("report_template_sql_parameter");

            builder.HasKey(x => new { x.SqlId, x.ParameterId });

            builder.Property(x => x.SqlId)
                .HasColumnName("sql_id")
                .IsRequired();

            builder.Property(x => x.ParameterId)
                .HasColumnName("parameter_id")
                .IsRequired();

            builder.HasOne<ReportTemplateSql>()
                .WithMany()
                .HasForeignKey(x => x.SqlId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<ReportTemplateParameter>()
                .WithMany()
                .HasForeignKey(x => x.ParameterId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
