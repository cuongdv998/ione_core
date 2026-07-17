import type { ResObjectTypeStatus } from '@/proxy/res-object-types/res-object-type-status.enum';
import type { ResObjectGroup } from '@/proxy/res-object-types/res-object-group.enum';

export interface ObjectTypeSearchForm {
  code: string | null;
  name: string | null;
  objectGroup: ResObjectGroup | null;
  status: ResObjectTypeStatus | null;
}

export interface ObjectTypeFormData {
  code: string;
  name: string;
  objectGroup: ResObjectGroup | null;
  description: string;
  status: ResObjectTypeStatus;
}

