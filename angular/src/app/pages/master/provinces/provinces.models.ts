import type { ResProvinceStatus } from '@/proxy/res-provinces/res-province-status.enum';

export interface ProvinceSearchForm {
  countryId: string | null;
  code: string | null;
  name: string | null;
  status: ResProvinceStatus | null;
}

export interface ProvinceFormData {
  countryId: string;
  code: string;
  name: string;
  status: ResProvinceStatus;
  description: string | null;
}

