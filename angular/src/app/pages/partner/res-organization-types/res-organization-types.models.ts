import type { ResOrganizationTypeStatus } from '@/proxy/res-organization-types/res-organization-type-status.enum';
import type { OrganizationTypeType } from '@/proxy/res-organization-types/organization-type-type.enum';

export interface ResOrganizationTypeSearchForm {
  code: string | null;
  name: string | null;
  status: ResOrganizationTypeStatus | null;
}

export interface ResOrganizationTypeFormData {
  code: string;
  name: string;
  status: ResOrganizationTypeStatus;
  type: OrganizationTypeType;
}

