import type { ResPartnerStatus } from '@/proxy/res-partners/res-partner-status.enum';
import type { OrganizationTypeType } from '@/proxy/res-organization-types/organization-type-type.enum';

export interface ResPartnerSearchForm {
  code: string | null;
  name: string | null;
  partnerTypeId: string | null;
  organizationTypeId: string | null;
  provinceId: string | null;
  wardId: string | null;
  status: ResPartnerStatus | null;
  partnerRole: string | null;
}

export interface ResPartnerFormData {
  code: string;
  name: string;
  partnerTypeId: string;
  partnerRole: string | null;
  organizationTypeId: string | null;
  channelId: string | null; // Added
  provinceId: string;
  wardId: string;
  address: string;
  fullAddress: string; // Computed, readonly
  email: string | null;
  phone: string;
  note: string | null;
  status: ResPartnerStatus;
  // Invoice Address
  invoiceProvinceId: string | null;
  invoiceWardId: string | null;
  invoiceAddress: string | null;
  invoiceFullAddress: string | null; // Computed, readonly
  // CN (Cá nhân) fields
  idNo: string | null;
  // TC (Tổ chức) fields
  tin: string | null;
  repName: string | null;
  repEmail: string | null;
  repPhone: string | null;
  repIdNo: string | null;
  repTitle: string | null;
  authorizer: string | null;
  authorizerPhone: string | null;
  authorizerEmail: string | null;
  authorizerNo: string | null;
  authorizerDate: Date | null;
  authorizerTitle: string | null;
  businessNo: string | null;
  // Agreements
  agreements: ResPartnerAgreementFormData[];
}

export interface ResPartnerAgreementFormData {
  id?: string; // For edit mode
  agreementTermId: string;
  value: any;
  effectDate: Date | null;
  expireDate: Date | null;
}

