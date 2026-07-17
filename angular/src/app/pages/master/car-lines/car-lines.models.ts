import type { ResCarLineStatus } from '@/proxy/res-car-lines/res-car-line-status.enum';

export interface CarLineSearchForm {
  code: string | null;
  name: string | null;
  status: ResCarLineStatus | null;
}

export interface CarLineFormData {
  code: string;
  name: string;
  description: string;
  status: ResCarLineStatus;
}

