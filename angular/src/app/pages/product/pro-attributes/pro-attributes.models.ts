import { ProAttributeStatus } from '@/proxy/pro-attributes';

export interface ProAttributeSearchForm {
    code?: string | null;
    name?: string | null;
    status?: ProAttributeStatus | null;
}
