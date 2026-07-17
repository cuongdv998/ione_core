import type { ResCountryStatus } from '@/proxy/res-countries/res-country-status.enum';

export interface CountrySearchForm {
  code: string | null;
  name: string | null;
  status: ResCountryStatus | null;
}

export interface CountryFormData {
  code: string;
  name: string;
  status: ResCountryStatus;
}

