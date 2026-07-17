import type { ResDamageLevelStatus } from '@/proxy/res-damage-levels/res-damage-level-status.enum';

export interface DamageLevelSearchForm {
  objectTypeId: string | null;
  code: string | null;
  name: string | null;
  status: ResDamageLevelStatus | null;
}

export interface DamageLevelFormData {
  objectTypeId: string;
  code: string;
  name: string;
  description: string;
  status: ResDamageLevelStatus;
}

