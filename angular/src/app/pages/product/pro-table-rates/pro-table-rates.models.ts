import type { ProTableRateStatus } from '@/proxy/pro-table-rates/pro-table-rate-status.enum';
import type { CreateProTableRateVariableDto } from '@/proxy/product/pro-table-rate-variables/models';
import type { CreateProTableRateLineDto } from '@/proxy/product/pro-table-rate-lines/models';

export interface ProTableRateSearchForm {
  lobId: string | null;
  insurerId: string | null;
  code: string | null;
  name: string | null;
  status: ProTableRateStatus | null;
  productId: string | null;
}

export interface ProTableRateFormData {
  lobId: string;
  insurerId: string | null;
  code: string;
  name: string;
  description: string;
  status: ProTableRateStatus;
  variables: CreateProTableRateVariableDto[];
}

export interface ProTableRateVariableFormItem {
  attributeId: string;
  operator: string;
  attributeName?: string; // For display
  attributeCode?: string; // For display
  creationTime?: Date | string; // Audit field
  creatorId?: string; // Audit field
  creatorName?: string; // Audit field (display)
}

export interface ProTableRateLineFormData {
  tableRateId: string;
  coverageId: string | null;
  channelId: string | null;
  partnerId: string | null;
  name: string;
  condition: string;
  minimumRate: number | null;
  baseRate: number | null;
  flatRate: number | null;
  maxDiscount: number | null;
  loading: number | null;
  loadingRate: number | null;
  effectDate: Date | null;
  expireDate: Date | null;
}
