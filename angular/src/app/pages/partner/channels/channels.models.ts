import type { ResChannelStatus } from '@/proxy/res-channels/res-channel-status.enum';

export interface ChannelSearchForm {
  code: string | null;
  name: string | null;
  status: ResChannelStatus | null;
}

export interface ChannelFormData {
  code: string;
  name: string;
  status: ResChannelStatus;
  description: string | null;
}

