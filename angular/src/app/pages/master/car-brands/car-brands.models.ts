import type { ResCarBrandStatus } from '@/proxy/res-car-brands/res-car-brand-status.enum';

export interface CarBrandSearchForm {
  code: string | null;
  name: string | null;
  status: ResCarBrandStatus | null;
}

export interface CarBrandFormData {
  code: string;
  name: string;
  description: string;
  status: ResCarBrandStatus;
}



