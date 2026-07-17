import type { ResAgreementTermStatus } from '@/proxy/res-agreement-terms/res-agreement-term-status.enum';

export interface AgreementTermSearchForm {
  code: string | null;
  name: string | null;
  status: ResAgreementTermStatus | null;
}

export interface AgreementTermFormData {
  code: string;
  name: string;
  status: ResAgreementTermStatus;
}

