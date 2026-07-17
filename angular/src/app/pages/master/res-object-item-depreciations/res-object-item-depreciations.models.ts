/**
 * Models and interfaces for ResObjectItemDepreciations feature
 */

import { ResObjectItemDepreciationStatus } from '@/proxy/res-object-item-depreciations/res-object-item-depreciation-status.enum';

export interface ResObjectItemDepreciationSearchForm {
  objectTypeItemId: string | null;
  carGroupId: string | null;
  status: ResObjectItemDepreciationStatus | null;
  effectDateFrom: string | null;
  effectDateTo: string | null;
}

export interface ResObjectItemDepreciationFormData {
  objectTypeItemId: string;
  carGroupId: string;
  usedTimeFrom: number;
  usedTimeTo: number;
  depreciationPercent: number;
  effectDate: any;
  expireDate: any;
  status: ResObjectItemDepreciationStatus;
}

export interface StatusOption {
  label: string;
  value: ResObjectItemDepreciationStatus;
}
