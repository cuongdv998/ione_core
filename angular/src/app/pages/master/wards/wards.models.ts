import type { ResWardStatus } from '@/proxy/res-wards/res-ward-status.enum';

export interface WardSearchForm {
  provinceId: string | null;
  code: string | null;
  name: string | null;
  status: ResWardStatus | null;
}

export interface WardFormData {
  provinceId: string;
  code: string;
  name: string;
  status: ResWardStatus;
  description: string | null;
}

