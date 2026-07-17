import type { PolicyTypeStatus } from '@/proxy/policy-types/policy-type-status.enum';

export interface PolicyTypeSearchForm {
  code: string | null;
  name: string | null;
  status: PolicyTypeStatus | null;
}

export interface PolicyTypeFormData {
  code: string;
  name: string;
  description: string;
  status: PolicyTypeStatus;
}
