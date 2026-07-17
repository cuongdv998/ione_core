import type { ResEventStatus } from '@/proxy/res-events/res-event-status.enum';

export interface EventSearchForm {
  code: string | null;
  name: string | null;
  status: ResEventStatus | null;
}

export interface EventFormData {
  code: string;
  name: string;
  description: string;
  status: ResEventStatus;
}

export interface EventNotifyTemplateFormData {
  appChannelId: string;
  retryNumber: number;
  title: string;
  body: string;
  data: string;
}

