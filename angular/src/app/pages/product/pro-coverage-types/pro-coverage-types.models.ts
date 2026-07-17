import { ProCoverageTypeStatus } from '@/proxy/pro-coverage-types';

export interface ProCoverageTypeSearchForm {
    code: string | null;
    name: string | null;
    status: ProCoverageTypeStatus | null;
}
