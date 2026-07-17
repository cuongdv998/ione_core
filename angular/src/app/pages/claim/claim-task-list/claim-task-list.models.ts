import type { ProcessClaimType } from '@/proxy/claims/process-claim-type.enum';
import type { ClaimStatus } from '@/proxy/claims/claim-status.enum';

export interface ClaimTaskSearchForm {
  lobId: string | null;
  insurerId: string | null;
  processClaimType: ProcessClaimType | null;
  processDeptId: string | null;
  notifierPhone: string | null;
  openEmployeeId: string | null;
  openDateFrom: Date | null;
  openDateTo: Date | null;
  status: ClaimStatus | null;
  carPlate: string | null;
  vin: string | null;
  engineNumber: string | null;
  code: string | null;
}
