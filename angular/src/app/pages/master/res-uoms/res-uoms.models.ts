import type { ResUomStatus } from '@/proxy/res-uoms/res-uom-status.enum';
import type { ResUomType } from '@/proxy/res-uoms/res-uom-type.enum';

export interface ResUomSearchForm {
  classId: string | null;
  code: string | null;
  name: string | null;
  status: ResUomStatus | null;
  type: ResUomType | null;
}

export interface ResUomFormData {
  classId: string;
  code: string;
  name: string;
  status: ResUomStatus;
  rounding: number;
  factor: number | null;
  type: ResUomType;
}
