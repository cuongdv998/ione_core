import type { ResCustomerStatus } from '@/proxy/res-customers/res-customer-status.enum';
import type { ResCustomerSex } from '@/proxy/res-customers/res-customer-sex.enum';

export interface ResCustomerSearchForm {
  filter: string | null;
  code: string | null;
  name: string | null;
  industryId: string | null;
  provinceId: string | null;
  wardId: string | null;
  organizationTypeId: string | null;
  saleId: string | null;
  status: ResCustomerStatus | null;
}

export interface ResCustomerFormData {
  code: string;
  name: string;
  industryId: string | null;
  provinceId: string | null;
  wardId: string | null;
  address: string;
  fullAddress: string; // Computed, readonly
  email: string | null;
  phone: string;
  note: string | null;
  status: ResCustomerStatus;
  // Invoice Address
  invoiceProvinceId: string | null;
  invoiceWardId: string | null;
  invoiceAddress: string | null;
  invoiceFullAddress: string | null; // Computed, readonly
  // Tax and Identity Info
  tin: string | null;
  idNo: string | null; // CN (Cá nhân) field
  passportNo: string | null;
  dob: Date | null;
  sex: ResCustomerSex | null;
  // TC (Tổ chức) fields
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
  organizationTypeId: string | null;
  saleId: string | null;
}

