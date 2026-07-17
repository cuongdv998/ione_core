import { ProLineOfBusinessStatus } from '@/proxy/pro-line-of-businesses';

export interface ProLineOfBusinessSearchForm {
  code: string | null;
  name: string | null;
  status: ProLineOfBusinessStatus | null;
  parentId: string | null;
}
