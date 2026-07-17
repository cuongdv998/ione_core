import type { HrEmployeePositionType } from '@/proxy/hr-employee-positions/hr-employee-position-type.enum';
import type { HrEmployeePositionStatus } from '@/proxy/hr-employee-positions/hr-employee-position-status.enum';

export interface EmployeePositionSearchForm {
  code: string | null;
  name: string | null;
  type: HrEmployeePositionType | null;
  status: HrEmployeePositionStatus | null;
}

export interface EmployeePositionFormData {
  code: string;
  name: string;
  type: HrEmployeePositionType;
  status: HrEmployeePositionStatus;
}

