import type { ResReasonGroupStatus } from '@/proxy/res-reason-groups/res-reason-group-status.enum';

export interface ResReasonGroupSearchForm {
  code: string | null;
  name: string | null;
  status: ResReasonGroupStatus | null;
}

export interface ResReasonGroupFormData {
  code: string;
  name: string;
  description: string;
  status: ResReasonGroupStatus;
}
