import { Component, EventEmitter, Input, Output, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { MultiSelectModule } from 'primeng/multiselect';
import { InputNumberModule } from 'primeng/inputnumber';
import { DatePickerModule } from 'primeng/datepicker';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { MessageService, ConfirmationService } from 'primeng/api';

import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';
import type { ClaimObjectTypeDto, ClaimPolicyDto, CreateClaimFolderDto } from '@/proxy/claim/claims/models';
import { ClaimFolderService } from '@/proxy/claim/controllers/claim-folder.service';
import { ClaimTaskService } from '@/proxy/claim/controllers/claim-task.service';
import { HrDepartmentService } from '@/proxy/hr/controllers/hr-department.service';
import { HrEmployeeService } from '@/proxy/hr/controllers/hr-employee.service';

@Component({
  selector: 'app-open-claim-folder-modal',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    DialogModule,
    ButtonModule,
    SelectModule,
    MultiSelectModule,
    InputNumberModule,
    DatePickerModule,
    ConfirmDialogModule,
    ToastModule,
    TranslatePipe
  ],
  templateUrl: './open-claim-folder-modal.component.html',
  styleUrl: './open-claim-folder-modal.component.scss',
  providers: [MessageService, ConfirmationService]
})
export class OpenClaimFolderModalComponent implements OnChanges {
  @Input() visible = false;
  @Output() visibleChange = new EventEmitter<boolean>();

  @Input() claimId: string = '';
  @Input() insurerId: string | null = null;
  @Input() policies: ClaimPolicyDto[] = [];
  /** ISO date string; aligns policy version with claim lookup when resolving coverages → object types. */
  @Input() incidentDate: string | null = null;

  @Output() created = new EventEmitter<void>();

  // Form fields — composite `policyId|proProductId` for unique policy+product row
  productId: string | null = null;
  estimateAmount: number | null = null;
  selectedObjectIds: string[] = [];

  assigneeOrganizationId: string | null = null;
  assigneeId: string | null = null;
  assessmentStartDate: Date | null = null;

  saving = false;

  // Options
  productOptions: Array<{ label: string; value: string }> = [];
  objectTypeOptions: Array<{ label: string; value: string }> = [];
  loadingObjectTypes = false;

  departmentOptions: Array<{ label: string; value: string }> = [];
  loadingDepartments = false;
  employeeOptions: Array<{ label: string; value: string }> = [];
  loadingEmployees = false;

  private suppressConfirmOnHide = false;
  /** Tránh reset form khi giữ dialog mở sau khi user đóng (X/ESC/mask) rồi hủy confirm. */
  private reopenWithoutReset = false;

  constructor(
    private claimFolderService: ClaimFolderService,
    private claimTaskService: ClaimTaskService,
    private departmentService: HrDepartmentService,
    private employeeService: HrEmployeeService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private localizationService: LocalizationService
  ) {}

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['visible'] && this.visible) {
      if (this.reopenWithoutReset) {
        this.reopenWithoutReset = false;
        return;
      }
      this.suppressConfirmOnHide = false;
      this.resetForm();
      this.buildProductOptions();
      this.loadDepartments();
    }
  }

  private resetForm(): void {
    this.productId = null;
    this.objectTypeOptions = [];
    this.estimateAmount = null;
    this.selectedObjectIds = [];
    this.assigneeOrganizationId = null;
    this.assigneeId = null;
    this.assessmentStartDate = null;
    this.employeeOptions = [];
  }

  private buildProductOptions(): void {
    const seen = new Set<string>();
    const options: Array<{ label: string; value: string }> = [];
    for (const p of this.policies || []) {
      const name = p.products?.[0]?.trim();
      const prodId = p.productId;
      const polId = p.policyId;
      if (!name || !prodId || !polId) {
        continue;
      }
      const value = `${polId}|${prodId}`;
      if (seen.has(value)) {
        continue;
      }
      seen.add(value);
      options.push({ label: name, value });
    }
    this.productOptions = options;
  }

  onProductSelectionChange(): void {
    this.selectedObjectIds = [];
    this.objectTypeOptions = [];
    if (!this.productId) {
      return;
    }
    const parts = this.productId.split('|');
    if (parts.length !== 2) {
      return;
    }
    const [policyId, proProductId] = parts;
    this.loadObjectTypesForPolicyProduct(policyId, proProductId);
  }

  private loadObjectTypesForPolicyProduct(policyId: string, proProductId: string): void {
    this.loadingObjectTypes = true;
    this.claimTaskService
      .getObjectTypesByPolicyAndProduct(policyId, proProductId, this.incidentDate ?? undefined)
      .subscribe({
        next: (items: ClaimObjectTypeDto[]) => {
          this.objectTypeOptions = (items || []).map((x: ClaimObjectTypeDto) => ({
            label: x.name || x.id || '',
            value: x.id || ''
          }));
          this.loadingObjectTypes = false;
        },
        error: () => {
          this.loadingObjectTypes = false;
        }
      });
  }

  private loadDepartments(): void {
    this.loadingDepartments = true;
    this.departmentService.getSelectList('UNIT').subscribe({
      next: (items) => {
        this.departmentOptions = (items || []).map((x: { id?: string; name?: string }) => ({
          label: x.name || x.id || '',
          value: x.id || ''
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
    if (!this.assigneeOrganizationId) {
      return;
    }
    this.loadEmployees(this.assigneeOrganizationId);
  }

  private loadEmployees(deptId?: string): void {
    if (!deptId) {
      this.employeeOptions = [];
      return;
    }
    this.loadingEmployees = true;
    this.employeeService
      .getList({ departmentId: deptId, maxResultCount: 500, roleCode: 'GDV' })
      .subscribe({
        next: (res) => {
          this.employeeOptions = (res.items || []).map((x: { id?: string; fullName?: string }) => ({
            label: x.fullName || x.id || '',
            value: x.id || ''
          }));
          this.loadingEmployees = false;
        },
        error: () => {
          this.loadingEmployees = false;
        }
      });
  }

  onVisibleChange(value: boolean): void {
    // Đóng bằng X / ESC / click nền: confirm trước, không đóng dialog rồi mới hỏi.
    if (!value && !this.suppressConfirmOnHide) {
      this.reopenWithoutReset = true;
      queueMicrotask(() => {
        this.visible = true;
      });
      this.visibleChange.emit(true);
      this.confirmClose();
      return;
    }
    this.visibleChange.emit(value);
  }

  close(): void {
    // When closing programmatically (e.g. after execute success),
    // we don't want `p-dialog`'s `onHide` to open the confirm-close popup.
    this.suppressConfirmOnHide = true;
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

  private formatDateTime(date: Date | null): string | undefined {
    if (!date) return undefined;
    return date.toISOString();
  }

  private validate(): boolean {
    if (!this.claimId) {
      return false;
    }
    if (!this.productId) {
      this.messageService.add({
        severity: 'warn',
        summary: this.localizationService.localize('AbpUi::ValidationError'),
        detail: this.localizationService.localize('Claim::ProductRequired') || 'Vui lòng chọn sản phẩm bảo hiểm.'
      });
      return false;
    }
    if (!this.selectedObjectIds || this.selectedObjectIds.length === 0) {
      this.messageService.add({
        severity: 'warn',
        summary: this.localizationService.localize('AbpUi::ValidationError'),
        detail:
          this.localizationService.localize('Claim::ObjectTypeRequired') ||
          'Vui lòng chọn đối tượng bảo hiểm.'
      });
      return false;
    }
    if (this.estimateAmount == null || this.estimateAmount <= 0) {
      this.messageService.add({
        severity: 'warn',
        summary: this.localizationService.localize('AbpUi::ValidationError'),
        detail:
          this.localizationService.localize('Claim::EstimateAmountRequired') ||
          'Vui lòng nhập ước bồi thường hợp lệ.'
      });
      return false;
    }
    if (!this.assigneeOrganizationId || !this.assigneeId) {
      this.messageService.add({
        severity: 'warn',
        summary: this.localizationService.localize('AbpUi::ValidationError'),
        detail:
          this.localizationService.localize('Claim::AssessorInfoRequired') ||
          'Vui lòng nhập đầy đủ thông tin người giám định.'
      });
      return false;
    }
    if (!this.assessmentStartDate) {
      this.messageService.add({
        severity: 'warn',
        summary: this.localizationService.localize('AbpUi::ValidationError'),
        detail:
          this.localizationService.localize('Claim::AssessmentStartDateRequired') ||
          'Vui lòng nhập ngày giám định.'
      });
      return false;
    }
    return true;
  }

  submit(): void {
    if (!this.validate()) {
      return;
    }

    const composite = this.productId!.split('|');
    const selectedProductId = composite.length === 2 ? composite[1] : null;
    if (!selectedProductId) {
      this.messageService.add({
        severity: 'warn',
        summary: this.localizationService.localize('AbpUi::ValidationError'),
        detail: this.localizationService.localize('Claim::ProductRequired') || 'Vui lòng chọn sản phẩm bảo hiểm.'
      });
      return;
    }

    const dto: CreateClaimFolderDto = {
      claimId: this.claimId,
      productId: selectedProductId,
      insurerId: this.insurerId ?? undefined,
      folderName: undefined,
      policyNo: undefined,
      insurerPolicyNo: undefined,
      description: undefined,
      priority: undefined,
      estimateAmount: this.estimateAmount ?? undefined,
      hasAdjustLocation: undefined,
      incidentObjectIds: [...this.selectedObjectIds],
      assigneeOrganizationId: this.assigneeOrganizationId ?? undefined,
      assigneeId: this.assigneeId ?? undefined,
      assessmentStartDate: this.formatDateTime(this.assessmentStartDate)
    };

    this.saving = true;
    this.claimFolderService.create(dto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('AbpUi::Success'),
          detail: this.localizationService.localize('Claim::FolderCreated') || 'Tạo hồ sơ bồi thường thành công.'
        });
        this.saving = false;
        this.created.emit();
        this.close();
      },
      error: () => {
        this.saving = false;
      }
    });
  }
}

