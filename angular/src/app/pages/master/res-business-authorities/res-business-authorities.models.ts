import type { ResBusinessAuthorityStatus } from '@/proxy/res-business-authorities/res-business-authority-status.enum';

export interface ResBusinessAuthoritySearchForm {
  code: string | null;
  businessCode: string | null;
  name: string | null;
  status: ResBusinessAuthorityStatus | null;
}

export interface ResBusinessAuthorityFormData {
  code: string;
  businessCode: string;
  name: string;
  status: ResBusinessAuthorityStatus;
}
