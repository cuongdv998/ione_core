import type { HrEmployeeLevelStatus } from '@/proxy/hr-employee-levels/hr-employee-level-status.enum';

export interface EmployeeLevelSearchForm {
  code: string | null;
  name: string | null;
  status: HrEmployeeLevelStatus | null;
}

export interface EmployeeLevelFormData {
  code: string;
  name: string;
  status: HrEmployeeLevelStatus;
  description: string;
}

