import type { PolicyContractType } from '@/proxy/policy-contracts/policy-contract-type.enum';
import type { PolicyContractStatus } from '@/proxy/policy-contracts/policy-contract-status.enum';

export interface PolicyContractSearchForm {
  customerId: string | null;
  code: string | null;
  type: PolicyContractType | null;
  status: PolicyContractStatus | null;
  certificateCode: string | null;
  policyNo: string | null;
  sellerId: string | null;
  effectDateFrom: Date | null;
  effectDateTo: Date | null;
  expireDateFrom: Date | null;
  expireDateTo: Date | null;
  insurerId: string | null;
}
