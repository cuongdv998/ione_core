import type { ResBankStatus } from '@/proxy/res-banks/res-bank-status.enum';

export interface BankSearchForm {
  code: string | null;
  name: string | null;
  status: ResBankStatus | null;
}

export interface BankFormData {
  code: string;
  name: string;
  status: ResBankStatus;
}

