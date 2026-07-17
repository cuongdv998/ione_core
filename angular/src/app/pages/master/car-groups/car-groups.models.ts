import type { ResCarGroupStatus } from '@/proxy/res-car-groups/res-car-group-status.enum';

export interface CarGroupSearchForm {
  carLineId: string | null;
  code: string | null;
  name: string | null;
  status: ResCarGroupStatus | null;
}

export interface CarGroupFormData {
  carLineId: string | null;
  code: string;
  name: string;
  description: string;
  status: ResCarGroupStatus;
}
