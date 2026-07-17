import type { SystemEventNotifyStatus } from '@/proxy/system-event-notifies/system-event-notify-status.enum';

export interface SystemEventNotifySearchForm {
  eventCode: string | null;
  appChannelId: string | null;
  recipientType: string | null;
  recipientId: string | null;
  recipient: string | null;
  status: SystemEventNotifyStatus | null;
  scheduleAtFrom: string | null;
  scheduleAtTo: string | null;
  sentAtFrom: string | null;
  sentAtTo: string | null;
}

export interface SystemEventNotifyFormData {
  eventCode: string;
  appChannelId: string;
  title: string;
  body: string;
  payload: string | null;
  recipientType: string;
  recipientId: string;
  recipient: string | null;
  status: SystemEventNotifyStatus;
  scheduleAt: string;
  sentAt: string | null;
  readAt: string | null;
  errorMessage: string | null;
}
