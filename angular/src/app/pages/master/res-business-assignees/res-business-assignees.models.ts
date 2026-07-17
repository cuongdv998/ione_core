import type { ResBusinessAssigneeStatus } from '@/proxy/res-business-assignees/res-business-assignee-status.enum';
import type { ResBusinessAssigneeType } from '@/proxy/res-business-assignees/res-business-assignee-type.enum';

export interface ResBusinessAssigneeSearchForm {
  organizationId: string | null;
  businessCode: string | null;
  authorityCode: string | null;
  assigneeType: ResBusinessAssigneeType | null;
  departmentId: string | null;
  status: ResBusinessAssigneeStatus | null;
  effectDateFrom: string | null;
  effectDateTo: string | null;
}

export interface ResBusinessAssigneeFormData {
  organizationId: string | null;
  businessCode: string;
  authorityCode: string;
  assigneeType: ResBusinessAssigneeType;
  effectDate: string;
  expireDate: string | null;
  assigneeRole: string | null;
  assigneeId: string | null;
  departmentId: string | null;
  departmentLevel: number | null;
  status: ResBusinessAssigneeStatus;
}
