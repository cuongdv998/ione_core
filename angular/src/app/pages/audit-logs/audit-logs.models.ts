/**
 * Models and interfaces for Audit Logs feature
 */

export interface HttpMethodOption {
  label: string;
  value: string;
}

export interface AuditLogSearchForm {
  startTime: Date | null;
  endTime: Date | null;
  httpMethod: string | null;
  url: string | null;
  userName: string | null;
  applicationName: string | null;
  clientIpAddress: string | null;
  correlationId: string | null;
  maxExecutionDuration: number | null;
  minExecutionDuration: number | null;
  hasException: boolean | null;
  httpStatusCode: number | null;
}

export const HTTP_METHOD_OPTIONS: HttpMethodOption[] = [
  { label: 'GET', value: 'GET' },
  { label: 'POST', value: 'POST' },
  { label: 'PUT', value: 'PUT' },
  { label: 'DELETE', value: 'DELETE' },
  { label: 'PATCH', value: 'PATCH' },
  { label: 'OPTIONS', value: 'OPTIONS' },
  { label: 'HEAD', value: 'HEAD' }
];

