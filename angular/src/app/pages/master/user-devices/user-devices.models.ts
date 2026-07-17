import type { ResUserDeviceStatus } from '@/proxy/res-user-devices/res-user-device-status.enum';

export interface UserDeviceSearchForm {
  userName: string | null;
  deviceUid: string | null;
  appChannelCode: string | null;
  status: ResUserDeviceStatus | null;
}

export interface UserDeviceFormData {
  userName: string;
  deviceUid: string;
  deviceToken: string;
  appChannelCode: string;
  effectDate: string;
  expirDate: string | null;
  status: ResUserDeviceStatus;
  os: string | null;
  deviceName: string | null;
}
