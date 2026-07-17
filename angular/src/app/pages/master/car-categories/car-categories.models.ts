import type { ResCarCategoryStatus } from '@/proxy/res-car-categories/res-car-category-status.enum';

export interface CarCategorySearchForm {
  code: string | null;
  name: string | null;
  status: ResCarCategoryStatus | null;
}

export interface CarCategoryFormData {
  carBrandId: string;
  carModelId: string | null;
  motorClassId: string;
  carLineId: string | null;
  code: string;
  name: string;
  seatNumber: number;
  description: string;
  status: ResCarCategoryStatus;
}

