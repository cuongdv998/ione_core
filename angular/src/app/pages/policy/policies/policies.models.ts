import type { PolicyStatus } from '@/proxy/policies/policy-status.enum';
import type { PolicyContractType } from '@/proxy/policy-contracts/policy-contract-type.enum';
import type { PolicyContractStatus } from '@/proxy/policy-contracts/policy-contract-status.enum';

export interface PolicySearchForm {
  // Existing fields
  policyNo: string | null;
  status: PolicyStatus | null;

  // Changed to dropdown (Guid)
  contractId: string | null;
  policyTypeId: string | null;
  partnerId: string | null;

  // New dropdown fields
  channelId: string | null;
  customerId: string | null;
  contractType: PolicyContractType | null;
  contractStatus: PolicyContractStatus | null;
  implementerId: string | null;

  // New text field
  certificateNo: string | null;

  // New date range fields
  effectiveDateFrom: string | null;
  effectiveDateTo: string | null;
  expiryDateFrom: string | null;
  expiryDateTo: string | null;

  // Additional search filters
  carPlate: string | null;
  carVin: string | null;
  carEngineNumber: string | null;
  primaryInsurancePartnerId: string | null;
  importLotNumber: string | null;

  /** policy_amount.payment_status: new | paid | partial */
  paymentStatus: string | null;
}

export interface PolicyFormData {
  // Contract Information
  contractId: string | null; // Số hợp đồng - select from ResContract
  primaryInsurancePartnerId: string | null; // Maps to Contract.InsurerId
  lobId: string;
  contractType: PolicyContractType | null;
  customerId: string | null; // Khách hàng - from ResCustomer
  payerId: string | null; // Người thanh toán - select from ResCustomer
  payerName: string | null;
  payerPhone: string | null;
  payerEmail: string | null;
  payerAddress: string | null;
  payerFullAddress: string | null;
  payerProvinceId: string | null;
  payerWardId: string | null;
  isReceiveInvoice: boolean; // Có nhận hoá đơn - checkbox
  invoiceRecipient: string | null; // Nhận hóa đơn
  contractNo: string | null;
  contractName: string | null;
  rootPolicyNo: string | null;

  // Policy Information
  policyNo: string; // Số đơn BH - disabled
  lastVersionId: string; // Phiên bản - disabled, default 0
  versionNo: number | null; // Phiên bản (number) - display only (from versionDetail.version)
  sellType: number;
  insurerPolicyNo: string | null;
  policyTypeId: string;
  partnerId: string;
  sellerId: string;
  implementerId: string;
  currencyId: string; // Tiền tệ - from ResCurrency
  exchangeRate: number; // Tỷ giá * - only number
  status: PolicyStatus;
  issueDate: string | null;
  approvalStatus: string | null;
  terminationStatus: number | string | null;
  paymentStatus: string | null; // Trạng thái thanh toán: new, inprogress, done (read-only)
  orgEffectDate: string;
  orgExpireDate: string;
  isRenewal: boolean; // Đơn tái tục - checkbox
  isGift: string;
  premiumTotal: number;
  premium: number;
  vat: number;
  discount: number | null;
  discountRate: number | null;
  isBankLoan: boolean; // Có sử dụng khoản vay - checkbox
  channelId: string | null;
  certificateNo: string | null; // Số GCN
  beneficiaryId: string | null; // Đối tượng thụ hưởng * - select from ResCustomer

  // Vehicle/Insured Object Information
  vehicleOwnerId: string | null; // Chủ xe * - select from ResCustomer
  vehicleOwnerName: string | null;
  vehicleOwnerPhone: string | null;
  vehicleOwnerEmail: string | null;
  vehicleOwnerAddress: string | null;
  vehicleOwnerFullAddress: string | null;
  vehicleOwnerProvinceId: string | null;
  vehicleOwnerWardId: string | null;
  vehicleOwnerIdNo: string | null; // REPIDNO (CCCD chủ xe)
  carUsage: string; // Mục đích kinh doanh * - selectbox: KDVT (có kinh doanh), KKDVT (không kinh doanh)
  carBrandId: string | null; // Hãng xe * - select from ResCarBrand
  carModelId: string | null; // Hiệu xe * - select from ResCarModel (filtered by carBrandId)
  carCategoryId: string | null; // Hiệu xe * - select from ResCarCategory (filtered by carBrandId)
  seatingCapacity: number | null; // Số chỗ ngồi * - only number
  weight: number | null; // Trọng tải (tấn)
  isNewCar: boolean | null; // Xe mới - checkbox
  carValue: number | null; // Giá trị xe (VNĐ)
  carColor: string | null; // Màu sơn - selectbox from admin_config code = CAR_COLOR
  vehiclePlate: string | null; // Biển số xe - input with selectbox next to it
  vehiclePlateType: string | null; // Biển số xe type - selectbox from admin_config code = CAR_PLATE_TYPE
  chassisNumber: string | null; // Số Khung
  engineNumber: string | null; // Số máy
  vehicleTypeId: string | null; // Loại xe * - select from ResCarType
  productionYear: string | Date | null; // Năm sản xuất/Đăng ký lần đầu
  origin: string | null; // Nguồn gốc - selectbox from admin_config code = CAR_ORG
  carLineId: string | null; // Dòng xe * - select from ResCarLine
  carGroupId: string | null; // Nhóm xe * - select from ResCarGroup

  // Insured Person Information
  insuredName: string | null;
  insuredIdNo: string | null;
  insuredPhone: string | null;
  insuredEmail: string | null;
  insuredProvinceId: string | null;
  insuredWardId: string | null;
  insuredAddress: string | null;
  insuredFullAddress: string | null;

  // Beneficiary Information
  beneficiaryName: string | null;
  beneficiaryIdNo: string | null;
  beneficiaryPhone: string | null;
  beneficiaryEmail: string | null;
  beneficiaryProvinceId: string | null;
  beneficiaryWardId: string | null;
  beneficiaryAddress: string | null;
  beneficiaryFullAddress: string | null;

  // Insurance Coverage
  insurancePeriodFrom: string | Date; // Thời hạn bảo hiểm (từ) *
  insurancePeriodTo: string | Date; // Thời hạn bảo hiểm (đến) *
  isVehicleMaterialInsurance: boolean;
  isMandatoryCivilLiability: boolean;
  isVoluntaryCivilLiability: boolean;

  // Summary
  packageType: string | null; // 'BASIC' | 'ADVANCED' | 'COMPREHENSIVE' | 'CUSTOM'
  customerNotes: string | null;
  internalNotes: string | null;
}

export interface CoverageItem {
  // Required identifiers for mapping to API payload
  productCoverageId?: string; // ProProductCoverage.Id
  coverageId?: string; // ProCoverage.Id / ProProductCoverage.CoverageId
  coverageCode?: string; // ProCoverage.Code – gửi kèm cho API tính phí
  coverageParentId?: string;
  /**
   * Hierarchy level derived from productCoverage.parentId (0 = root).
   * Used only for UI indentation in the Benefit column.
   */
  hierarchyLevel?: number;
  uomId?: string;
  taxId?: string;
  taxCode?: string;
  enableQuantity?: string; // 'Y'/'N' from ProProductCoverage.EnableQuantity
  insurerCoverageCode?: string;

  /**
   * Liability Amount constraint (derived from productCoverageLevels.terms where coverageLevelTypeCode === 'LIABILITY_AMOUNT')
   * - If no term found => free input (all undefined)
   * - If fixed => liabilityAmountFixed is set and input should be disabled
   * - If ranged => liabilityAmountMin/liabilityAmountMax are set and input should be constrained
   * - If 2+ point terms (from === to) => liabilityAmountOptions: choose amount via select; fixed/min/max cleared
   */
  liabilityAmountMin?: number | null;
  liabilityAmountMax?: number | null;
  liabilityAmountFixed?: number | null;
  /** Discrete LIABILITY_AMOUNT options; optional isDefault from pro_product_coverage_level_term.is_default = Y */
  liabilityAmountOptions?: Array<{ label: string; value: number; isDefault?: boolean }>;

  /**
   * Deductible options extracted from ProductCoverageLevels.Terms where coverageLevelTypeCode === 'DEDUCTIBLE'
   * (effective only when issueDate is within the level's effect/expire range).
   */
  deductibleOptions?: Array<{ label: string; value: string }>;
  /**
   * Deductible term metadata keyed by the selected deductible value (stringified number).
   * Used to create/update `policy_coverage_level` rows when saving the policy.
   */
  deductibleTermByValue?: Record<
    string,
    {
      coverageLevelTypeId: string;
      coverageLevelBasisId: string;
      amountType: string;
      fromAmount: number;
      toAmount: number;
      conditionScript?: string | null;
      computeScript?: string | null;
    }
  >;

  // Raw interactions from ProProductCoverage (by-lob API)
  productCoverageInteractions?: Array<{
    productCoverageId?: string;
    interactionCoverageId?: string;
    interactionType?: any;
    interactionTypeCode?: string;
    effectDate?: any;
    expireDate?: any;
  }>;

  benefit: string;
  insuranceAmount: number | null;
  quantity: number | null;
  taxRate: number | null;
  premiumRate: number | null;
  premium: number;
  vat: number;
  premiumWithVAT: number;
  // Pricing metadata (persisted to policy_coverage)
  tableRateLineId?: string;
  baseRate?: number | null;
  flatRate?: number | null;
  deductible: string | null;
  /** Snapshot of premium when loading from detail (origin for Tăng/Giảm phí) */
  premiumOrigin?: number | null;
  /** Snapshot of VAT when loading from detail (origin for Tăng/Giảm phí) */
  vatOrigin?: number | null;
  /** Tăng/Giảm phí = (premium + vat) - (premiumOrigin + vatOrigin) = premiumVat - premiumVat snapshot; display only, always disabled, money format */
  premiumChange?: number | null;
  description: string | null;
  coverageType?: number; // ProCoverageTermType: 0 = Main, 1 = Addon, 2 = Exclusion, 3 = Benefit
  selected?: boolean; // For checkbox state when coverageType !== 0
  isSectionHeader?: boolean; // True if this is a section header row
  availabilityType?: string; // ProProductCoverageAvailabilityType: Required, Standard, Optional, Selectable
  isCheckboxDisabled?: boolean; // True if checkbox should be disabled (Required type)
  refundAmount?: number | null; // Refund amount for termination (calculated by API)
}

export interface SummaryItem {
  category: string;
  premiumWithVAT: number;
  discount: number;
  /** Per-product markup (policy_product.markup), same pattern as discount. */
  markup: number;
  /** Product id when this row is a selected product (for discount input binding). */
  productId?: string;
}
