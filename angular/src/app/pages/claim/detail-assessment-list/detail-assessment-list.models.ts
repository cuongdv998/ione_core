import { ProcessClaimType } from '@/proxy/claims/process-claim-type.enum';
import { WorkTaskStatus } from '@/proxy/claims/work-task-status.enum';

export interface DetailAssessmentSearchForm {
  lobId: string | null;
  insurerId: string | null;
  processDeptId: string | null;
  notifierPhone: string | null;
  openEmployeeId: string | null;
  openDateFrom: Date | null;
  openDateTo: Date | null;
  processClaimType: ProcessClaimType | null;
  workTaskStatus: WorkTaskStatus | null;
  carPlate: string | null;
  vin: string | null;
  engineNumber: string | null;
  code: string | null;
}

export interface DetailAssessmentTaskRow {
  id: string;
  claimId: string;
  workTaskId: string;
  claimCode: string;
  folderNo: string;
  insurerId?: string | null;
  insurerCode: string;
  lobId?: string | null;
  lobName: string;
  productName: string;
  processDeptId?: string | null;
  carPlate: string;
  vin?: string | null;
  engineNumber?: string | null;
  notifierPhone?: string | null;
  openDate: string;
  openEmployeeId?: string | null;
  openEmployeeName: string;
  incidentDate: string;
  onLocation: boolean;
  processClaimType: ProcessClaimType;
  compensationAmount: number;
  claimStatus?: string | null;
  workTaskStatus: WorkTaskStatus;
  taskCreationTime?: string | null;
  canCancel: boolean;
  isReporter: boolean;
  isAssignee: boolean;
  canReassign: boolean;
  canUpdateCompletedDetailedAssessment: boolean;
}
