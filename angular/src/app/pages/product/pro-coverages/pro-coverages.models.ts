import { ProCoverageTermType } from '@/proxy/pro-coverages/pro-coverage-term-type.enum';
import { ProCoverageStatus } from '@/proxy/pro-coverages/pro-coverage-status.enum';

export interface ProCoverageSearchForm {
    lobId: string | null;
    coverageGroupId: string | null;
    coverageTypeId: string | null;
    objectTypeId: string | null;
    type: ProCoverageTermType | null;
    code: string | null;
    name: string | null;
    status: ProCoverageStatus | null;
}
