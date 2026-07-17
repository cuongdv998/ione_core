import type { ResDocumentTypeStatus } from '@/proxy/res-document-types/res-document-type-status.enum';

export interface DocumentTypeSearchForm {
  code: string | null;
  name: string | null;
  status: ResDocumentTypeStatus | null;
}

export interface DocumentTypeFormData {
  code: string;
  name: string;
  description: string | null;
  status: ResDocumentTypeStatus;
  bucket: string | null;
  documentGroupCode: string | null;
}

