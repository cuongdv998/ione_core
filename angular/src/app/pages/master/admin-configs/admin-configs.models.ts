import type { AdminConfigStatus } from '@/proxy/admin-configs/admin-config-status.enum';

export interface AdminConfigSearchForm {
  code: string | null;
  subCode: string | null;
  name: string | null;
  status: AdminConfigStatus | null;
}

export interface AdminConfigFormData {
  code: string;
  name: string;
  subCode: string;
  value: string;
  description: string | null;
  status: AdminConfigStatus;
}

