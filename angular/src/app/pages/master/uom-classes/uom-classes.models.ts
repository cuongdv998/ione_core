import type { ResUomClassStatus } from '@/proxy/res-uom-classes/res-uom-class-status.enum';

export interface UomClassSearchForm {
  code: string | null;
  name: string | null;
  status: ResUomClassStatus | null;
}

export interface UomClassFormData {
  code: string;
  name: string;
  status: ResUomClassStatus;
}
