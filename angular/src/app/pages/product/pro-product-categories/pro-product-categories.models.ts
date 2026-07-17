import { ProProductCategoryStatus } from '@/proxy/pro-product-categorys';

export interface ProProductCategorySearchForm {
    code: string | null;
    name: string | null;
    lobId: string | null;
    parentId: string | null;
    status: ProProductCategoryStatus | null;
}
