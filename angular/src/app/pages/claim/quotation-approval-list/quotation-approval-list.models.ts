/** Approval task UI bucket (aligned with backend ApprovalUiStatus). */
export type QuotationApprovalUiStatus = 'new' | 'approved' | 'rejected';

export interface QuotationApprovalSearchForm {
  lobId: string | null;
  insurerId: string | null;
  processDeptId: string | null;
  /** Selected claim id (yêu cầu bồi thường) */
  claimId: string | null;
  /** new | approved | rejected */
  workTaskStatus: QuotationApprovalUiStatus | '' | null;
  carPlate: string | null;
  vin: string | null;
  engineNumber: string | null;
  folderNo: string | null;
  reporterId: string | null;
}

export interface QuotationApprovalListRow {
  stt?: number;
  workTaskId: string;
  claimId: string;
  claimFolderId: string;
  claimCode: string;
  folderNo: string;
  insurerCode?: string | null;
  lobName?: string | null;
  productName?: string | null;
  carPlate?: string | null;
  reporterName?: string | null;
  creationTime: string;
  actualEndDate?: string | null;
  incidentDate?: string | null;
  onLocation?: string | null;
  totalPascAmount: number;
  /** Backend: lowercase enum name (new, inprogress, completed, …). */
  workTaskStatus: string;
  approvalUiStatus: QuotationApprovalUiStatus;
  /** Work task giám định chi tiết (tab GDV chi tiết / nhận xét). */
  detailAssessmentWorkTaskId?: string | null;
}

export interface QuotationApprovalListQuery {
  skipCount?: number;
  maxResultCount?: number;
  sorting?: string;
  lobId?: string;
  insurerId?: string;
  processDeptId?: string;
  claimId?: string;
  approvalStatus?: QuotationApprovalUiStatus;
  carPlate?: string;
  vin?: string;
  engineNumber?: string;
  folderNo?: string;
  reporterId?: string;
}
