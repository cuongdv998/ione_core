export interface RepairQuotationRow {
  id: string;
  selected: boolean;
  partnerId: string | null;
  partnerName: string | null;
  /** ID của ClaimFolderIncidentObject đang được báo giá */
  incidentObjectId: string | null;
  quotationDate: Date | null;
  totalAmount: number | null;
  documentId: string | null;
  fileName: string | null;
  fileUrl: string | null;
  uploading: boolean;
}

export interface RepairPlanItemCostRow {
  costType: 'man_cost' | 'paint_cost' | 'material_cost';
  costTypeName: string;
  garageAmount: number | null;
  proposedAmount: number | null;
  approvedAmount: number | null;
  discountPercent: number | null;
  discountAmount: number | null;
  finalAmount: number;
  depreciationPercent: number | null;
  depreciationAmount: number;
}

export interface RepairPlanItemRow {
  id: string;
  stt: number;
  itemId: string | null;
  itemName: string;
  planName: string;
  /** claimplanid (RESCLAIMPLAN) — gửi trong JSON để backend sync claim_folder_item_plan */
  claimPlanId: string | null;
  quantity: number;
  costRows: RepairPlanItemCostRow[];
}

export interface RepairPlanOtherCosts {
  assessmentCost: number | null;
  lossPrevCost: number | null;
  rescueCost: number | null;
  otherCost: number | null;
}

export interface RepairPlanFormData {
  quotations: RepairQuotationRow[];
  selectedQuotationId: string | null;
  items: RepairPlanItemRow[];
  otherCosts: RepairPlanOtherCosts;
}

export interface RepairPlanInitDataDto {
  objectTypes: RepairPlanObjectTypeDto[];
  items: RepairPlanAssessmentItemDto[];
  incidentObjects: RepairPlanIncidentObjectDto[];
  savedData?: string | null;
  /** Trạng thái PASC (chuỗi API: new, pending_approval, …). */
  quotationApprovalStatus?: string | null;
  /** Hồ sơ gắn sản phẩm loại VCX (vật chất xe). */
  isPascProductEligible?: boolean;
}

export interface RepairPlanPascEligibilityDto {
  eligible: boolean;
}

export interface RepairPlanObjectTypeDto {
  id: string;
  name: string;
}

export interface RepairPlanIncidentObjectDto {
  id: string;
  objectTypeId: string;
  objectTypeName: string;
  /** Biển số xe (nếu có) */
  carPlate?: string | null;
}

export interface RepairPlanAssessmentItemDto {
  id: string;
  itemId: string;
  itemName: string;
  planName: string;
  /** Guid RESCLAIMPLAN từ backend (có thể là claimPlanId hoặc ClaimPlanId trong JSON API) */
  claimPlanId?: string | null;
  quantity: number;
  objectTypeItemId: string;
  /** Tỷ lệ khấu hao vật tư (%) — tính sẵn từ backend */
  depreciationPercent: number | null;
}

export interface RepairPlanGarageOptionDto {
  id: string;
  name: string;
}

export interface RepairPlanApproverDto {
  id: string;
  name: string;
}

export interface RepairPlanSubmitInfoDto {
  submitterName: string;
  folderNo: string;
  productName: string;
  businessAuthorityName: string;
  approvers: RepairPlanApproverDto[];
  suggestedApproverId: string | null;
}

export interface SaveRepairPlanInput {
  claimId: string;
  claimFolderId?: string | null;
  /**
   * Work task giám định chi tiết — gửi khi lưu nháp và trình duyệt để backend gán đúng ClaimFolderId
   * (khớp với GET init sau F5).
   */
  workTaskId?: string | null;
  partnerId: string | null;
  claimAmount: number;
  discountAmount: number;
  depreciationAmount: number;
  expenseAmount: number;
  assessmentAmount: number | null;
  lossPreventionAmount: number | null;
  rescueAmount: number | null;
  otherAmount: number | null;
  submittedDate?: string | null;
  data: string;
  approverId?: string | null;
  description?: string | null;
}
