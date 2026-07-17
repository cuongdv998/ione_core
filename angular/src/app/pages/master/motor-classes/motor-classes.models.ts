import type { ResMotorClassStatus } from '@/proxy/res-motor-classes/res-motor-class-status.enum';

export interface MotorClassSearchForm {
  code: string | null;
  name: string | null;
  status: ResMotorClassStatus | null;
}

export interface MotorClassFormData {
  code: string;
  name: string;
  description: string;
  status: ResMotorClassStatus;
}

