import type { ResCurrencyStatus } from '@/proxy/res-currencies/res-currency-status.enum';

export interface CurrencySearchForm {
  code: string | null;
  name: string | null;
  status: ResCurrencyStatus | null;
}

export interface CurrencyFormData {
  code: string;
  name: string;
  description: string;
  status: ResCurrencyStatus;
}
