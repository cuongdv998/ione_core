import type { ResTaskCategoryBusinessType } from '@/proxy/res-task-categories/res-task-category-business-type.enum';
import type { ResTaskCategoryStatus } from '@/proxy/res-task-categories/res-task-category-status.enum';

export interface ResTaskCategorySearchForm {
  code: string | null;
  businessType: ResTaskCategoryBusinessType | null;
  name: string | null;
  status: ResTaskCategoryStatus | null;
}

export interface ResTaskCategoryFormData {
  businessType: ResTaskCategoryBusinessType;
  code: string;
  name: string;
  status: ResTaskCategoryStatus;
}
