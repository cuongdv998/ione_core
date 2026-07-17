import type { ResAppChannelStatus } from '@/proxy/res-app-channels/res-app-channel-status.enum';
import type { ResAppChannelType } from '@/proxy/res-app-channels/res-app-channel-type.enum';

export interface AppChannelSearchForm {
  code: string | null;
  name: string | null;
  status: ResAppChannelStatus | null;
  type: ResAppChannelType | null;
}

export interface AppChannelFormData {
  code: string;
  name: string;
  description: string;
  status: ResAppChannelStatus;
  type: ResAppChannelType | null;
}
