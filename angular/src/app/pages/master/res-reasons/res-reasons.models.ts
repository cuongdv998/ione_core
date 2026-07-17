import type { ResReasonGroupStatus } from '@/proxy/res-reason-groups/res-reason-group-status.enum';

export interface ResReasonSearchForm {
  groupId: string | null;
  code: string | null;
  name: string | null;
  status: ResReasonGroupStatus | null;
}

export interface ResReasonFormData {
  groupId: string;
  code: string;
  name: string;
  description: string;
  status: ResReasonGroupStatus;
}
