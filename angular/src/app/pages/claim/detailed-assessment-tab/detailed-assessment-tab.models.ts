import type { ClaimDetailDto } from '@/proxy/claim/claims/models';

export interface DetailedAssessmentOptionDto {
  id: string;
  code?: string | null;
  name: string;
  optionKey?: string | null;
  displayName?: string | null;
  objectTypeId?: string | null;
  objectTypeCode?: string | null;
  objectTypeName?: string | null;
  objectTypeGroup?: string | null;
  objectKind?: string | null;
  type?: string | null;
}

export interface DetailedAssessmentAttachmentDto {
  id: string;
  claimDocumentId?: string | null;
  documentId?: string | null;
  fileName: string;
  url?: string | null;
  thumbnailUrl?: string | null;
  mimeType?: string | null;
  uploading?: boolean;
}

export interface DetailedAssessmentItemDto {
  id: string;
  itemId?: string | null;
  itemName?: string | null;
  personName?: string | null;
  personIdNo?: string | null;
  description?: string | null;
  quantity?: number | null;
  riskId?: string | null;
  claimPlanId?: string | null;
  isGenuine?: string | null;
  isRecovery: boolean;
  dischargeDate?: string | Date | null;
  issueDate?: string | Date | null;
  attachments: DetailedAssessmentAttachmentDto[];
}

export interface DetailedAssessmentSectionDto {
  id: string;
  coverageId: string;
  coverageName: string;
  objectTypeId?: string | null;
  objectTypeCode?: string | null;
  objectTypeName?: string | null;
  objectTypeGroup?: string | null;
  objectKind?: string | null;
  items: DetailedAssessmentItemDto[];
}

export interface DetailedAssessmentDocumentRowDto {
  id: string;
  code?: string | null;
  documentTypeId?: string | null;
  name: string;
  complete: boolean | null;
  isCopy: boolean | null;
  note?: string | null;
  issueDate?: string | Date | null;
  attachments: DetailedAssessmentAttachmentDto[];
}

export interface DetailedAssessmentDetailDto {
  workTaskId: string;
  workTaskStatus?: number | string | null;
  claimId: string;
  isReporter?: boolean;
  canReassign?: boolean;
  assessorDeptName?: string | null;
  assessorName?: string | null;
  startDate?: string | null;
  endDate?: string | null;
  driverName?: string | null;
  driverSex?: string | null;
  driverPhone?: string | null;
  driverIdNo?: string | null;
  driverLicenseNo?: string | null;
  driverLicenseEffectDate?: string | null;
  driverLicenseExpireDate?: string | null;
  driverLicenseLevel?: string | null;
  carRegistryNo?: string | null;
  carRegistryEffectDate?: string | null;
  carRegistryExpireDate?: string | null;
  licenseLevelOptions: DetailedAssessmentOptionDto[];
  coverageOptions: DetailedAssessmentOptionDto[];
  riskOptions: DetailedAssessmentOptionDto[];
  claimPlanOptions: DetailedAssessmentOptionDto[];
  itemOptions: DetailedAssessmentOptionDto[];
  sections: DetailedAssessmentSectionDto[];
  documentRows: DetailedAssessmentDocumentRowDto[];
}

export interface SaveDetailedAssessmentInput {
  driverName?: string | null;
  driverSex?: string | null;
  driverPhone?: string | null;
  driverIdNo?: string | null;
  driverLicenseNo?: string | null;
  driverLicenseEffectDate?: string | null;
  driverLicenseExpireDate?: string | null;
  driverLicenseLevel?: string | null;
  carRegistryNo?: string | null;
  carRegistryEffectDate?: string | null;
  carRegistryExpireDate?: string | null;
  sections: DetailedAssessmentSectionDto[];
  documentRows: DetailedAssessmentDocumentRowDto[];
}

export interface DetailedAssessmentFormData {
  assessorDeptName: string;
  assessorName: string;
  startDate: Date | null;
  endDate: Date | null;
  driverName: string;
  driverSex: string | null;
  driverPhone: string;
  driverIdNo: string;
  driverLicenseNo: string;
  driverLicenseEffectDate: Date | null;
  driverLicenseExpireDate: Date | null;
  driverLicenseLevel: string | null;
  carRegistryNo: string;
  carRegistryEffectDate: Date | null;
  carRegistryExpireDate: Date | null;
  selectedCoverageId: string | null;
  sections: DetailedAssessmentSectionDto[];
  documentRows: DetailedAssessmentDocumentRowDto[];
}

export interface DetailedAssessmentTabInputs {
  claim: ClaimDetailDto | null;
  workTaskId: string | null;
}
