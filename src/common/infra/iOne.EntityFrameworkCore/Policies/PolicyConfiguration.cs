using System;
using iOne.Policies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace iOne.EntityFrameworkCore.Policies;

public class PolicyConfiguration : IEntityTypeConfiguration<Policy>
{
    public void Configure(EntityTypeBuilder<Policy> builder)
    {
        builder.ToTable("policy", t =>
        {
            t.HasComment("Bảng lưu thông tin đơn bảo hiểm");
        });

        builder.ConfigureByConvention();

        builder.Property(x => x.Id).HasColumnName("id");

        // Basic Info
        builder.Property(x => x.ContractId)
            .HasColumnName("contract_id")
            .HasComment("Hợp đồng bảo hiểm");
        builder.Property(x => x.LobId)
            .HasColumnName("lob_id")
            .IsRequired()
            .HasComment("Loại hình bảo hiểm");
        builder.Property(x => x.PolicyNo)
            .HasColumnName("policy_no")
            .HasMaxLength(50)
            .IsRequired()
            .HasComment("Số đơn bảo hiểm");
        builder.Property(x => x.LastVersionId)
            .HasColumnName("last_version_id")
            .IsRequired()
            .HasComment("Phiên bản hiện tại của Policy");
        builder.Property(x => x.SellType)
            .HasColumnName("sell_type")
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<PolicySellType>(v, true)
            )
            .HasMaxLength(15)
            .IsRequired()
            .HasComment("Loại khai thác: agency (đại lý), direct (trực tiếp), indirect (gián tiếp)");
        builder.Property(x => x.InsurerPolicyNo)
            .HasColumnName("insurer_policy_no")
            .HasMaxLength(50)
            .HasComment("Mã policy tương ứng của bảo hiểm gốc");
        builder.Property(x => x.PolicyTypeId)
            .HasColumnName("policy_type_id")
            .IsRequired()
            .HasComment("Hình thức cấp đơn");
        builder.Property(x => x.PartnerId)
            .HasColumnName("partner_id")
            .IsRequired()
            .HasComment("Đơn vị/Đối tác cấp đơn");
        builder.Property(x => x.SellerId)
            .HasColumnName("seller_id")
            .IsRequired()
            .HasComment("Nhân viên khai thác");
        builder.Property(x => x.ImplementerId)
            .HasColumnName("implementer_id")
            .IsRequired()
            .HasComment("Nhân viên cấp đơn");
        builder.Property(x => x.CurrencyId)
            .HasColumnName("currency_id")
            .IsRequired()
            .HasComment("Loại tiền tệ");
        builder.Property(x => x.ExchangeRate)
            .HasColumnName("exchange_rate")
            .HasColumnType("NUMERIC(15,3)")
            .IsRequired()
            .HasDefaultValue(1m)
            .HasComment("Tỷ giá tại thời điểm bán");
        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>(
                v => v.ToString().ToLowerInvariant(),
                v => Enum.Parse<PolicyStatus>(v, true)
            )
            .HasMaxLength(15)
            .IsRequired()
            .HasComment("Trạng thái: quotation (báo giá), draft (nháp), active (đang hiệu lực), expired (hết hạn), terminated (đã chấm dứt), cancelled (đã hủy)");

        // Dates
        builder.Property(x => x.IssueDate)
            .HasColumnName("issue_date")
            .HasComment("Ngày cấp đơn");
        builder.Property(x => x.ApprovalStatus)
            .HasColumnName("approval_status")
            .HasMaxLength(15)
            .HasComment("Trạng thái duyệt: pending_approval (chờ duyệt), approved (đã duyệt), rejected (từ chối duyệt)");
        builder.Property(x => x.CancellationDate)
            .HasColumnName("cancellation_date")
            .HasComment("Ngày hủy đơn, bắt buộc có nếu trạng thái đơn là cancelled");
        builder.Property(x => x.TerminationDate)
            .HasColumnName("termination_date")
            .HasComment("Ngày chấm dứt đơn, bắt buộc có nếu trạng thái đơn là terminated");
        builder.Property(x => x.CancellationReasonId)
            .HasColumnName("cancellation_reason_id")
            .HasComment("Lý do hủy đơn (tham chiếu bảng ResReason)");
        builder.Property(x => x.TerminationReasonId)
            .HasColumnName("termination_reason_id")
            .HasComment("Lý do chấm dứt đơn (tham chiếu bảng ResReason)");
        builder.Property(x => x.OrgEffectDate)
            .HasColumnName("org_effect_date")
            .IsRequired()
            .HasComment("Ngày hiệu lực gốc của đơn bảo hiểm");
        builder.Property(x => x.OrgExpireDate)
            .HasColumnName("org_expire_date")
            .IsRequired()
            .HasComment("Ngày hết hạn gốc của đơn bảo hiểm");

        // Flags
        builder.Property(x => x.IsRenewal)
            .HasColumnName("is_renewal")
            .HasMaxLength(1)
            .IsRequired()
            .HasDefaultValue("N")
            .HasComment("Đánh dấu đơn có phải tái tục hay không: Y (có), N (không). Mặc định là N.");
        builder.Property(x => x.IsGift)
            .HasColumnName("is_gift")
            .HasMaxLength(1)
            .IsRequired()
            .HasDefaultValue("N")
            .HasComment("Đánh dấu đơn có phải quà tặng không: Y (có), N (không). Mặc định là N.");
        builder.Property(x => x.IsBankLoan)
            .HasColumnName("is_bank_loan")
            .HasMaxLength(1)
            .IsRequired()
            .HasDefaultValue("N")
            .HasComment("Có vay từ công ty tài chính hay không: Y (có), N (không). Mặc định là không.");

        // Financial
        builder.Property(x => x.PremiumTotal)
            .HasColumnName("premium_total")
            .HasColumnType("NUMERIC(15,3)")
            .IsRequired()
            .HasComment("Tổng phí bảo hiểm hiện hành");
        builder.Property(x => x.Premium)
            .HasColumnName("premium")
            .HasColumnType("NUMERIC(15,3)")
            .IsRequired()
            .HasComment("Phí bảo hiểm hiện hành");
        builder.Property(x => x.Vat)
            .HasColumnName("vat")
            .HasColumnType("NUMERIC(15,3)")
            .IsRequired()
            .HasComment("Tiền thuế hiện hành");
        builder.Property(x => x.Discount)
            .HasColumnName("discount")
            .HasColumnType("NUMERIC(15,3)")
            .HasComment("Số tiền giảm phí trên tổng đơn hiện hành");
        builder.Property(x => x.DiscountRate)
            .HasColumnName("discount_rate")
            .HasColumnType("NUMERIC(15,3)")
            .HasComment("Tỷ lệ giảm phí trên tổng đơn hiện hành");

        // Channel & Import
        builder.Property(x => x.ChannelId)
            .HasColumnName("channel_id")
            .HasComment("Kênh phân phối");
        builder.Property(x => x.LotImportCode)
            .HasColumnName("lot_import_code")
            .HasMaxLength(50)
            .HasComment("Mã lô import");

        // Insured Info
        builder.Property(x => x.InsuredName)
            .HasColumnName("insured_name")
            .HasMaxLength(250)
            .HasComment("Tên người được bảo hiểm");
        builder.Property(x => x.InsuredIdNo)
            .HasColumnName("insured_id_no")
            .HasMaxLength(15)
            .HasComment("Số CCCD người được bảo hiểm");
        builder.Property(x => x.InsuredTin)
            .HasColumnName("insured_tin")
            .HasMaxLength(15)
            .HasComment("Mã số thuế người được bảo hiểm");
        builder.Property(x => x.InsuredPassport)
            .HasColumnName("insured_passport")
            .HasMaxLength(25)
            .HasComment("Số hộ chiếu người được bảo hiểm");
        builder.Property(x => x.InsuredPhone)
            .HasColumnName("insured_phone")
            .HasMaxLength(15)
            .HasComment("Số điện thoại người được bảo hiểm");
        builder.Property(x => x.InsuredEmail)
            .HasColumnName("insured_email")
            .HasMaxLength(50)
            .HasComment("Email người được bảo hiểm");
        builder.Property(x => x.InsuredProvinceId)
            .HasColumnName("insured_province_id")
            .HasComment("Tỉnh người được bảo hiểm");
        builder.Property(x => x.InsuredWardId)
            .HasColumnName("insured_ward_id")
            .HasComment("Phường/xã người được bảo hiểm");
        builder.Property(x => x.InsuredAddress)
            .HasColumnName("insured_address")
            .HasMaxLength(250)
            .HasComment("Địa chỉ người được bảo hiểm");
        builder.Property(x => x.InsuredFullAddress)
            .HasColumnName("insured_full_address")
            .HasMaxLength(500)
            .HasComment("Địa chỉ đầy đủ người được bảo hiểm");
        builder.Property(x => x.InsuredOrgType)
            .HasColumnName("insured_org_type")
            .HasMaxLength(15)
            .HasComment("Loại người được bảo hiểm: individual (cá nhân), group (tổ chức)");

        // Beneficiary Info
        builder.Property(x => x.BeneficiaryName)
            .HasColumnName("beneficiary_name")
            .HasMaxLength(250)
            .HasComment("Tên người thụ hưởng");
        builder.Property(x => x.BeneficiaryIdNo)
            .HasColumnName("beneficiary_id_no")
            .HasMaxLength(15)
            .HasComment("Số CCCD người thụ hưởng");
        builder.Property(x => x.BeneficiaryTin)
            .HasColumnName("beneficiary_tin")
            .HasMaxLength(15)
            .HasComment("Mã số thuế người thụ hưởng");
        builder.Property(x => x.BeneficiaryPassport)
            .HasColumnName("beneficiary_passport")
            .HasMaxLength(25)
            .HasComment("Số hộ chiếu người thụ hưởng");
        builder.Property(x => x.BeneficiaryPhone)
            .HasColumnName("beneficiary_phone")
            .HasMaxLength(15)
            .HasComment("Số điện thoại người thụ hưởng");
        builder.Property(x => x.BeneficiaryEmail)
            .HasColumnName("beneficiary_email")
            .HasMaxLength(50)
            .HasComment("Email người thụ hưởng");
        builder.Property(x => x.BeneficiaryProvinceId)
            .HasColumnName("beneficiary_province_id")
            .HasComment("Tỉnh người thụ hưởng");
        builder.Property(x => x.BeneficiaryWardId)
            .HasColumnName("beneficiary_ward_id")
            .HasComment("Phường/xã người thụ hưởng");
        builder.Property(x => x.BeneficiaryAddress)
            .HasColumnName("beneficiary_address")
            .HasMaxLength(250)
            .HasComment("Địa chỉ người thụ hưởng");
        builder.Property(x => x.BeneficiaryFullAddress)
            .HasColumnName("beneficiary_full_address")
            .HasMaxLength(500)
            .HasComment("Địa chỉ đầy đủ người thụ hưởng");
        builder.Property(x => x.BeneficiaryOrgType)
            .HasColumnName("beneficiary_org_type")
            .HasMaxLength(15)
            .HasComment("Loại người thụ hưởng: individual (cá nhân), group (tổ chức)");

        // Audit columns
        builder.Property(x => x.ExtraProperties).HasColumnName("extra_properties");
        builder.Property(x => x.CreationTime).HasColumnName("creation_time");
        builder.Property(x => x.CreatorId).HasColumnName("creator_id");
        builder.Property(x => x.LastModificationTime).HasColumnName("last_modification_time");
        builder.Property(x => x.LastModifierId).HasColumnName("last_modifier_id");
        builder.Property(x => x.DeletionTime).HasColumnName("deletion_time");
        builder.Property(x => x.DeleterId).HasColumnName("deleter_id");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted");
        builder.Property(x => x.ConcurrencyStamp).HasColumnName("concurrency_stamp");

        // Indexes
        builder.HasIndex(e => e.PolicyNo, "ix_policy_policy_no")
            .IsUnique();
        builder.HasIndex(e => e.LobId, "ix_policy_lob_id");
        builder.HasIndex(e => e.PolicyTypeId, "ix_policy_policy_type_id");
        builder.HasIndex(e => e.PartnerId, "ix_policy_partner_id");
        builder.HasIndex(e => e.SellerId, "ix_policy_seller_id");
        builder.HasIndex(e => e.ImplementerId, "ix_policy_implementer_id");
        builder.HasIndex(e => e.CurrencyId, "ix_policy_currency_id");
        builder.HasIndex(e => e.ContractId, "ix_policy_contract_id");

        // Foreign Key Relationships
        // Contract (PolicyContract)
        builder.HasOne(x => x.Contract)
            .WithMany()
            .HasForeignKey(x => x.ContractId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_policy_contract_id");

        // LOB (ProLineOfBusiness)
        builder.HasOne(x => x.Lob)
            .WithMany(x => x.Policies)
            .HasForeignKey(x => x.LobId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_policy_lob_id");

        // PolicyType
        builder.HasOne(x => x.PolicyType)
            .WithMany(x => x.Policies)
            .HasForeignKey(x => x.PolicyTypeId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_policy_policy_type_id");

        // Partner (ResPartner)
        builder.HasOne(x => x.Partner)
            .WithMany(x => x.Policies)
            .HasForeignKey(x => x.PartnerId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_policy_partner_id");

        // Seller (HrEmployee)
        builder.HasOne(x => x.Seller)
            .WithMany(x => x.PoliciesAsSeller)
            .HasForeignKey(x => x.SellerId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_policy_seller_id");

        // Implementer (HrEmployee)
        builder.HasOne(x => x.Implementer)
            .WithMany(x => x.PoliciesAsImplementer)
            .HasForeignKey(x => x.ImplementerId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_policy_implementer_id");

        // Currency (ResCurrency)
        builder.HasOne(x => x.Currency)
            .WithMany(x => x.Policies)
            .HasForeignKey(x => x.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_policy_currency_id");
    }
}
