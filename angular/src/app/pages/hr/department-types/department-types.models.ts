import type { HrDepartmentTypeStatus } from '@/proxy/hr-department-types/hr-department-type-status.enum';

export interface DepartmentTypeSearchForm {
  code: string | null;
  name: string | null;
  status: HrDepartmentTypeStatus | null;
}

export interface DepartmentTypeFormData {
  code: string;
  name: string;
  status: HrDepartmentTypeStatus;
}

