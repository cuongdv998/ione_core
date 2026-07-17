import { CommonModule } from '@angular/common';
import {
  Component,
  EventEmitter,
  Input,
  OnChanges,
  Output,
  SimpleChanges,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { DatePickerModule } from 'primeng/datepicker';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { AutoCompleteModule, AutoCompleteCompleteEvent } from 'primeng/autocomplete';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { RadioButtonModule } from 'primeng/radiobutton';
import { SelectModule } from 'primeng/select';
import { DialogModule } from 'primeng/dialog';

import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

import type { ClaimDetailDto } from '@/proxy/claim/claims/models';
import type { GetResPartnersInput, ResPartnerDto } from '@/proxy/partner/res-partners/models';
import { ResPartnerStatus } from '@/proxy/res-partners/res-partner-status.enum';
import { ResDocumentService } from '@/proxy/master/controllers/res-document.service';
import { ResDocumentTypeService } from '@/proxy/master/controllers/res-document-type.service';
import { ResPartnerService } from '@/proxy/partner/controllers/res-partner.service';
import { WorkTaskStatus } from '@/proxy/work-tasks/work-task-status.enum';

import { ResPartnerFormDialogComponent } from '@/shared/components/res-partner-form-dialog/res-partner-form-dialog.component';

import type {
  RepairPlanFormData,
  RepairPlanItemCostRow,
  RepairPlanItemRow,
  RepairPlanOtherCosts,
  RepairPlanGarageOptionDto,
  RepairQuotationRow,
  SaveRepairPlanInput,
  RepairPlanAssessmentItemDto,
  RepairPlanSubmitInfoDto,
} from './repair-plan-tab.models';
import { RepairPlanTabService, type RepairPlanSavedDto } from './repair-plan-tab.service';

@Component({
  selector: 'app-repair-plan-tab',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ButtonModule,
    DatePickerModule,
    InputNumberModule,
    InputTextModule,
    AutoCompleteModule,
    ConfirmDialogModule,
    ToastModule,
    RadioButtonModule,
    SelectModule,
    DialogModule,
    ResPartnerFormDialogComponent,
  ],
  templateUrl: './repair-plan-tab.component.html',
  styleUrl: './repair-plan-tab.component.scss',
  providers: [MessageService, ConfirmationService],
})
export class RepairPlanTabComponent implements OnChanges {
  @Input() claim: ClaimDetailDto | null = null;
  @Input() claimId: string | null = null;
  @Input() workTaskId: string | null = null;
  /** Khi true: tab PASC đang được chọn (dùng để gọi lại API init khi người dùng chuyển tab). */
  @Input() repairPlanTabActive = false;
  /**
   * Khi bật (màn giám định chi tiết): chỉ cho Lưu / Trình duyệt nếu {@link detailedAssessmentWorkTaskStatus} là Completed.
   */
  @Input() allowSaveSubmitOnlyWhenDetailedCompleted = false;
  /** Trạng thái công việc giám định chi tiết (từ màn cha). */
  @Input() detailedAssessmentWorkTaskStatus: WorkTaskStatus | null = null;
  /** Khi true: chỉ xem PASC (ẩn Lưu/Trình duyệt, khóa tương tác biểu mẫu). */
  @Input() readOnly = false;
  @Output() closeRequested = new EventEmitter<void>();
  @Output() quotationApprovalStatusChanged = new EventEmitter<string | null>();

  /** Align with ClaimFolderQuotationApprovalStatus DB values — ẩn Lưu/Trình duyệt. */
  private static readonly quotationApprovalStatusesBlockingSaveSubmit = new Set([
    'inprogress',
    'pending_approval',
    'approved',
    'done',
    'cancelled',
  ]);

  loading = false;
  saving = false;
  loadingSubmitInfo = false;
  confirmingSubmit = false;

  submitDialogVisible = false;
  submitInfo: RepairPlanSubmitInfoDto | null = null;
  submitPreviewDate: Date | null = null;
  submitForm: { approverId: string | null; description: string } = {
    approverId: null,
    description: '',
  };

  garageOptions: RepairPlanGarageOptionDto[] = [];
  garageSearchResults: RepairPlanGarageOptionDto[] = [];
  incidentObjectOptions: Array<{ id: string; label: string }> = [];

  form: RepairPlanFormData = this.buildEmptyForm();

  /** Chuỗi trạng thái PASC từ init (new, pending_approval, …). */
  quotationApprovalStatus: string | null = null;

  /** null = chưa load; false = hồ sơ không phải sản phẩm VCX (PASC không áp dụng). */
  isPascProductEligible: boolean | null = null;

  // ── Garage creation (full partner form, default type GARAGE) ─
  addGarageVisible = false;

  private quotationDocTypeId: string | null = null;

  /** Đã load init thành công cho claimId/workTaskId hiện tại — tránh reload khi quay lại tab. */
  private dataLoaded = false;

  constructor(
    private readonly service: RepairPlanTabService,
    private readonly messageService: MessageService,
    private readonly confirmationService: ConfirmationService,
    private readonly resDocumentService: ResDocumentService,
    private readonly resDocumentTypeService: ResDocumentTypeService,
    private readonly resPartnerService: ResPartnerService,
  ) {
    this.loadQuotationDocType();
    this.loadGarages();
  }

  ngOnChanges(changes: SimpleChanges): void {
    const idsReady = !!this.claimId && !!this.workTaskId;
    const idsChanged = !!(changes['claimId'] || changes['workTaskId']);

    if (idsChanged && idsReady) {
      this.quotationApprovalStatus = null;
      this.dataLoaded = false;
      this.loadInitData();
    }

    const tabCh = changes['repairPlanTabActive'];
    if (
      tabCh &&
      !tabCh.firstChange &&
      tabCh.previousValue === false &&
      tabCh.currentValue === true &&
      idsReady &&
      !this.dataLoaded
    ) {
      this.loadInitData();
    }
  }

  /** Cho phép Lưu / Trình duyệt (khi không read-only và thỏa điều kiện GĐ chi tiết nếu được bật). */
  get canSaveOrSubmit(): boolean {
    if (this.readOnly) {
      return false;
    }
    if (!this.allowSaveSubmitOnlyWhenDetailedCompleted) {
      return true;
    }
    return this.detailedAssessmentWorkTaskStatus === WorkTaskStatus.Completed;
  }

  /** Ẩn Lưu/Trình duyệt khi PASC đang luồng khóa (inprogress, pending_approval, approved, done, cancelled). */
  get quotationApprovalBlocksSaveSubmit(): boolean {
    const s = (this.quotationApprovalStatus ?? '').trim().toLowerCase();
    if (!s) {
      return false;
    }
    return RepairPlanTabComponent.quotationApprovalStatusesBlockingSaveSubmit.has(s);
  }

  get canShowSaveSubmitButtons(): boolean {
    return (
      this.isPascProductEligible === true &&
      !this.readOnly &&
      this.canSaveOrSubmit &&
      !this.quotationApprovalBlocksSaveSubmit
    );
  }

  /** Khóa toàn bộ biểu mẫu: read-only màn cha, PASC không áp dụng SP VCX, hoặc PASC đã vào luồng phê duyệt. */
  get formUiLocked(): boolean {
    return (
      this.readOnly ||
      this.isPascProductEligible === false ||
      this.quotationApprovalBlocksSaveSubmit
    );
  }

  /**
   * Chặn Lưu / Trình duyệt khi chưa hoàn thành giám định chi tiết (theo cấu hình màn cha).
   * @returns false nếu hành động không được phép (read-only hoặc chưa Completed khi bật gate).
   */
  private guardSaveSubmitActions(): boolean {
    if (this.readOnly) {
      return false;
    }
    if (this.isPascProductEligible === false) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Cảnh báo',
        detail:
          'Phương án sửa chữa (PASC) chỉ áp dụng cho hồ sơ sản phẩm vật chất xe (VCX).',
      });
      return false;
    }
    if (this.quotationApprovalBlocksSaveSubmit) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Cảnh báo',
        detail: 'Không thể lưu hoặc trình duyệt phương án sửa chữa ở trạng thái hiện tại.',
      });
      return false;
    }
    if (!this.allowSaveSubmitOnlyWhenDetailedCompleted) {
      return true;
    }
    if (this.detailedAssessmentWorkTaskStatus !== WorkTaskStatus.Completed) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Cảnh báo',
        detail: 'Vui lòng hoàn thành giám định chi tiết trước khi lưu hoặc trình duyệt phương án sửa chữa.',
      });
      return false;
    }
    return true;
  }

  get selectedQuotation(): RepairQuotationRow | null {
    return this.form.quotations.find(q => q.id === this.form.selectedQuotationId) ?? null;
  }

  get totalGarageAmount(): number {
    return this.form.items.reduce(
      (sum, item) => sum + item.costRows.reduce((s, c) => s + (c.garageAmount ?? 0), 0),
      0,
    );
  }

  get totalProposedAmount(): number {
    return this.form.items.reduce(
      (sum, item) => sum + item.costRows.reduce((s, c) => s + (c.proposedAmount ?? 0), 0),
      0,
    );
  }

  get totalDiscountAmount(): number {
    return this.form.items.reduce(
      (sum, item) => sum + item.costRows.reduce((s, c) => s + (c.discountAmount ?? 0), 0),
      0,
    );
  }

  get totalFinalAmount(): number {
    return this.form.items.reduce(
      (sum, item) => sum + item.costRows.reduce((s, c) => s + c.finalAmount, 0),
      0,
    );
  }

  get totalDepreciationAmount(): number {
    return this.form.items.reduce(
      (sum, item) =>
        sum +
        item.costRows
          .filter(c => c.costType === 'material_cost')
          .reduce((s, c) => s + c.depreciationAmount, 0),
      0,
    );
  }

  get sectionI1(): number {
    return this.totalProposedAmount;
  }

  get sectionI2(): number {
    return this.totalDiscountAmount;
  }

  get sectionI3(): number {
    return this.totalDepreciationAmount;
  }

  get sectionI4(): number {
    return this.sectionI1 - this.sectionI2 - this.sectionI3;
  }

  get sectionII1(): number {
    return this.form.otherCosts.assessmentCost ?? 0;
  }

  get sectionII2(): number {
    return this.form.otherCosts.lossPrevCost ?? 0;
  }

  get sectionII3(): number {
    return this.form.otherCosts.rescueCost ?? 0;
  }

  get sectionII4(): number {
    return this.form.otherCosts.otherCost ?? 0;
  }

  get sectionIII(): number {
    return this.sectionI4 + this.sectionII1 + this.sectionII2 + this.sectionII3 + this.sectionII4;
  }

  addQuotation(): void {
    const newRow: RepairQuotationRow = {
      id: crypto.randomUUID(),
      selected: false,
      partnerId: null,
      partnerName: null,
      incidentObjectId: null,
      quotationDate: new Date(),
      totalAmount: null,
      documentId: null,
      fileName: null,
      fileUrl: null,
      uploading: false,
    };
    this.form.quotations = [...this.form.quotations, newRow];

    if (this.form.quotations.length === 1) {
      this.selectQuotation(newRow.id);
    }
  }

  selectQuotation(quotationId: string): void {
    this.form.selectedQuotationId = quotationId;
  }

  confirmDeleteQuotation(quotationId: string): void {
    this.confirmationService.confirm({
      message: 'Bạn có muốn xóa báo giá đã chọn không?',
      header: 'Xác nhận xóa',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Có',
      rejectLabel: 'Không',
      accept: () => {
        this.deleteQuotation(quotationId);
      },
    });
  }

  deleteQuotation(quotationId: string): void {
    this.form.quotations = this.form.quotations.filter(q => q.id !== quotationId);
    if (this.form.selectedQuotationId === quotationId) {
      this.form.selectedQuotationId = this.form.quotations[0]?.id ?? null;
    }
  }

  triggerQuotationUpload(quotationId: string, input: HTMLInputElement): void {
    input.setAttribute('data-quotation-id', quotationId);
    input.click();
  }

  onQuotationFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    const quotationId = input.getAttribute('data-quotation-id');
    if (!file || !quotationId) {
      input.value = '';
      return;
    }

    const row = this.form.quotations.find(q => q.id === quotationId);
    if (!row) {
      input.value = '';
      return;
    }

    if (!this.quotationDocTypeId) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Cảnh báo',
        detail: 'Chưa tải được loại tài liệu báo giá.',
      });
      input.value = '';
      return;
    }

    row.uploading = true;
    row.fileName = file.name;
    row.fileUrl = null;
    row.documentId = null;

    this.resDocumentService
      .uploadSingleFile(this.quotationDocTypeId, 'CLAIM_QUOTATION', file)
      .subscribe({
        next: doc => {
          row.uploading = false;
          row.documentId = doc.id ?? null;
          row.fileName = doc.fileName ?? file.name;
          row.fileUrl = doc.url ?? null;
        },
        error: () => {
          row.uploading = false;
          row.fileName = null;
          this.messageService.add({
            severity: 'error',
            summary: 'Lỗi',
            detail: 'Không tải được file báo giá.',
          });
        },
      });

    input.value = '';
  }

  downloadQuotationFile(row: RepairQuotationRow): void {
    if (!row.fileUrl && !row.documentId) return;

    const doDownload = (url: string) => {
      const a = document.createElement('a');
      a.href = url;
      a.download = row.fileName || 'bao-gia';
      a.target = '_blank';
      a.rel = 'noopener';
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
    };

    if (row.fileUrl) {
      doDownload(row.fileUrl);
      return;
    }

    this.resDocumentService.getSingleFile(row.documentId!).subscribe({
      next: res => {
        if (res.url) {
          row.fileUrl = res.url;
          doDownload(res.url);
        }
      },
    });
  }

  onGarageSearch(event: AutoCompleteCompleteEvent): void {
    const q = (event.query || '').toLowerCase();
    this.garageSearchResults = this.garageOptions.filter(g =>
      (g.name || '').toLowerCase().includes(q),
    );
  }

  onGarageSelected(row: RepairQuotationRow, selectedValue: unknown): void {
    const opt = selectedValue as RepairPlanGarageOptionDto | null;
    if (opt && typeof opt === 'object' && 'id' in opt) {
      row.partnerId = opt.id;
      row.partnerName = opt.name;
    } else {
      row.partnerId = null;
      row.partnerName = null;
    }
  }

  onGarageChange(row: RepairQuotationRow, partnerId: string | null): void {
    const selected = partnerId ? this.garageOptions.find(option => option.id === partnerId) ?? null : null;
    this.onGarageSelected(row, selected);
  }

  getGarageObject(row: RepairQuotationRow): RepairPlanGarageOptionDto | null {
    if (!row.partnerId) return null;
    return this.garageOptions.find(g => g.id === row.partnerId) ?? null;
  }

  // ── Garage creation dialog ──────────────────────────────────

  openAddGarageDialog(): void {
    if (this.formUiLocked) return;
    this.addGarageVisible = true;
  }

  onGarageCreated(partner: ResPartnerDto): void {
    const newOpt: RepairPlanGarageOptionDto = {
      id: partner.id ?? '',
      name: partner.name ?? '',
    };
    const withoutDup = this.garageOptions.filter(g => g.id !== newOpt.id);
    this.garageOptions = [...withoutDup, newOpt];
    this.garageSearchResults = [...this.garageOptions];
    this.messageService.add({
      severity: 'success',
      summary: 'Thành công',
      detail: `Đã thêm garage "${newOpt.name}".`,
    });
  }

  onCostDiscountPercentChange(costRow: RepairPlanItemCostRow): void {
    if (costRow.discountPercent == null || costRow.discountPercent <= 0) {
      costRow.discountPercent = null;
      costRow.discountAmount = null;
    } else {
      const proposed = costRow.proposedAmount ?? 0;
      costRow.discountAmount = Math.round((proposed * costRow.discountPercent) / 100);
    }
    this.recalcCostRow(costRow);
  }

  onCostDiscountAmountChange(costRow: RepairPlanItemCostRow): void {
    if (costRow.discountAmount == null || costRow.discountAmount <= 0) {
      costRow.discountAmount = null;
      costRow.discountPercent = null;
    } else {
      const proposed = costRow.proposedAmount ?? 0;
      costRow.discountPercent = proposed > 0 ? Math.round((costRow.discountAmount / proposed) * 100) : null;
    }
    this.recalcCostRow(costRow);
  }

  onDepreciationPercentChange(costRow: RepairPlanItemCostRow): void {
    if (costRow.depreciationPercent != null && costRow.depreciationPercent < 0) {
      costRow.depreciationPercent = 0;
    }
    this.recalcCostRow(costRow);
  }

  onProposedAmountChange(costRow: RepairPlanItemCostRow): void {
    if (costRow.discountPercent != null && costRow.discountPercent > 0) {
      const proposed = costRow.proposedAmount ?? 0;
      costRow.discountAmount = Math.round((proposed * costRow.discountPercent) / 100);
    } else if (costRow.discountAmount != null) {
      const proposed = costRow.proposedAmount ?? 0;
      costRow.discountPercent = proposed > 0 ? Math.round((costRow.discountAmount / proposed) * 100) : null;
    }
    this.recalcCostRow(costRow);
  }

  recalcCostRow(costRow: RepairPlanItemCostRow): void {
    const proposed = costRow.proposedAmount ?? 0;
    const discount = costRow.discountAmount ?? 0;
    costRow.finalAmount = Math.max(0, proposed - discount);

    if (costRow.costType === 'material_cost' && costRow.depreciationPercent != null) {
      costRow.depreciationAmount = Math.round(
        (costRow.finalAmount * costRow.depreciationPercent) / 100,
      );
    } else {
      costRow.depreciationAmount = 0;
    }

    // Giá duyệt = Thành tiền − Tiền khấu hao (đồng bộ backend ClaimRepairPlanAppService)
    costRow.approvedAmount = Math.max(0, costRow.finalAmount - costRow.depreciationAmount);
  }

  /** Tính lại finalAmount / khấu hao / giá duyệt cho toàn bộ form (sau load init hoặc restore JSON). */
  private recalcAllFormCostRows(): void {
    for (const item of this.form.items) {
      for (const cost of item.costRows) {
        this.recalcCostRow(cost);
      }
    }
  }

  private readAssessmentClaimPlanId(item: RepairPlanAssessmentItemDto): string | null {
    if (item.claimPlanId != null && item.claimPlanId !== '') {
      return String(item.claimPlanId);
    }
    const r = item as unknown as Record<string, unknown>;
    const raw = r['ClaimPlanId'] ?? r['claimPlanId'];
    if (raw == null || raw === '') {
      return null;
    }
    return String(raw);
  }

  close(): void {
    this.confirmationService.confirm({
      message: 'Thông tin chưa được lưu, bạn có chắc chắn muốn đóng màn hình này?',
      header: 'Xác nhận đóng',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Có',
      rejectLabel: 'Không',
      accept: () => this.closeRequested.emit(),
    });
  }

  save(): void {
    if (!this.guardSaveSubmitActions()) return;
    if (!this.validateForm()) return;

    this.confirmationService.confirm({
      message: 'Bạn có chắc chắn muốn lưu thông tin đã nhập?',
      header: 'Xác nhận lưu',
      icon: 'pi pi-question-circle',
      acceptLabel: 'Có',
      rejectLabel: 'Không',
      accept: () => this.doSave('draft'),
    });
  }

  submit(): void {
    if (!this.guardSaveSubmitActions()) return;
    if (!this.validateForm()) return;
    if (!this.claimId || !this.workTaskId) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Cảnh báo',
        detail: 'Thiếu thông tin claim hoặc công việc.',
      });
      return;
    }

    this.loadingSubmitInfo = true;
    this.service.getSubmitInfo(this.claimId, this.workTaskId).subscribe({
      next: raw => {
        this.loadingSubmitInfo = false;
        const info = this.normalizeSubmitInfo(raw);
        this.submitInfo = info;
        this.submitPreviewDate = new Date();
        this.submitForm = {
          approverId: info.suggestedApproverId ?? info.approvers[0]?.id ?? null,
          description: '',
        };
        this.submitDialogVisible = true;
        if (!info.approvers.length) {
          this.messageService.add({
            severity: 'warn',
            summary: 'Cảnh báo',
            detail:
              'Chưa cấu hình người duyệt (CLAIM_QUOTAION_APPROVAL / APPROVAL_LEVEL_ONE). Không thể thực hiện trình duyệt.',
          });
        }
      },
      error: () => {
        this.loadingSubmitInfo = false;
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: 'Không tải được thông tin trình duyệt.',
        });
      },
    });
  }

  cancelSubmitDialog(): void {
    if (this.confirmingSubmit) return;
    this.confirmationService.confirm({
      message: 'Thông tin chưa được lưu, bạn có chắc chắn muốn đóng màn hình này?',
      header: 'Xác nhận đóng',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Có',
      rejectLabel: 'Không',
      accept: () => {
        this.submitDialogVisible = false;
      },
    });
  }

  confirmSubmit(): void {
    if (!this.guardSaveSubmitActions()) return;
    if (!this.submitForm.approverId) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Cảnh báo',
        detail: 'Vui lòng chọn người duyệt.',
      });
      return;
    }
    if (!this.claimId || !this.workTaskId) return;

    this.confirmingSubmit = true;
    const input = this.buildSaveInput(true);
    input.approverId = this.submitForm.approverId;
    input.description = this.submitForm.description?.trim() ? this.submitForm.description.trim() : null;

    this.service.submit(input).subscribe({
      next: (saved: RepairPlanSavedDto) => {
        this.confirmingSubmit = false;
        this.submitDialogVisible = false;
        this.quotationApprovalStatus =
          this.normalizeQuotationApprovalStatusString(saved?.status) ?? 'pending_approval';
        this.quotationApprovalStatusChanged.emit(this.quotationApprovalStatus);
        this.messageService.add({
          severity: 'success',
          summary: 'Thành công',
          detail: 'Đã trình duyệt phương án sửa chữa.',
        });
        this.loadInitData();
      },
      error: () => {
        this.confirmingSubmit = false;
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: 'Trình duyệt thất bại. Vui lòng thử lại.',
        });
      },
    });
  }

  formatSubmitDisplayDate(d: Date | null): string {
    if (!d) return '';
    const pad = (n: number) => n.toString().padStart(2, '0');
    return `${pad(d.getHours())}:${pad(d.getMinutes())}, ${pad(d.getDate())}/${pad(d.getMonth() + 1)}/${d.getFullYear()}`;
  }

  formatCurrency(value: number | null | undefined): string {
    if (value == null) return '0';
    return value.toLocaleString('vi-VN');
  }

  private buildEmptyForm(): RepairPlanFormData {
    return {
      quotations: [],
      selectedQuotationId: null,
      items: [],
      otherCosts: {
        assessmentCost: null,
        lossPrevCost: null,
        rescueCost: null,
        otherCost: null,
      },
    };
  }

  private loadInitData(): void {
    if (!this.claimId || !this.workTaskId) return;
    this.loading = true;
    this.isPascProductEligible = null;

    this.service.getInitData(this.claimId, this.workTaskId).subscribe({
      next: data => {
        const d = data as unknown as Record<string, unknown>;
        const eligibleRaw = d['isPascProductEligible'] ?? d['IsPascProductEligible'];
        this.isPascProductEligible =
          typeof eligibleRaw === 'boolean'
            ? eligibleRaw
            : eligibleRaw === 'false' || eligibleRaw === false
              ? false
              : true;

        const raw =
          data.quotationApprovalStatus ??
          (typeof d['QuotationApprovalStatus'] === 'string' ? (d['QuotationApprovalStatus'] as string) : null);
        this.quotationApprovalStatus = this.normalizeQuotationApprovalStatusString(raw);
        this.quotationApprovalStatusChanged.emit(this.quotationApprovalStatus);

        this.incidentObjectOptions = (data.incidentObjects ?? []).map(o => ({
          id: o.id,
          label: o.carPlate ? `${o.objectTypeName} - ${o.carPlate}` : o.objectTypeName,
        }));
        const fallbackForm: RepairPlanFormData = {
          ...this.buildEmptyForm(),
          items: this.buildItemRows(data.items ?? []),
        };
        this.form = this.restoreSavedFormData(data.savedData, fallbackForm);
        this.recalcAllFormCostRows();
        this.mergeQuotationPartnerRefsIntoGarageOptions();
        this.loading = false;
        this.dataLoaded = true;
      },
      error: () => {
        this.loading = false;
        this.dataLoaded = false;
        this.quotationApprovalStatus = null;
        this.isPascProductEligible = null;
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: 'Không tải được dữ liệu phương án sửa chữa.',
        });
      },
    });
  }

  private normalizeQuotationApprovalStatusString(raw: unknown): string | null {
    if (typeof raw !== 'string') {
      return null;
    }
    const s = raw.trim().toLowerCase();
    return s !== '' ? s : null;
  }

  private buildItemRows(assessmentItems: RepairPlanAssessmentItemDto[]): RepairPlanItemRow[] {
    return assessmentItems.map((item, idx) => {
      const costRows = this.buildDefaultCostRows(item.depreciationPercent);
      costRows.forEach(c => this.recalcCostRow(c));
      return {
        id: item.id,
        stt: idx + 1,
        itemId: item.itemId,
        itemName: item.itemName,
        planName: item.planName,
        claimPlanId: this.readAssessmentClaimPlanId(item),
        quantity: item.quantity,
        costRows,
      };
    });
  }

  private buildDefaultCostRows(materialDepreciationPercent: number | null): RepairPlanItemCostRow[] {
    const costTypes: Array<{ type: 'man_cost' | 'paint_cost' | 'material_cost'; name: string }> = [
      { type: 'man_cost', name: 'CP nhân công' },
      { type: 'paint_cost', name: 'CP sơn' },
      { type: 'material_cost', name: 'CP phụ tùng' },
    ];

    return costTypes.map(ct => ({
      costType: ct.type,
      costTypeName: ct.name,
      garageAmount: null,
      proposedAmount: null,
      approvedAmount: null,
      discountPercent: null,
      discountAmount: null,
      finalAmount: 0,
      depreciationPercent: ct.type === 'material_cost' ? materialDepreciationPercent : null,
      depreciationAmount: 0,
    }));
  }

  private restoreSavedFormData(
    savedData: string | null | undefined,
    fallbackForm: RepairPlanFormData,
  ): RepairPlanFormData {
    if (!savedData) {
      return fallbackForm;
    }

    try {
      const parsed = JSON.parse(savedData) as Partial<RepairPlanFormData>;
      const quotations = this.normalizeQuotations(parsed.quotations);
      const selectedQuotationId =
        (parsed.selectedQuotationId && quotations.some(q => q.id === parsed.selectedQuotationId))
          ? parsed.selectedQuotationId
          : (quotations[0]?.id ?? null);
      return {
        quotations,
        selectedQuotationId,
        items: this.normalizeItems(parsed.items, fallbackForm.items),
        otherCosts: {
          assessmentCost: parsed.otherCosts?.assessmentCost ?? null,
          lossPrevCost: parsed.otherCosts?.lossPrevCost ?? null,
          rescueCost: parsed.otherCosts?.rescueCost ?? null,
          otherCost: parsed.otherCosts?.otherCost ?? null,
        },
      };
    } catch {
      return fallbackForm;
    }
  }

  private normalizeQuotations(quotations: unknown): RepairQuotationRow[] {
    if (!Array.isArray(quotations)) {
      return [];
    }

    return quotations.map(q => {
      const row = (q ?? {}) as Partial<RepairQuotationRow>;
      return {
        id: row.id ?? crypto.randomUUID(),
        selected: !!row.selected,
        partnerId: row.partnerId ?? null,
        partnerName: row.partnerName ?? null,
        incidentObjectId: row.incidentObjectId ?? null,
        quotationDate: row.quotationDate ? new Date(row.quotationDate) : null,
        totalAmount: row.totalAmount ?? null,
        documentId: row.documentId ?? null,
        fileName: row.fileName ?? null,
        fileUrl: row.fileUrl ?? null,
        uploading: false,
      };
    });
  }

  private normalizeItems(items: unknown, fallbackItems: RepairPlanItemRow[]): RepairPlanItemRow[] {
    if (!Array.isArray(items)) {
      return fallbackItems;
    }

    return items.map((item, index) => {
      const row = (item ?? {}) as Partial<RepairPlanItemRow>;
      const fallbackItem = fallbackItems.find(x => x.id === row.id) ?? fallbackItems[index];
      const normalizedCostRows = this.normalizeCostRows(row.costRows);
      const claimPlanIdRaw = row.claimPlanId ?? (row as { claimplanId?: string }).claimplanId;
      const mergedClaimPlanId =
        claimPlanIdRaw != null && claimPlanIdRaw !== ''
          ? String(claimPlanIdRaw)
          : (fallbackItem?.claimPlanId ?? null);
      return {
        id: row.id ?? fallbackItems[index]?.id ?? crypto.randomUUID(),
        stt: typeof row.stt === 'number' ? row.stt : index + 1,
        itemId: row.itemId ?? null,
        itemName: row.itemName ?? '',
        planName: row.planName ?? '',
        claimPlanId: mergedClaimPlanId,
        quantity: row.quantity ?? 0,
        costRows: normalizedCostRows.length > 0 ? normalizedCostRows : (fallbackItem?.costRows ?? []),
      };
    });
  }

  private normalizeCostRows(costRows: unknown): RepairPlanItemCostRow[] {
    if (!Array.isArray(costRows)) {
      return [];
    }

    return costRows.map(cost => {
      const row = (cost ?? {}) as Partial<RepairPlanItemCostRow>;
      return {
        costType: (row.costType as RepairPlanItemCostRow['costType']) ?? 'man_cost',
        costTypeName: row.costTypeName ?? '',
        garageAmount: row.garageAmount ?? null,
        proposedAmount: row.proposedAmount ?? null,
        approvedAmount: row.approvedAmount ?? null,
        discountPercent: row.discountPercent ?? null,
        discountAmount: row.discountAmount ?? null,
        finalAmount: row.finalAmount ?? 0,
        depreciationPercent: row.depreciationPercent ?? null,
        depreciationAmount: row.depreciationAmount ?? 0,
      };
    });
  }

  private normalizeSubmitInfo(raw: unknown): RepairPlanSubmitInfoDto {
    const r = raw as Record<string, unknown>;
    const approversRaw = (r['approvers'] ?? r['Approvers'] ?? []) as unknown[];
    const approvers = approversRaw.map(a => {
      const row = (a ?? {}) as Record<string, unknown>;
      return {
        id: String(row['id'] ?? row['Id'] ?? ''),
        name: String(row['name'] ?? row['Name'] ?? ''),
      };
    });
    const suggested = r['suggestedApproverId'] ?? r['SuggestedApproverId'];
    return {
      submitterName: String(r['submitterName'] ?? r['SubmitterName'] ?? ''),
      folderNo: String(r['folderNo'] ?? r['FolderNo'] ?? ''),
      productName: String(r['productName'] ?? r['ProductName'] ?? ''),
      businessAuthorityName: String(r['businessAuthorityName'] ?? r['BusinessAuthorityName'] ?? ''),
      approvers,
      suggestedApproverId: suggested != null ? String(suggested) : null,
    };
  }

  private buildSaveInput(isSubmit: boolean): SaveRepairPlanInput {
    const selected = this.selectedQuotation;
    return {
      claimId: this.claimId!,
      workTaskId: this.workTaskId ?? undefined,
      partnerId: selected?.partnerId ?? null,
      claimAmount: this.sectionI1,
      discountAmount: this.sectionI2,
      depreciationAmount: this.sectionI3,
      expenseAmount: this.sectionIII,
      assessmentAmount: this.form.otherCosts.assessmentCost,
      lossPreventionAmount: this.form.otherCosts.lossPrevCost,
      rescueAmount: this.form.otherCosts.rescueCost,
      otherAmount: this.form.otherCosts.otherCost,
      submittedDate: isSubmit ? new Date().toISOString() : null,
      data: JSON.stringify(this.form),
    };
  }

  private doSave(status: 'draft' | 'new'): void {
    if (!this.claimId) return;

    if (status !== 'draft') {
      return;
    }

    if (!this.guardSaveSubmitActions()) return;

    const input = this.buildSaveInput(false);

    this.saving = true;
    this.service.save(input).subscribe({
      next: (saved: RepairPlanSavedDto) => {
        this.saving = false;
        const status = this.normalizeQuotationApprovalStatusString(saved?.status) ?? 'new';
        this.quotationApprovalStatus = status;
        this.quotationApprovalStatusChanged.emit(status);
        this.messageService.add({
          severity: 'success',
          summary: 'Thành công',
          detail: 'Đã lưu phương án sửa chữa.',
        });
      },
      error: () => {
        this.saving = false;
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: 'Lưu thất bại. Vui lòng thử lại.',
        });
      },
    });
  }

  private validateForm(): boolean {
    if (this.form.quotations.length === 0) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Cảnh báo',
        detail: 'Phải có ít nhất 1 báo giá trong danh sách.',
      });
      return false;
    }

    if (!this.form.selectedQuotationId) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Cảnh báo',
        detail: 'Phải chọn 1 báo giá phù hợp trước khi Lưu/Trình duyệt.',
      });
      return false;
    }

    for (const q of this.form.quotations) {
      if (!q.partnerId) {
        this.messageService.add({
          severity: 'warn',
          summary: 'Cảnh báo',
          detail: 'Tên đơn vị báo giá là bắt buộc.',
        });
        return false;
      }
      if (!q.incidentObjectId) {
        this.messageService.add({
          severity: 'warn',
          summary: 'Cảnh báo',
          detail: 'Đối tượng BH là bắt buộc.',
        });
        return false;
      }
      if (!q.quotationDate) {
        this.messageService.add({
          severity: 'warn',
          summary: 'Cảnh báo',
          detail: 'Ngày báo giá là bắt buộc.',
        });
        return false;
      }
      if (q.totalAmount == null || q.totalAmount <= 0) {
        this.messageService.add({
          severity: 'warn',
          summary: 'Cảnh báo',
          detail: 'Tổng tiền báo giá phải lớn hơn 0.',
        });
        return false;
      }
      if (!q.documentId) {
        this.messageService.add({
          severity: 'warn',
          summary: 'Cảnh báo',
          detail: 'File báo giá là bắt buộc.',
        });
        return false;
      }
      if (q.uploading) {
        this.messageService.add({
          severity: 'warn',
          summary: 'Cảnh báo',
          detail: 'File báo giá đang tải lên, vui lòng chờ.',
        });
        return false;
      }
    }

    for (const item of this.form.items) {
      for (const cost of item.costRows) {
        if (cost.garageAmount != null && cost.garageAmount < 0) {
          this.messageService.add({
            severity: 'warn',
            summary: 'Cảnh báo',
            detail: 'Báo giá gara phải là số nguyên dương.',
          });
          return false;
        }
        if (cost.proposedAmount != null && cost.proposedAmount < 0) {
          this.messageService.add({
            severity: 'warn',
            summary: 'Cảnh báo',
            detail: 'GĐV đề xuất phải là số nguyên dương.',
          });
          return false;
        }
      }
    }

    return true;
  }

  private static readonly garageListPageSize = 500;

  private loadGarages(): void {
    const base: GetResPartnersInput = {
      partnerTypeCode: 'GARAGE',
      status: ResPartnerStatus.Active,
      sorting: 'name asc',
      maxResultCount: RepairPlanTabComponent.garageListPageSize,
    };

    const collected: RepairPlanGarageOptionDto[] = [];

    const loadPage = (skipCount: number): void => {
      this.resPartnerService.getList({ ...base, skipCount }).subscribe({
        next: res => {
          const batch = (res.items || []).map(p => ({
            id: p.id ?? '',
            name: p.name ?? '',
          }));
          collected.push(...batch);

          const totalCount = res.totalCount;
          const pageFull = batch.length === RepairPlanTabComponent.garageListPageSize;
          const underTotal =
            typeof totalCount === 'number' ? collected.length < totalCount : pageFull;

          if (pageFull && underTotal) {
            loadPage(skipCount + RepairPlanTabComponent.garageListPageSize);
            return;
          }

          this.garageOptions = collected;
          this.garageSearchResults = [...collected];
          this.mergeQuotationPartnerRefsIntoGarageOptions();
        },
        error: () => {
          this.garageOptions = [];
          this.garageSearchResults = [];
          this.messageService.add({
            severity: 'error',
            summary: 'Lỗi',
            detail: 'Không tải được danh sách garage.',
          });
        },
      });
    };

    loadPage(0);
  }

  /**
   * Bổ sung vào dropdown các đối tác đã chọn trong báo giá (kể cả Ngưng hoạt động) để hiển thị đúng bản ghi lưu.
   */
  private mergeQuotationPartnerRefsIntoGarageOptions(): void {
    const existing = new Set(this.garageOptions.map(g => g.id));
    const missing = [
      ...new Set(
        this.form.quotations
          .map(q => q.partnerId)
          .filter((id): id is string => !!id && id.length > 0 && !existing.has(id)),
      ),
    ];

    if (missing.length === 0) {
      this.garageSearchResults = [...this.garageOptions];
      return;
    }

    forkJoin(
      missing.map(id =>
        this.resPartnerService.get(id).pipe(catchError(() => of(null as ResPartnerDto | null))),
      ),
    ).subscribe({
      next: partners => {
        const additions: RepairPlanGarageOptionDto[] = [];
        for (const p of partners) {
          if (!p?.id) {
            continue;
          }
          const id = p.id;
          if (this.garageOptions.some(g => g.id === id)) {
            continue;
          }
          const baseName = (p.name ?? '').trim() || id;
          const inactive = p.status === ResPartnerStatus.Deactive;
          const name = inactive ? `${baseName} (Ngưng hoạt động)` : baseName;
          additions.push({ id, name });
        }
        if (additions.length > 0) {
          this.garageOptions = [...this.garageOptions, ...additions].sort((a, b) =>
            (a.name || '').localeCompare(b.name || '', 'vi', { sensitivity: 'base' }),
          );
        }
        this.garageSearchResults = [...this.garageOptions];
      },
      error: () => {
        this.garageSearchResults = [...this.garageOptions];
      },
    });
  }

  private loadQuotationDocType(): void {
    this.resDocumentTypeService.getList({ code: 'QUOTATION', maxResultCount: 5 }).subscribe({
      next: res => {
        const matched = (res.items || []).find(
          x => !!x.id && (x.code || '').toUpperCase() === 'QUOTATION',
        );
        this.quotationDocTypeId = matched?.id ?? null;

        if (!this.quotationDocTypeId) {
          this.resDocumentTypeService.getList({ code: 'OTHER', maxResultCount: 5 }).subscribe({
            next: res2 => {
              const fallback = (res2.items || []).find(
                x => !!x.id && (x.code || '').toUpperCase() === 'OTHER',
              );
              this.quotationDocTypeId = fallback?.id ?? null;
            },
          });
        }
      },
    });
  }
}
