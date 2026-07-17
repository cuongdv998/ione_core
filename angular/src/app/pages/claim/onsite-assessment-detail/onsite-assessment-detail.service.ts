import { Injectable } from '@angular/core';
import { Rest, RestService } from '@abp/ng.core';

export interface SaveOnsiteAssessmentInput {
  driverName: string;
  driverSex: string;
  driverPhone: string;
  driverIdNo: string;
  driverLicenseNo: string;
  driverLicenseEffectDate?: string;
  driverLicenseExpireDate?: string;
  driverLicenseLevel: string;
  carRegistryNo: string;
  carRegistryEffectDate?: string;
  carRegistryExpireDate?: string;
  lossPositions: string[];
  hasLossThirdParty: boolean;
  witnessTestimony: string;
  causeDescription: string;
  description: string;
  locationDescription: string;
  damageDescription: string;
  partiesInvolvedDescription: string;
  addressPlan: string;
  customerRecommendation: string;
  otherDescription?: string;
  garageId?: string;
  issueDate?: string;
  images?: SaveOnsiteImageInput[];
  documents?: SaveOnsiteDocumentInput[];
}

export interface SaveOnsiteImageInput {
  claimDocumentId?: string;
  documentId?: string;
  documentTypeId?: string;
}

export interface SaveOnsiteDocumentInput {
  claimDocumentId?: string;
  documentId?: string;
  documentTypeId?: string;
  note?: string;
  complete?: 'Y' | 'N';
  isCopy?: 'Y' | 'N';
  issueDate?: string;
}

export interface OnsiteAssessmentDetailDto {
  workTaskId: string;
  workTaskStatus?: number | string;
  claimId: string;
  isReporter?: boolean;
  assessorDeptName?: string;
  assessorName?: string;
  startDate?: string;
  endDate?: string;
  driverName?: string;
  driverSex?: string;
  driverPhone?: string;
  driverIdNo?: string;
  driverLicenseNo?: string;
  driverLicenseEffectDate?: string;
  driverLicenseExpireDate?: string;
  driverLicenseLevel?: string;
  carRegistryNo?: string;
  carRegistryEffectDate?: string;
  carRegistryExpireDate?: string;
  lossPositions?: string[];
  hasLossThirdParty?: boolean;
  witnessTestimony?: string;
  causeDescription?: string;
  description?: string;
  locationDescription?: string;
  damageDescription?: string;
  partiesInvolvedDescription?: string;
  addressPlan?: string;
  customerRecommendation?: string;
  otherDescription?: string;
  garageId?: string;
  garageName?: string;
  issueDate?: string;
  images?: OnsiteAssessmentImageDto[];
  documents?: OnsiteAssessmentDocumentDto[];
}

export interface OnsiteAssessmentImageDto {
  claimDocumentId: string;
  documentId?: string;
  documentTypeId?: string;
  documentTypeName?: string;
  fileName?: string;
  url?: string;
  thumbnailUrl?: string;
  uploadedAt?: string;
  uploaderName?: string;
}

export interface OnsiteAssessmentDocumentDto {
  claimDocumentId: string;
  documentId?: string;
  documentTypeId?: string;
  documentTypeName?: string;
  fileName?: string;
  url?: string;
  thumbnailUrl?: string;
  complete?: string;
  isCopy?: string;
  note?: string;
  issueDate?: string;
}

export interface OnsiteRejectTaskInput {
  reasonId: string;
  reasonDescription: string;
}

export interface OnsiteTransferTaskInput {
  reasonId: string;
  reasonDescription: string;
  departmentId: string;
  assigneeId: string;
}

@Injectable({ providedIn: 'root' })
export class OnsiteAssessmentDetailService {
  private readonly apiName = 'Claim';

  constructor(private readonly restService: RestService) {}

  getDetail = (workTaskId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, OnsiteAssessmentDetailDto>(
      {
        method: 'GET',
        url: `/api/claim-onsite-assessment-tasks/${workTaskId}/detail`,
      },
      { apiName: this.apiName, ...config }
    );

  getDetailByClaimId = (claimId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, OnsiteAssessmentDetailDto>(
      {
        method: 'GET',
        url: `/api/claim-onsite-assessment-tasks/by-claim/${claimId}/detail`,
      },
      { apiName: this.apiName, ...config }
    );

  save = (workTaskId: string, input: SaveOnsiteAssessmentInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>(
      {
        method: 'PUT',
        url: `/api/claim-onsite-assessment-tasks/${workTaskId}/onsite/save`,
        body: input,
      },
      { apiName: this.apiName, ...config }
    );

  saveAndAssign = (workTaskId: string, input: SaveOnsiteAssessmentInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>(
      {
        method: 'PUT',
        url: `/api/claim-onsite-assessment-tasks/${workTaskId}/onsite/save-and-assign`,
        body: input,
      },
      { apiName: this.apiName, ...config }
    );

  removeProfileFile = (workTaskId: string, documentId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>(
      {
        method: 'DELETE',
        url: `/api/claim-onsite-assessment-tasks/${workTaskId}/onsite/profile-files/${documentId}`,
      },
      { apiName: this.apiName, ...config }
    );

  accept = (workTaskId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>(
      {
        method: 'PUT',
        url: `/api/claim-onsite-assessment-tasks/${workTaskId}/accept`,
      },
      { apiName: this.apiName, ...config }
    );

  reject = (workTaskId: string, input: OnsiteRejectTaskInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>(
      {
        method: 'PUT',
        url: `/api/claim-onsite-assessment-tasks/${workTaskId}/reject`,
        body: input,
      },
      { apiName: this.apiName, ...config }
    );

  transfer = (workTaskId: string, input: OnsiteTransferTaskInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>(
      {
        method: 'PUT',
        url: `/api/claim-onsite-assessment-tasks/${workTaskId}/transfer`,
        body: input,
      },
      { apiName: this.apiName, ...config }
    );
}
