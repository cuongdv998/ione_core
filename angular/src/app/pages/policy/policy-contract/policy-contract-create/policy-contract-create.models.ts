import type { PolicyContractType } from '@/proxy/policy-contracts/policy-contract-type.enum';
import type { PolicyContractStatus } from '@/proxy/policy-contracts/policy-contract-status.enum';

export interface PolicyContractFormData {
  insurerId: string | null;
  insurerContractCode: string;
  name: string;
  effectDate: Date | null;
  status: PolicyContractStatus;
  documentId: string | null;
  code: string;
  type: PolicyContractType;
  expireDate: Date | null;
  employeeId: string | null;
  isReciveInvoice: boolean;
  customerId: string | null;
  lobId: string | null;
  payerName: string;
  payerEmail: string;
  payerPhone: string;
  payerProvinceId: string | null;
  payerWardId: string | null;
  payerAddress: string;
  payerFullAddress: string;
  payerTin: string;
  quantity: number;
  currentQuantity: number;
  description: string;
}
