import type { ResPartnerTypeStatus } from '@/proxy/res-partner-types/res-partner-type-status.enum';

export interface PartnerTypeSearchForm {
  code: string | null;
  name: string | null;
  status: ResPartnerTypeStatus | null;
}

export interface PartnerTypeFormData {
  code: string;
  name: string;
  status: ResPartnerTypeStatus;
}

