import type { ResRiskStatus } from '@/proxy/res-risks/res-risk-status.enum';

export interface RiskSearchForm {
  objectTypeId: string | null;
  code: string | null;
  name: string | null;
  status: ResRiskStatus | null;
}

export interface RiskFormData {
  objectTypeId: string;
  code: string;
  name: string;
  description: string;
  status: ResRiskStatus;
}

