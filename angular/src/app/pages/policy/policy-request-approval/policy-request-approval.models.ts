import type { PolicyStatus } from '@/proxy/policies/policy-status.enum';
import type { PolicyContractType } from '@/proxy/policy-contracts/policy-contract-type.enum';
import type { PolicyContractStatus } from '@/proxy/policy-contracts/policy-contract-status.enum';

export type ApprovalTypeFilter = 'all' | 'request' | 'terminate' | 'endorsement';

export interface PolicyRequestApprovalSearchForm {
  approvalTypeFilter: ApprovalTypeFilter;
  policyNo: string | null;
  contractId: string | null;
  policyTypeId: string | null;
  partnerId: string | null;
  status: PolicyStatus | null;
  approvalStatus: string | null;
  channelId: string | null;
  customerId: string | null;
  contractType: PolicyContractType | null;
  contractStatus: PolicyContractStatus | null;
  implementerId: string | null;
  certificateNo: string | null;
  effectiveDateFrom: Date | null;
  effectiveDateTo: Date | null;
  expiryDateFrom: Date | null;
  expiryDateTo: Date | null;
  carPlate: string | null;
  carVin: string | null;
  carEngineNumber: string | null;
  primaryInsurancePartnerId: string | null;
  importLotNumber: string | null;
}
