import type { HrEmployeeStatus } from '@/proxy/hr-employees/hr-employee-status.enum';

export interface EmployeeSearchForm {
  code: string | null;
  fullName: string | null;
  status: HrEmployeeStatus | null;
  departmentId: string | null;
  orgId: string | null;
  partnerId: string | null;
  positionId: string | null;
  levelId: string | null;
}

export interface EmployeeFormData {
  code: string;
  fullName: string;
  status: HrEmployeeStatus;
  positionId: string | null;
  levelId: string | null;
  partnerId: string | null;
  orgId: string | null;
  departmentId: string;
  isManager: boolean | null;
  managerId: string | null;
  provinceId: string | null;
  wardId: string | null;
  address: string | null;
  fullAddress: string | null;
  phone: string | null;
  email: string | null;
  userId: string | null;
}

export interface EmployeeRoleFormData {
  roleId: string;
  effectDate: Date | string | null;
  expireDate: Date | string | null;
}

export interface DepartmentTreeNode {
  key: string;
  label: string;
  data: any;
  children?: DepartmentTreeNode[];
  parent?: DepartmentTreeNode;
  expanded?: boolean;
  leaf?: boolean;
}

