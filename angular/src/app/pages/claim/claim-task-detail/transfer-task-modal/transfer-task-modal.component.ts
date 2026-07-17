import { Component, EventEmitter, Input, Output, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { TextareaModule } from 'primeng/textarea';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';

import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';
import { ClaimTaskService } from '@/proxy/claim/controllers/claim-task.service';
import { ResReasonService } from '@/proxy/master/controllers/res-reason.service';
import { HrDepartmentService } from '@/proxy/hr/controllers/hr-department.service';
import { HrEmployeeService } from '@/proxy/hr/controllers/hr-employee.service';
import type { TransferClaimTaskInput } from '@/proxy/claim/claims/models';
import { OnsiteAssessmentDetailService, type OnsiteTransferTaskInput } from '@/pages/claim/onsite-assessment-detail/onsite-assessment-detail.service';
import { DetailedAssessmentTabService } from '@/pages/claim/detailed-assessment-tab/detailed-assessment-tab.service';

@Component({
  selector: 'app-transfer-task-modal',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    DialogModule,
    ButtonModule,
    SelectModule,
    TextareaModule,
    ToastModule,
    TranslatePipe
  ],
  templateUrl: './transfer-task-modal.component.html',
  styleUrl: './transfer-task-modal.component.scss',
  providers: [MessageService]
})
export class TransferTaskModalComponent implements OnChanges {
  @Input() visible = false;
  @Output() visibleChange = new EventEmitter<boolean>();
  @Input() workTaskId: string = '';
  @Input() mode: 'claim' | 'onsite' | 'detailed' = 'claim';
  @Output() transferred = new EventEmitter<void>();

  reasonId: string | null = null;
  reasonDescription = '';
  departmentId: string | null = null;
  assigneeId: string | null = null;
  saving = false;
  reasonOptions: Array<{ label: string; value: string }> = [];
  loadingReasons = false;
  departmentOptions: Array<{ label: string; value: string }> = [];
  loadingDepartments = false;
  employeeOptions: Array<{ label: string; value: string }> = [];
  loadingEmployees = false;

  private readonly reasonGroupCode = 'CLAIM_TRANSFER_REASON';

  constructor(
    private claimTaskService: ClaimTaskService,
    private onsiteAssessmentDetailService: OnsiteAssessmentDetailService,
    private detailedAssessmentTabService: DetailedAssessmentTabService,
    private resReasonService: ResReasonService,
    private departmentService: HrDepartmentService,
    private employeeService: HrEmployeeService,
    private messageService: MessageService,
    private localizationService: LocalizationService
  ) {}

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['visible'] && this.visible) {
      this.reasonId = null;
      this.reasonDescription = '';
      this.departmentId = null;
      this.assigneeId = null;
      this.employeeOptions = [];
      this.loadReasons();
      this.loadDepartments();
    }
  }

  loadReasons(): void {
    this.loadingReasons = true;
    this.resReasonService.getSelectListByGroupCode(this.reasonGroupCode).subscribe({
      next: (items) => {
        this.reasonOptions = (items ?? []).map((x: { id?: string; name?: string }) => ({
          label: x.name ?? x.id ?? '',
          value: x.id ?? ''
        }));
        this.loadingReasons = false;
      },
      error: () => {
        this.loadingReasons = false;
      }
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
    this.loadEmployees(this.departmentId ?? undefined);
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

  focusConfirmButton(): void {
    setTimeout(() => this.getConfirmButton()?.focus(), 100);
  }

  private getConfirmButton(): HTMLButtonElement | null {
    return document.querySelector<HTMLButtonElement>('.p-dialog .transfer-confirm-button');
  }

  close(): void {
    this.visible = false;
    this.visibleChange.emit(false);
  }

  submit(): void {
    if (!this.workTaskId || !this.reasonId || !this.reasonDescription?.trim() || !this.departmentId || !this.assigneeId) {
      this.messageService.add({
        severity: 'warn',
        summary: this.localizationService.localize('AbpUi::ValidationError'),
        detail: this.localizationService.localize('Claim::TransferFieldsRequired')
      });
      return;
    }
    const input: TransferClaimTaskInput | OnsiteTransferTaskInput = {
      reasonId: this.reasonId,
      reasonDescription: this.reasonDescription.trim(),
      departmentId: this.departmentId,
      assigneeId: this.assigneeId
    };
    this.saving = true;
    const request$ = this.mode === 'onsite'
      ? this.onsiteAssessmentDetailService.transfer(this.workTaskId, input as OnsiteTransferTaskInput)
      : this.mode === 'detailed'
        ? this.detailedAssessmentTabService.transfer(this.workTaskId, input as TransferClaimTaskInput)
        : this.claimTaskService.transfer(this.workTaskId, input as TransferClaimTaskInput);

    request$.subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('AbpUi::Success'),
          detail: this.localizationService.localize('Claim::TransferSuccess')
        });
        this.saving = false;
        this.transferred.emit();
        this.close();
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Claim::Error'),
          detail: this.extractErrorMessage(error)
        });
        this.saving = false;
      }
    });
  }

  private extractErrorMessage(error: any): string {
    return error?.error?.error?.message
      || error?.error?.message
      || error?.message
      || this.localizationService.localize('Claim::InternalServerErrorMessage');
  }
}
