import type { ResFeeItemStatus } from '@/proxy/res-fee-items/res-fee-item-status.enum';

export interface FeeItemSearchForm {
  code: string | null;
  name: string | null;
  status: ResFeeItemStatus | null;
}

export interface FeeItemFormData {
  code: string;
  name: string;
  taxId: string | null;
  description: string;
  status: ResFeeItemStatus;
}
