import type { HrEmployeeRoleStatus } from '@/proxy/hr-employee-roles/hr-employee-role-status.enum';

export interface EmployeeRoleSearchForm {
  code: string | null;
  name: string | null;
  status: HrEmployeeRoleStatus | null;
}

export interface EmployeeRoleFormData {
  code: string;
  name: string;
  status: HrEmployeeRoleStatus;
}

