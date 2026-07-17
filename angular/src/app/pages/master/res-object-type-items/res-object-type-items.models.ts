import type { ResObjectTypeItemStatus } from '@/proxy/res-object-type-items/res-object-type-item-status.enum';

export interface ResObjectTypeItemSearchForm {
  code: string | null;
  name: string | null;
  objectTypeId: string | null;
  objectItemType: string | null;
  uomId: string | null;
  status: ResObjectTypeItemStatus | null;
}

export interface ResObjectTypeItemFormData {
  code: string;
  name: string;
  objectTypeId: string;
  objectItemType: string | null;
  uomId: string;
  description: string;
  status: ResObjectTypeItemStatus;
}
