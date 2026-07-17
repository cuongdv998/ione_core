import type { ResIndustryStatus } from '@/proxy/res-industries/res-industry-status.enum';

export interface IndustrySearchForm {
  code: string | null;
  name: string | null;
  status: ResIndustryStatus | null;
}

export interface IndustryFormData {
  code: string;
  name: string;
  description?: string;
  status: ResIndustryStatus;
}

