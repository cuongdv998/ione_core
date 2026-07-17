import type { ResObjectItemTypeStatus } from '@/proxy/res-object-item-types/res-object-item-type-status.enum';

export interface ResObjectItemTypeSearchForm {
  code: string | null;
  name: string | null;
  status: ResObjectItemTypeStatus | null;
}

export interface ResObjectItemTypeFormData {
  code: string;
  name: string;
  description: string;
  status: ResObjectItemTypeStatus;
}
