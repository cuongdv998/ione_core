import type { ResSequenceType } from '@/proxy/res-sequences/res-sequence-type.enum';
import type { ResSequenceUseDateRange } from '@/proxy/res-sequences/res-sequence-use-date-range.enum';
import type { ResSequenceDateRangeType } from '@/proxy/res-sequences/res-sequence-date-range-type.enum';
import type { ResSequenceStatus } from '@/proxy/res-sequences/res-sequence-status.enum';

export interface ResSequenceSearchForm {
  code: string | null;
  name: string | null;
  type: ResSequenceType | null;
  status: ResSequenceStatus | null;
}

export interface ResSequenceFormData {
  code: string;
  name: string;
  prefix: string | null;
  suffix: string | null;
  type: ResSequenceType;
  padding: number | null;
  numberNext: number;
  numberIncrement: number;
  useDateRange: ResSequenceUseDateRange;
  dateRangeType: ResSequenceDateRangeType | null;
  status: ResSequenceStatus;
}

