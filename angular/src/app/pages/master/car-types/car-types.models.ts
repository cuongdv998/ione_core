import type { ResCarTypeStatus } from '@/proxy/res-car-types/res-car-type-status.enum';

export interface CarTypeSearchForm {
  code: string | null;
  name: string | null;
  status: ResCarTypeStatus | null;
}

export interface CarTypeFormData {
  code: string;
  name: string;
  description: string;
  status: ResCarTypeStatus;
}


