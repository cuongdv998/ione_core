import { Component, EventEmitter, Input, Output, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { DatePickerModule } from 'primeng/datepicker';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';

import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';
import { ClaimTaskService } from '@/proxy/claim/controllers/claim-task.service';
import { HrDepartmentService } from '@/proxy/hr/controllers/hr-department.service';
import { HrEmployeeService } from '@/proxy/hr/controllers/hr-employee.service';
import type { AssignOnsiteAssessmentInput } from '@/proxy/claim/claims/models';

@Component({
  selector: 'app-assign-onsite-assessment-modal',
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
    TranslatePipe
  ],
  templateUrl: './assign-onsite-assessment-modal.component.html',
  styleUrl: './assign-onsite-assessment-modal.component.scss',
  providers: [MessageService, ConfirmationService]
})
export class AssignOnsiteAssessmentModalComponent implements OnChanges {
  @Input() visible = false;
  @Output() visibleChange = new EventEmitter<boolean>();
  @Input() claimId: string = '';
  @Output() assigned = new EventEmitter<void>();

  assigneeOrganizationId: string | null = null;
  assigneeId: string | null = null;
  startDate: Date | null = null;
  endDate: Date | null = null;
  saving = false;

  departmentOptions: Array<{ label: string; value: string }> = [];
  loadingDepartments = false;
  employeeOptions: Array<{ label: string; value: string }> = [];
  loadingEmployees = false;

  private currentEmployeeId: string | null = null;
  private currentDepartmentId: string | null = null;
  private defaultsLoaded = false;

  constructor(
    private claimTaskService: ClaimTaskService,
    private departmentService: HrDepartmentService,
    private employeeService: HrEmployeeService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private localizationService: LocalizationService
  ) {}

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['visible'] && this.visible) {
      this.assigneeOrganizationId = null;
      this.assigneeId = null;
      this.startDate = null;
      this.endDate = null;
      this.employeeOptions = [];
      this.loadDepartments();
      this.loadCurrentEmployee();
    }
  }

  private loadCurrentEmployee(): void {
    if (this.defaultsLoaded) {
      this.assigneeOrganizationId = this.currentDepartmentId;
      this.assigneeId = this.currentEmployeeId;
      if (this.currentDepartmentId) {
        this.loadEmployees(this.currentDepartmentId);
      }
      return;
    }
    this.employeeService.getCurrent().subscribe({
      next: (emp) => {
        if (emp) {
          this.currentEmployeeId = emp.id ?? null;
          this.currentDepartmentId = emp.departmentId ?? null;
          this.assigneeOrganizationId = emp.departmentId ?? null;
          this.assigneeId = emp.id ?? null;
          this.defaultsLoaded = true;
          if (emp.departmentId) {
            this.loadEmployees(emp.departmentId);
          }
        }
      },
      error: () => {}
    });
  }

  loadDepartments(): void {
    this.loadingDepartments = true;
    this.departmentService.getSelectList('UNIT').subscribe({
      next: (items) => {
        this.departmentOptions = (items ?? []).map((x: { id?: string; name?: string }) => ({
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
    this.employeeService.getList({ departmentId: deptId, maxResultCount: 500, roleCode: 'GDV' }).subscribe({
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

  confirmClose(): void {
    this.confirmationService.confirm({
      message: this.localizationService.localize('Claim::ConfirmCloseModal'),
      header: this.localizationService.localize('Claim::Close'),
      icon: 'pi pi-question-circle',
      accept: () => {
        this.close();
      }
    });
  }

  submit(): void {
    if (!this.claimId || !this.assigneeOrganizationId || !this.assigneeId || !this.startDate || !this.endDate) {
      this.messageService.add({
        severity: 'warn',
        summary: this.localizationService.localize('AbpUi::ValidationError'),
        detail: this.localizationService.localize('Claim::AssignOnsiteAssessmentFieldsRequired')
      });
      return;
    }
    if (this.startDate > this.endDate) {
      this.messageService.add({
        severity: 'warn',
        summary: this.localizationService.localize('AbpUi::ValidationError'),
        detail: this.localizationService.localize('Claim::AssessmentStartDateMustBeBeforeOrEqualExpectedCompletionDate')
      });
      return;
    }
    const input: AssignOnsiteAssessmentInput = {
      assigneeOrganizationId: this.assigneeOrganizationId,
      assigneeId: this.assigneeId,
      startDate: this.startDate.toISOString(),
      endDate: this.endDate.toISOString()
    };
    this.saving = true;
    this.claimTaskService.assignOnsiteAssessment(this.claimId, input).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('AbpUi::Success'),
          detail: this.localizationService.localize('Claim::AssignOnsiteAssessmentSuccess')
        });
        this.saving = false;
        this.assigned.emit();
        this.close();
      },
      error: () => {
        this.saving = false;
      }
    });
  }
}
