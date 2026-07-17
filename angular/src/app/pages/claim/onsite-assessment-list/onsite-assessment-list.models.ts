import { ProcessClaimType } from '@/proxy/claims/process-claim-type.enum';
import { WorkTaskStatus } from '@/proxy/claims/work-task-status.enum';

export interface OnsiteAssessmentSearchForm {
  lobId: string | null;
  insurerId: string | null;
  processClaimType: ProcessClaimType | null;
  processDeptId: string | null;
  notifierPhone: string | null;
  openEmployeeId: string | null;
  openDateFrom: Date | null;
  openDateTo: Date | null;
  workTaskStatus: WorkTaskStatus | null;
  carPlate: string | null;
  vin: string | null;
  engineNumber: string | null;
  code: string | null;
}

export interface GetOnsiteAssessmentTasksInput {
  skipCount?: number;
  maxResultCount?: number;
  sorting?: string;
  lobId?: string;
  insurerId?: string;
  processClaimType?: ProcessClaimType;
  processDeptId?: string;
  notifierPhone?: string;
  openEmployeeId?: string;
  openDateFrom?: string;
  openDateTo?: string;
  workTaskStatus?: WorkTaskStatus;
  carPlate?: string;
  vin?: string;
  engineNumber?: string;
  code?: string;
}

export interface OnsiteAssessmentTaskDto {
  id?: string;
  claimId?: string;
  claimAdjustAtLocationId?: string;
  code?: string;
  insurerName?: string;
  insurerCode?: string;
  lobName?: string;
  notifierName?: string;
  notifyDate?: string;
  carPlate?: string;
  reporterName?: string;
  reporterId?: string;
  assigneeName?: string;
  assigneeId?: string;
  processClaimType?: ProcessClaimType;
  workTaskStatus?: WorkTaskStatus;
  canView?: boolean;
  canAccept?: boolean;
  canProcess?: boolean;
  canComplete?: boolean;
  canCancel?: boolean;
  isReporter?: boolean;
  isAssignee?: boolean;
  canReassign?: boolean;
}

export interface OnsiteAssessmentCreateRequestDto {
  claimId?: string;
  claimCode?: string;
  notifyDate?: string;
  notifierName?: string;
  carPlate?: string;
}

export interface CreateOnsiteAssessmentRequestInput {
  claimId: string;
  assigneeOrganizationId?: string;
  assigneeId?: string;
  startDate?: string;
  endDate?: string;
}

export interface OnsiteAssessmentCreateResultDto {
  claimId?: string;
  workTaskId?: string;
}

export interface ReassignOnsiteAssessmentInput {
  assigneeOrganizationId: string;
  assigneeId: string;
  startDate: string;
  endDate: string;
}
