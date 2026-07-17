import type { CreateClaimDto } from '@/proxy/claim/claims/models';
import type { ProcessClaimType } from '@/proxy/claims/process-claim-type.enum';

export interface ClaimCreateFormData {
  // Process Info
  processClaimType: ProcessClaimType | null;
  processDeptId: string | null;
  processEmpId: string | null;

  // Notifier Info
  notifierName: string | null;
  notifierPhone: string | null;
  notifierEmail: string | null;
  notifierInRelationship: string | null;

  // Contact Info
  contactName: string | null;
  contactPhone: string | null;
  contactEmail: string | null;
  contactInRelationship: string | null;

  // Incident Info
  incidentDate: Date | null;
  certificateNo: string | null;
  insurerId: string | null;

  // Vehicle Info
  carPlate: string | null;
  vin: string | null;
  engineNumber: string | null;

  // Location Info
  incidentProvinceId: string | null;
  incidentWardId: string | null;
  incidentAddress: string | null;
  onLocation: 'Y' | 'N' | null;

  // Cause & Result
  incidentCauseId: string | null;
  incidentDescription: string | null;
  incidentResult: string | null;
  priority: number | null; // 1-3, default = 2

  // Assessment Info
  assessmentDate: Date | null;
  assessmentPartnerId: string | null;

  // Driver Info
  personOnCar: number | null;
  driverName: string | null;
  driverSex: 'M' | 'F' | null;
  driverIdNo: string | null;
  driverPhone: string | null;
  driverLicenseNo: string | null;
  driverLicenseEffectDate: Date | null;
  driverLicenseExpireDate: Date | null;
  driverLicenseLevel: string | null;
  driverRegistryNo: string | null;
  driverRegistryEffectDate: Date | null;
  driverRegistryExpireDate: Date | null;

  // Other
  note: string | null;
}
