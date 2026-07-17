import { Component, EventEmitter, Input, Output, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';

import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';
import { ClaimService } from '@/proxy/claim/controllers/claim.service';
import { HrDepartmentService } from '@/proxy/hr/controllers/hr-department.service';
import { HrEmployeeService } from '@/proxy/hr/controllers/hr-employee.service';
import type { ClaimDetailDto, UpdateClaimDto } from '@/proxy/claim/claims/models';
import { ProcessClaimType } from '@/proxy/claims/process-claim-type.enum';

@Component({
  selector: 'app-assign-assessment-modal',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    DialogModule,
    ButtonModule,
    SelectModule,
    ToastModule,
    TranslatePipe
  ],
  templateUrl: './assign-assessment-modal.component.html',
  styleUrl: './assign-assessment-modal.component.scss',
  providers: [MessageService]
})
export class AssignAssessmentModalComponent implements OnInit, OnChanges {
  @Input() visible = false;
  @Output() visibleChange = new EventEmitter<boolean>();
  @Input() claimId: string = '';
  @Input() currentProcessDeptId: string | undefined;
  @Input() currentProcessEmpId: string | undefined;

  claim: ClaimDetailDto | null = null;
  loading = false;
  saving = false;
  processDeptId: string | null = null;
  processEmpId: string | null = null;
  departmentOptions: Array<{ label: string; value: string }> = [];
  loadingDepartments = false;
  employeeOptions: Array<{ label: string; value: string }> = [];
  loadingEmployees = false;

  constructor(
    private claimService: ClaimService,
    private departmentService: HrDepartmentService,
    private employeeService: HrEmployeeService,
    private messageService: MessageService,
    private localizationService: LocalizationService
  ) {}

  ngOnInit(): void {
    this.loadDepartments();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['visible'] && this.visible && this.claimId) {
      this.processDeptId = this.currentProcessDeptId ?? null;
      this.processEmpId = this.currentProcessEmpId ?? null;
      this.loadClaim();
    }
    if (changes['visible'] && !this.visible) {
      this.claim = null;
    }
  }

  loadClaim(): void {
    if (!this.claimId) return;
    this.loading = true;
    this.claimService.get(this.claimId).subscribe({
      next: (data) => {
        this.claim = data;
        this.processDeptId = data.processDeptId ?? null;
        this.processEmpId = data.processEmpId ?? null;
        this.loading = false;
        this.loadEmployees(this.processDeptId ?? undefined);
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  loadDepartments(): void {
    this.loadingDepartments = true;
    this.departmentService.getList({ maxResultCount: 500 }).subscribe({
      next: (res) => {
        this.departmentOptions = (res.items ?? []).map((x: { id?: string; name?: string }) => ({
          label: x.name ?? x.id ?? '',
          value: x.id ?? ''
        }));
        this.loadingDepartments = false;
      },
      error: () => {
        this.loadingDepartments = false;
      }
    });
  }

  onDepartmentChange(): void {
    this.loadEmployees(this.processDeptId ?? undefined);
    this.processEmpId = null;
  }

  loadEmployees(deptId?: string): void {
    this.loadingEmployees = true;
    this.employeeService.getList({ departmentId: deptId, maxResultCount: 500 }).subscribe({
      next: (res) => {
        this.employeeOptions = (res.items ?? []).map((x: { id?: string; fullName?: string }) => ({
          label: x.fullName ?? x.id ?? '',
          value: x.id ?? ''
        }));
        this.loadingEmployees = false;
      },
      error: () => {
        this.loadingEmployees = false;
      }
    });
  }

  onVisibleChange(value: boolean): void {
    this.visibleChange.emit(value);
  }

  close(): void {
    this.visible = false;
    this.visibleChange.emit(false);
  }

  submit(): void {
    if (!this.claimId || !this.claim) return;
    const dto = this.buildUpdateDto();
    this.saving = true;
    this.claimService.update(this.claimId, dto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('AbpUi::Success'),
          detail: this.localizationService.localize('Claim::AssignSuccess')
        });
        this.saving = false;
        this.close();
      },
      error: () => {
        this.saving = false;
      }
    });
  }

  private buildUpdateDto(): UpdateClaimDto {
    const c = this.claim!;
    const fmt = (d: string | undefined): string => {
      if (!d) return '';
      const x = new Date(d);
      return isNaN(x.getTime()) ? '' : x.toISOString().slice(0, 10);
    };
    return {
      processClaimType: (c.processClaimType ?? 0) as ProcessClaimType,
      processDeptId: this.processDeptId ?? undefined,
      processEmpId: this.processEmpId ?? undefined,
      notifierName: c.notifierName ?? '',
      notifierPhone: c.notifierPhone ?? '',
      notifierEmail: c.notifierEmail,
      notifierInRelationship: c.notifierInRelationship ?? '',
      contactName: c.contactName ?? '',
      contactPhone: c.contactPhone ?? '',
      contactEmail: c.contactEmail,
      contactInRelationship: c.contactInRelationship ?? '',
      incidentDate: fmt(c.incidentDate),
      certificateNo: c.certificateNo,
      insurerId: c.insurerId ?? '',
      carPlate: c.carPlate,
      vin: c.vin,
      engineNumber: c.engineNumber,
      incidentProvinceId: c.incidentProvinceId ?? '',
      incidentWardId: c.incidentWardId ?? '',
      incidentAddress: c.incidentAddress,
      onLocation: c.onLocation ?? '',
      incidentCauseId: c.incidentCauseId ?? '',
      incidentDescription: c.incidentDescription,
      incidentResult: c.incidentResult ?? '',
      priority: c.priority,
      assessmentDate: fmt(c.assessmentDate),
      assessmentPartnerId: c.assessmentPartnerId ?? '',
      personOnCar: c.personOnCar,
      driverName: c.driverName,
      driverPhone: c.driverPhone,
      driverSex: c.driverSex,
      driverIdNo: c.driverIdNo,
      driverLicenseNo: c.driverLicenseNo,
      driverLicenseEffectDate: c.driverLicenseEffectDate,
      driverLicenseExpireDate: c.driverLicenseExpireDate,
      driverLicenseLevel: c.driverLicenseLevel,
      driverRegistryNo: c.driverRegistryNo,
      driverRegistryEffectDate: c.driverRegistryEffectDate,
      driverRegistryExpireDate: c.driverRegistryExpireDate,
      note: c.note
    };
  }
}
