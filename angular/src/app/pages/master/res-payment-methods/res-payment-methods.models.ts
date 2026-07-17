import type { ResPaymentMethodStatus } from '@/proxy/res-payment-methods/res-payment-method-status.enum';

export interface PaymentMethodSearchForm {
  code: string | null;
  name: string | null;
  status: ResPaymentMethodStatus | null;
}

export interface PaymentMethodFormData {
  code: string;
  name: string;
  status: ResPaymentMethodStatus;
}
