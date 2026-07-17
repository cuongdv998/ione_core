import type { HrDepartmentStatus } from '@/proxy/hr-departments/hr-department-status.enum';
import type { HrDepartmentLevel } from '@/proxy/hr-departments/hr-department-level.enum';

export interface DepartmentSearchForm {
  code: string | null;
  name: string | null;
  status: HrDepartmentStatus | null;
  deptLevel: HrDepartmentLevel | null;
  parentId: string | null;
  orgId: string | null;
  typeId: string | null;
  provinceId: string | null;
  wardId: string | null;
  bankId: string | null;
}

export interface DepartmentFormData {
  code: string;
  name: string;
  description: string | null;
  status: HrDepartmentStatus;
  deptLevel: HrDepartmentLevel;
  parentId: string | null;
  orgId: string | null;
  typeId: string | null;
  provinceId: string | null;
  wardId: string | null;
  address: string | null;
  fullAddress: string | null;
  bankId: string | null;
  bankNo: string | null;
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

