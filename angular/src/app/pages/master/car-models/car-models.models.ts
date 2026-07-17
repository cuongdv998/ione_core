import type { ResCarBrandStatus } from '@/proxy/res-car-brands/res-car-brand-status.enum';

export interface CarModelSearchForm {
  carBrandId: string | null;
  code: string | null;
  name: string | null;
  status: ResCarBrandStatus | null;
}

export interface CarModelFormData {
  carBrandId: string;
  code: string;
  name: string;
  description: string;
  status: ResCarBrandStatus;
}



