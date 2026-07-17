import { ProProductStatus } from '@/proxy/pro-products';

export interface ProductSearchForm {
    lobId: string | null;
    productCategoryId: string | null;
    partnerId: string | null;
    code: string | null;
    name: string | null;
    status: ProProductStatus | null;
}

export interface ProductFormData {
    id?: string;
    lobId: string;
    productCategoryId: string | null;
    partnerId: string | null;
    productTypeId: string | null;
    currencyId: string | null;
    rootProductId: string | null;
    code: string;
    insurerProductCode: string | null;
    shortName: string;
    name: string;
    description: string | null;
    internalNote: string | null;
    rateType: string;
    status: ProProductStatus;
    seqNumber: number;
    effectDate: Date | string;
    expireDate: Date | string | null;
    isPlan: string | null;
    planDefinitionId: string | null;
    isRootProduct: string;
    imageDocumentId?: string | null;
    certificateTemplateDocumentId?: string | null;
}
