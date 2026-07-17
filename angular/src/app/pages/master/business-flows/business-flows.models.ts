import type { BusinessFlowStatus } from '@/proxy/business-flows/business-flow-status.enum';

export interface BusinessFlowSearchForm {
  organizationId: string | null;
  insurerId: string | null;
  businessCode: string | null;
  workflowName: string | null;
  workflowVersion: string | null;
  status: BusinessFlowStatus | null;
  effectDateFrom: Date | null;
  effectDateTo: Date | null;
  expireDateFrom: Date | null;
  expireDateTo: Date | null;
}

export interface BusinessFlowFormData {
  organizationId: string | null;
  insurerId: string | null;
  businessCode: string;
  workflowName: string;
  workflowVersion: string;
  effectDate: Date | null;
  expireDate: Date | null;
}
