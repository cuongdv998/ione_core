import { Component, EventEmitter, Input, Output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { SelectModule } from 'primeng/select';
import { DatePickerModule } from 'primeng/datepicker';
import { InputNumberModule } from 'primeng/inputnumber';

import { LocalizationService } from '@/core/services/localization.service';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { environment } from '@environments/environment';

@Component({
  selector: 'app-payment-update-modal',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    DialogModule,
    ButtonModule,
    ToastModule,
    SelectModule,
    DatePickerModule,
    InputNumberModule,
    TranslatePipe
  ],
  templateUrl: './payment-update-modal.component.html',
  styleUrl: './payment-update-modal.component.scss',
  providers: [MessageService]
})
export class PaymentUpdateModalComponent {
  private http = inject(HttpClient);
  private messageService = inject(MessageService);
  private localizationService = inject(LocalizationService);

  @Input() visible = false;
  /** Cùng LOB ngầm với tìm kiếm danh sách đơn (DEFAULT_LOB / DEFAULT_LOB_MOTORBIKE); gửi kèm API tải mẫu. */
  @Input() exportTemplateLobId: string | null = null;
  /** true: tính số tiền / FIFO gồm phiên bản đơn Active + Draft (API payment config + create + export template). */
  @Input() isPaymentOnline = false;
  @Output() visibleChange = new EventEmitter<boolean>();
  @Output() closed = new EventEmitter<void>();
  @Output() submitted = new EventEmitter<void>();

  /**
   * Mode:
   * - 'single' : cập nhật thanh toán cho 1 đơn (popup theo wireframe)
   * - 'excel'  : import Excel hàng loạt (giữ lại tính năng cũ)
   */
  mode: 'single' | 'excel' = 'single';

  // --- Single payment mode state ---
  policyId: string | null = null;
  loadingConfig = false;
  submitting = false;

  paymentMethods: PaymentMethodSelectDto[] = [];
  paymentTypes: PaymentTypeSelectDto[] = [];
  amountToPay = 0;

  formPaymentMethodId: string | null = null;
  formPaymentDate: Date | null = null;
  formAmount: number | null = null;

  // --- Excel import mode state (giữ nguyên để dùng cho cập nhật hàng loạt) ---
  selectedFile: File | null = null;
  selectedFileName = '';
  importing = false;

  /**
   * Open modal
   * @param policyId - Id đơn bảo hiểm (dùng cho mode 'single')
   * @param mode - 'single' (mặc định) hoặc 'excel'
   */
  open(policyId?: string, mode: 'single' | 'excel' = 'single'): void {
    this.mode = mode;
    this.visible = true;
    this.visibleChange.emit(true);

    if (this.mode === 'single') {
      this.resetSingleForm();
      this.policyId = (policyId || '').trim() || null;
      if (this.policyId) {
        this.loadPaymentConfig(this.policyId);
      }
    } else {
      this.resetExcelForm();
    }
  }

  /**
   * Close modal
   */
  close(): void {
    this.visible = false;
    this.visibleChange.emit(false);
    this.closed.emit();

    // Dọn state theo mode hiện tại
    if (this.mode === 'single') {
      this.resetSingleForm();
    } else {
      this.resetExcelForm();
    }
  }

  private resetSingleForm(): void {
    this.policyId = null;
    this.paymentMethods = [];
    this.paymentTypes = [];
    this.amountToPay = 0;
    this.formPaymentMethodId = null;
    this.formPaymentDate = new Date();
    this.formAmount = null;
    this.loadingConfig = false;
    this.submitting = false;
  }

  private resetExcelForm(): void {
    this.selectedFile = null;
    this.selectedFileName = '';
    this.importing = false;
  }

  onVisibleChange(value: boolean): void {
    if (!value) {
      this.close();
    }
  }

  onClose(): void {
    this.close();
  }

  // ---------------- Single payment mode ----------------

  private loadPaymentConfig(policyId: string): void {
    this.loadingConfig = true;

    const apis = (environment as { apis?: Record<string, { url?: string }> }).apis;
    const apiUrl = (apis?.['Policy']?.url || apis?.['default']?.url || 'https://localhost:44360').replace(/\/$/, '');
    const online = this.isPaymentOnline ? 'true' : 'false';
    const url = `${apiUrl}/api/payment/config?policyId=${encodeURIComponent(policyId)}&isPaymentOnline=${online}`;

    this.http.get<PaymentConfigDto>(url).subscribe({
      next: (config) => {
        this.loadingConfig = false;
        this.paymentMethods = config?.paymentMethods || [];
        this.paymentTypes = config?.paymentTypes || [];
        this.amountToPay = config?.amountToPay ?? 0;

        // Gán mặc định (bao gồm 0 - khi amountToPay = 0 vẫn hiển thị trong input)
        this.formAmount = this.amountToPay;
        this.formPaymentMethodId = this.paymentMethods.length ? String(this.paymentMethods[0].id) : null;
        if (!this.formPaymentDate) {
          this.formPaymentDate = new Date();
        }
      },
      error: (err) => {
        this.loadingConfig = false;
        const detail = err.error?.error?.message ||
          err.error?.error?.details ||
          this.localizationService.localize('Policy::InternalServerErrorMessage');
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Policy::Error'),
          detail
        });
      }
    });
  }

  private formatDateOnly(date: Date): string {
    const d = new Date(date);
    const year = d.getFullYear();
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  canSubmitSingle(): boolean {
    return this.mode === 'single'
      && !!this.policyId
      && !!this.formPaymentMethodId
      && !!this.formPaymentDate
      && !!this.formAmount
      && !this.loadingConfig
      && !this.submitting;
  }

  /**
   * Thực hiện tạo đề nghị thanh toán cho 1 đơn (CreatePaymentRequestAsync)
   */
  onSubmitSingle(): void {
    if (!this.canSubmitSingle()) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Policy::Error'),
        detail: this.localizationService.localize('Policy::Payment:FillRequiredFields')
      });
      return;
    }

    const policyId = this.policyId!;
    const body: CreatePaymentRequestInput = {
      paymentMethodId: this.formPaymentMethodId!,
      amount: this.formAmount!,
      paymentDate: this.formatDateOnly(this.formPaymentDate!),
      isPaymentOnline: this.isPaymentOnline
    };

    this.submitting = true;

    const apis = (environment as { apis?: Record<string, { url?: string }> }).apis;
    const apiUrl = (apis?.['Policy']?.url || apis?.['default']?.url || 'https://localhost:44360').replace(/\/$/, '');
    const url = `${apiUrl}/api/payment/create?policyId=${encodeURIComponent(policyId)}`;

    this.http.post<AccountPaymentRequestDto>(url, body).subscribe({
      next: () => {
        this.submitting = false;
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Policy::Success'),
          detail: this.localizationService.localize('Policy::Payment:UpdatePaymentSuccess')
        });
        this.submitted.emit();
        this.close();
      },
      error: (err) => {
        this.submitting = false;
        const detail = err.error?.error?.message ||
          err.error?.error?.details ||
          this.localizationService.localize('Policy::InternalServerErrorMessage');
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Policy::Error'),
          detail
        });
      }
    });
  }

  // ---------------- Excel import mode (giữ nguyên) ----------------

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    const validExtensions = ['.xlsx', '.xls'];
    const ext = file.name.substring(file.name.lastIndexOf('.')).toLowerCase();
    if (!validExtensions.includes(ext)) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Policy::Error'),
        detail: this.localizationService.localize('Policy::Payment:InvalidFileType')
      });
      input.value = '';
      return;
    }

    this.selectedFile = file;
    this.selectedFileName = file.name;
    input.value = '';
  }

  /**
   * Download Excel template với dữ liệu các đơn bảo hiểm còn nợ thanh toán.
   * User điền: Số tiền thanh toán, Hình thức thanh toán, Ngày thanh toán
   */
  downloadTemplate(): void {
    const apis = (environment as { apis?: Record<string, { url?: string }> }).apis;
    const apiUrl = (apis?.['Policy']?.url || apis?.['default']?.url || 'https://localhost:44360').replace(/\/$/, '');
    const lob = (this.exportTemplateLobId || '').trim();
    const online = this.isPaymentOnline ? 'true' : 'false';
    const url =
      `${apiUrl}/api/payment/export-template` +
      (lob ? `?lobId=${encodeURIComponent(lob)}&isPaymentOnline=${online}` : `?isPaymentOnline=${online}`);

    this.importing = true;

    this.http.get(url, { responseType: 'blob' }).subscribe({
      next: (blob: Blob) => {
        this.importing = false;
        const link = document.createElement('a');
        link.href = window.URL.createObjectURL(blob);
        link.download = `Payment_Update_Template_${new Date().toISOString().slice(0, 10)}.xlsx`;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        window.URL.revokeObjectURL(link.href);
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Policy::Success'),
          detail: this.localizationService.localize('Policy::Payment:TemplateDownloaded')
        });
      },
      error: (err) => {
        this.importing = false;
        const detail = err.error?.error?.message || err.error?.error?.details ||
          this.localizationService.localize('Policy::InternalServerErrorMessage');
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Policy::Error'),
          detail
        });
      }
    });
  }

  /**
   * Execute import - upload file only when user clicks Thực hiện
   */
  onExecute(): void {
    if (!this.selectedFile) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Policy::Error'),
        detail: this.localizationService.localize('Policy::Payment:SelectFileFirst')
      });
      return;
    }

    this.importing = true;
    const formData = new FormData();
    formData.append('file', this.selectedFile);

    const apis = (environment as { apis?: Record<string, { url?: string }> }).apis;
    const apiUrl = (apis?.['Policy']?.url || apis?.['default']?.url || 'https://localhost:44360').replace(/\/$/, '');
    const url = `${apiUrl}/api/payment/import-excel`;

    this.http.post<ImportPaymentResult>(url, formData).subscribe({
      next: (result) => {
        this.importing = false;
        const successMsg = this.localizationService.localize('Policy::Payment:ImportSuccess');
        const detail = `${successMsg} - ${result.successCount || 0} ${this.localizationService.localize('Policy::Payment:RecordsSuccess')}`;
        this.messageService.add({
          severity: result.errorCount && result.errorCount > 0 ? 'warn' : 'success',
          summary: this.localizationService.localize('Policy::Success'),
          detail
        });
        this.submitted.emit();
        this.close();
      },
      error: (err) => {
        this.importing = false;
        const detail = err.error?.error?.message || err.error?.error?.details ||
          this.localizationService.localize('Policy::InternalServerErrorMessage');
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Policy::Error'),
          detail
        });
      }
    });
  }
}

interface ImportPaymentResult {
  totalRows?: number;
  successCount?: number;
  errorCount?: number;
  errors?: Array<{ rowNumber: number; field: string; message: string; value?: string }>;
}

interface PaymentMethodSelectDto {
  id: string;
  code: string;
  name: string;
}

interface PaymentTypeSelectDto {
  id: string;
  code: string;
  name: string;
}

interface PaymentConfigDto {
  paymentMethods: PaymentMethodSelectDto[];
  paymentTypes: PaymentTypeSelectDto[];
  amountToPay: number;
}

interface CreatePaymentRequestInput {
  /** Optional nếu gửi paymentMethodCode (server ưu tiên code) */
  paymentMethodId?: string | null;
  /** Optional; ưu tiên hơn paymentMethodId — khớp res_payment_method.code (UPPER trên DB) */
  paymentMethodCode?: string | null;
  amount: number;
  /** YYYY-MM-DD string to avoid timezone shift (Date serializes to UTC) */
  paymentDate: string;
  /** Optional; nếu có cùng paymentTypeCode thì server ưu tiên paymentTypeCode */
  paymentTypeId?: string | null;
  /** Optional; ưu tiên hơn paymentTypeId — khớp res_payment_type.code (chuẩn hóa UPPER trên server) */
  paymentTypeCode?: string | null;
  /** Optional; mã tham chiếu giao dịch (account_payment_request.trans_ref) */
  transRef?: string | null;
  /** true: tổng phải thu + FIFO gồm phiên bản Active và Draft */
  isPaymentOnline?: boolean;
}

interface AccountPaymentRequestDto {
  id: string;
  policyId?: string;
  customerId?: string;
  paymentMethodId: string;
  paymentTypeId: string;
  currencyId: string;
  amount: number;
  issueDate: string;
  dueDate: string;
  status: string;
  submittedDate?: string;
  submitterId?: string;
  transRef?: string | null;
}
