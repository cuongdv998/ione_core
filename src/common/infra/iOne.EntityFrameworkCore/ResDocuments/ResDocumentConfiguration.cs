using iOne.ResDocuments;
using iOne.ResDocumentTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.ResDocuments;

public class ResDocumentConfiguration : IEntityTypeConfiguration<ResDocument>
{
    public void Configure(EntityTypeBuilder<ResDocument> builder)
    {
        builder.ToTable("res_document", t =>
        {
            t.HasComment("Bảng lưu trữ các tài liệu đính kèm");
        });

        builder.ConfigureByConvention();

        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.GroupCode)
            .HasColumnName("group_code")
            .HasMaxLength(50)
            .HasComment("Phân nhóm (theo định nghĩa trong bảng AdminConfig với code = 'DOCUMENT_GROUP':\n1: Cac tai lieu khac,\n2: Tai lieu hien truong,\n3: Tai lieu toan canh,\n4: Ho so,\n5: Bao gia,\n6: Tai lieu trinh ky,\n7: Hang muc ton that");

        builder.Property(x => x.DocumentTypeId)
            .HasColumnName("doc_type_id")
            .IsRequired();

        builder.Property(x => x.FileSize)
            .HasColumnName("file_size")
            .IsRequired()
            .HasComment("Kích thước file upload (đơn vị byte)");

        builder.Property(x => x.FileName)
            .HasColumnName("file_name")
            .HasMaxLength(250)
            .IsRequired()
            .HasComment("Tên file gốc do người dùng upload lên");

        builder.Property(x => x.StoreFileName)
            .HasColumnName("store_file_name")
            .HasMaxLength(250)
            .IsRequired()
            .HasComment("Tên file lưu trữ (do hệ thống tự sinh)");

        builder.Property(x => x.Url)
            .HasColumnName("url")
            .HasMaxLength(500)
            .HasComment("Đường dẫn lưu file (nếu có)");

        builder.Property(x => x.ThumbnailUrl)
            .HasColumnName("thumbnail_url")
            .HasMaxLength(250)
            .HasComment("Hình ảnh dung lượng thấp, đại diện cho hình ảnh thật");

        builder.Property(x => x.Checksum)
            .HasColumnName("checksum")
            .HasMaxLength(40)
            .HasComment("Chuỗi mã hóa để kiểm tra tính toàn vẹn của file");

        builder.Property(x => x.MimeType)
            .HasColumnName("mime_type")
            .HasMaxLength(200)
            .IsRequired()
            .HasComment("Loại file");

        builder.Property(x => x.BucketName)
            .HasColumnName("bucket_name")
            .HasMaxLength(50)
            .IsRequired()
            .HasComment("Tên Buket chứa file");

        builder.Property(x => x.VersionId)
            .HasColumnName("version_id")
            .HasMaxLength(150)
            .HasComment("Version file nếu có bật tính năng này trên MinIO");

        builder.Property(x => x.ExtraProperties).HasColumnName("extra_properties");

        builder.Property(x => x.CreationTime).HasColumnName("creation_time");
        builder.Property(x => x.CreatorId).HasColumnName("creator_id");
        builder.Property(x => x.LastModificationTime).HasColumnName("last_modification_time");
        builder.Property(x => x.LastModifierId).HasColumnName("last_modifier_id");
        builder.Property(x => x.ConcurrencyStamp).HasColumnName("concurrency_stamp");

        // Foreign key relationship
        builder.HasOne(x => x.DocumentType)
            .WithMany()
            .HasForeignKey(x => x.DocumentTypeId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        // Index on DocumentTypeId for better query performance
        builder.HasIndex(e => e.DocumentTypeId, "ix_res_document_doc_type_id");
    }
}

