import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { DatePickerModule } from 'primeng/datepicker';
import { DialogModule } from 'primeng/dialog';
import { MessageService, ConfirmationService } from 'primeng/api';
import { SelectModule } from 'primeng/select';
import { ToastModule } from 'primeng/toast';

import { LocalizationService } from '@/core/services/localization.service';
import { HrDepartmentService } from '@/proxy/hr/controllers/hr-department.service';
import { HrEmployeeService } from '@/proxy/hr/controllers/hr-employee.service';
import type { ReassignDetailedAssessmentInput } from '@/proxy/claim/claims/models';
import { DetailedAssessmentTabService } from '../detailed-assessment-tab/detailed-assessment-tab.service';

@Component({
  selector: 'app-reassign-detailed-assessment-modal',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    DialogModule,
    ButtonModule,
    SelectModule,
    DatePickerModule,
    ConfirmDialogModule,
    ToastModule,
  ],
  templateUrl: './reassign-detailed-assessment-modal.component.html',
  styleUrl: './reassign-detailed-assessment-modal.component.scss',
  providers: [MessageService, ConfirmationService],
})
export class ReassignDetailedAssessmentModalComponent implements OnChanges {
  @Input() visible = false;
  @Output() visibleChange = new EventEmitter<boolean>();
  @Input() workTaskId = '';
  @Output() reassigned = new EventEmitter<void>();

  assigneeOrganizationId: string | null = null;
  assigneeId: string | null = null;
  startDate: Date | null = null;
  saving = false;

  departmentOptions: Array<{ label: string; value: string }> = [];
  loadingDepartments = false;
  employeeOptions: Array<{ label: string; value: string }> = [];
  loadingEmployees = false;

  constructor(
    private readonly detailedAssessmentTabService: DetailedAssessmentTabService,
    private readonly departmentService: HrDepartmentService,
    private readonly employeeService: HrEmployeeService,
    private readonly messageService: MessageService,
    private readonly confirmationService: ConfirmationService,
    private readonly localizationService: LocalizationService
  ) {}

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['visible'] && this.visible) {
      this.assigneeOrganizationId = null;
      this.assigneeId = null;
      this.startDate = null;
      this.employeeOptions = [];
      this.loadDepartments();
    }
  }

  loadDepartments(): void {
    this.loadingDepartments = true;
    this.departmentService.getSelectList('UNIT').subscribe({
      next: items => {
        this.departmentOptions = (items ?? []).map((x: { id?: string; name?: string }) => ({
          label: x.name ?? x.id ?? '',
          value: x.id ?? '',
        }));
        this.loadingDepartments = false;
      },
      error: () => {
        this.loadingDepartments = false;
      },
    });
  }

  onDepartmentChange(): void {
    this.assigneeId = null;
    this.employeeOptions = [];
    this.loadEmployees(this.assigneeOrganizationId ?? undefined);
  }

  loadEmployees(deptId?: string): void {
    if (!deptId) {
      this.employeeOptions = [];
      return;
    }

    this.loadingEmployees = true;
    this.employeeService.getList({ departmentId: deptId, maxResultCount: 500 }).subscribe({
      next: res => {
        this.employeeOptions = (res.items ?? []).map((x: { id?: string; fullName?: string }) => ({
          label: x.fullName ?? x.id ?? '',
          value: x.id ?? '',
        }));
        this.loadingEmployees = false;
      },
      error: () => {
        this.loadingEmployees = false;
      },
    });
  }

  onVisibleChange(value: boolean): void {
    this.visibleChange.emit(value);
  }

  close(): void {
    this.visible = false;
    this.visibleChange.emit(false);
  }

  confirmClose(): void {
    this.confirmationService.confirm({
      message: this.localizationService.localize('Claim::ConfirmCloseModal'),
      header: this.localizationService.localize('Claim::Close'),
      icon: 'pi pi-question-circle',
      accept: () => {
        this.close();
      },
    });
  }

  submit(): void {
    if (!this.workTaskId || !this.assigneeOrganizationId || !this.assigneeId || !this.startDate) {
      this.messageService.add({
        severity: 'warn',
        summary: this.localizationService.localize('AbpUi::ValidationError'),
        detail: 'Vui lòng nhập đầy đủ thông tin phân công lại.',
      });
      return;
    }

    const input: ReassignDetailedAssessmentInput = {
      assigneeOrganizationId: this.assigneeOrganizationId,
      assigneeId: this.assigneeId,
      startDate: this.startDate.toISOString(),
    };

    this.saving = true;
    this.detailedAssessmentTabService.reassign(this.workTaskId, input).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('AbpUi::Success'),
          detail: 'Phân công lại giám định chi tiết thành công.',
        });
        this.saving = false;
        this.reassigned.emit();
        this.close();
      },
      error: error => {
        this.saving = false;
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Claim::Error') || 'Lỗi',
          detail:
            error?.error?.error?.message ||
            error?.error?.message ||
            this.localizationService.localize('Claim::InternalServerErrorMessage'),
        });
      },
    });
  }
}
