import type { InsurerDictionaryStatus } from '@/proxy/insurer-dictionaries/insurer-dictionary-status.enum';

export interface InsurerDictionarySearchForm {
  businessName: string | null;
  insurerId: string | null;
  ownCode: string | null;
  insurerCode: string | null;
  status: InsurerDictionaryStatus | null;
}

/** Form data: Create = all fields; Edit = only editable (insurerCode, extraData, status, effectDate, expireDate) + readonly display (businessName, insurerId, ownCode) */
export interface InsurerDictionaryFormData {
  businessName: string;
  insurerId: string;
  ownCode: string;
  insurerCode: string;
  extraData: string;
  status: InsurerDictionaryStatus;
  effectDate: string;
  expireDate: string;
}
