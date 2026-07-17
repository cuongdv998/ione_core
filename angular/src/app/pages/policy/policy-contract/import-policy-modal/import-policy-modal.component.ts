import { Component, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { SelectModule } from 'primeng/select';
import { MessageService } from 'primeng/api';

import { LocalizationService } from '@/core/services/localization.service';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { PolicyContractService } from '@/proxy/policy/controllers/policy-contract.service';
import { PolicyContractStatus } from '@/proxy/policy-contracts/policy-contract-status.enum';
import { ProProductTypeService } from '@/proxy/product/controllers/pro-product-type.service';
import { ProProductTypeStatus } from '@/proxy/pro-product-types/pro-product-type-status.enum';
import { environment } from '@environments/environment';

@Component({
  selector: 'app-import-policy-modal',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    DialogModule,
    ButtonModule,
    ToastModule,
    SelectModule,
    TranslatePipe
  ],
  templateUrl: './import-policy-modal.component.html',
  styleUrl: './import-policy-modal.component.scss',
  providers: [MessageService]
})
export class ImportPolicyModalComponent implements OnInit {
  private http = inject(HttpClient);
  private messageService = inject(MessageService);
  private localizationService = inject(LocalizationService);
  private policyContractService = inject(PolicyContractService);
  private productTypeService = inject(ProProductTypeService);

  @Input() visible = false;
  @Output() visibleChange = new EventEmitter<boolean>();
  @Output() closed = new EventEmitter<void>();
  @Output() imported = new EventEmitter<void>();

  /** true = mở từ màn danh sách đơn (hiển thị chọn hợp đồng + đối tác); false = màn hợp đồng (giữ nguyên, chỉ import theo contractId). */
  @Input() fromPolicyList = false;
  /** Khi mở từ màn danh sách đơn: danh sách hợp đồng ban đầu (nếu có). Modal sẽ load lại theo Đối tác BH khi user chọn đối tác. */
  @Input() contractOptions: Array<{ label: string; value: string }> = [];
  /** Danh sách đối tác bảo hiểm (từ policies hoặc contract detail). */
  @Input() primaryInsurancePartnerOptions: Array<{ label: string; value: string }> = [];

  /** Danh sách hợp đồng hiển thị: khi fromPolicyList thì load theo đối tác (hoặc all nếu không chọn đối tác). */
  displayedContractOptions: Array<{ label: string; value: string }> = [];
  loadingContracts = false;

  contractId: string | null = null;
  /** Màn chi tiết HĐ: mã hợp đồng hiển thị read-only (danh sách đơn dùng dropdown). */
  contractDisplayLabel: string | null = null;
  /** Đối tác bảo hiểm: tự fill khi chọn hợp đồng, khi đó disable select. */
  primaryInsurancePartnerId: string | null = null;
  /** true khi đối tác được lấy từ hợp đồng đã chọn → disable select đối tác. */
  partnerFromContract = false;
  selectedFile: File | null = null;
  selectedFileName = '';
  importing = false;
  /** Sau khi import thành công (full hoặc partial) thì disable nút Import. Reset khi đóng/mở lại modal. */
  importSuccess = false;
  importResult: ImportPolicyResult | null = null;

  /** Loại sản phẩm (pro_product_type) — lọc cột SP trên file mẫu và khi import. */
  productTypeId: string | null = null;
  productTypeOptions: Array<{ label: string; value: string }> = [];
  loadingProductTypes = false;

  ngOnInit(): void {
    this.loadProductTypes();
  }

  private loadProductTypes(): void {
    this.loadingProductTypes = true;
    this.productTypeService
      .getList({
        skipCount: 0,
        maxResultCount: 1000,
        sorting: 'code asc',
        status: ProProductTypeStatus.Active
      })
      .subscribe({
        next: (result) => {
          this.loadingProductTypes = false;
          const items = result.items || [];
          this.productTypeOptions = items.map((item) => {
            const code = (item.code ?? '').trim();
            const name = (item.name ?? '').trim();
            const label = code && name ? `${code} — ${name}` : code || name || item.id || '';
            return { label, value: item.id ?? '' };
          });
        },
        error: () => {
          this.loadingProductTypes = false;
          this.productTypeOptions = [];
        }
      });
  }

  /**
   * Open modal. Khi gọi từ contract detail: open(contractId). Khi gọi từ màn tìm kiếm đơn: open() và truyền contractOptions để hiển thị dropdown chọn hợp đồng.
   */
  open(contractId?: string | null): void {
    this.contractId = contractId ?? null;
    this.visible = true;
    this.visibleChange.emit(true);
    this.reset();
    this.loadProductTypes();
    if (this.contractId) {
      this.loadContractAndSetPartner(this.contractId);
    }
    if (this.fromPolicyList) {
      this.loadContractOptionsByPartner(this.primaryInsurancePartnerId);
    }
  }

  close(): void {
    this.visible = false;
    this.visibleChange.emit(false);
    this.closed.emit();
    this.reset();
  }

  getSelectedContractLabel(): string {
    const options = this.fromPolicyList ? this.displayedContractOptions : this.contractOptions;
    if (!this.contractId || !options?.length) return this.contractId ?? '';
    const o = options.find(c => c.value === this.contractId);
    return o?.label ?? this.contractId ?? '';
  }

  /** Cho phép tải template / import. Màn hợp đồng: luôn true. Màn danh sách đơn: cần hợp đồng HOẶC đối tác bảo hiểm. */
  canProceed(): boolean {
    if (!this.fromPolicyList) return true; // màn hợp đồng: giữ nguyên, luôn cho phép
    if (this.contractId) return true;
    if (this.primaryInsurancePartnerOptions?.length) return !!this.primaryInsurancePartnerId;
    return true;
  }

  /** Khi đổi Đối tác BH: load lại danh sách hợp đồng chỉ lấy hợp đồng thuộc đối tác đó; không chọn đối tác thì load all. */
  onPrimaryPartnerChange(partnerId: string | null): void {
    this.contractId = null;
    this.partnerFromContract = false;
    this.loadContractOptionsByPartner(partnerId);
  }

  /** Khi đổi hợp đồng: load contract để fill đối tác bảo hiểm và disable select đối tác. */
  onContractIdChange(contractId: string | null): void {
    if (!contractId) {
      this.primaryInsurancePartnerId = null;
      this.partnerFromContract = false;
      return;
    }
    this.loadContractAndSetPartner(contractId);
  }

  /** Load danh sách hợp đồng: theo insurerId nếu có, không thì load all. Loại trừ Expired, Terminated, Cancelled. */
  private loadContractOptionsByPartner(insurerId: string | null): void {
    if (!this.fromPolicyList) return;
    this.loadingContracts = true;
    const params: { maxResultCount: number; skipCount: number; sorting: string; insurerId?: string } = {
      maxResultCount: 1000,
      skipCount: 0,
      sorting: 'creationTime desc'
    };
    if (insurerId) params.insurerId = insurerId;

    this.policyContractService.getList(params).subscribe({
      next: (result) => {
        this.loadingContracts = false;
        const items = result.items || [];
        const excluded = new Set<PolicyContractStatus>([
          PolicyContractStatus.Expired,
          PolicyContractStatus.Terminated,
          PolicyContractStatus.Cancelled
        ]);
        const filtered = items.filter((c: { status?: unknown; id?: string }) => {
          const status = c?.status as PolicyContractStatus | undefined;
          const id = (c?.id ?? '').toString().trim();
          if (this.contractId && id === this.contractId) return true;
          return status === undefined || !excluded.has(status);
        });
        this.displayedContractOptions = filtered.map((c: { code?: string; id?: string }) => ({
          label: c.code || '',
          value: c.id || ''
        }));
      },
      error: () => {
        this.loadingContracts = false;
        this.displayedContractOptions = [];
      }
    });
  }

  private loadContractAndSetPartner(contractId: string): void {
    this.policyContractService.get(contractId).subscribe({
      next: (contract) => {
        this.primaryInsurancePartnerId = contract.insurerId ?? null;
        this.partnerFromContract = true;
        this.contractDisplayLabel = (contract.code ?? '').trim() || null;
      },
      error: () => {
        this.primaryInsurancePartnerId = null;
        this.partnerFromContract = false;
        this.contractDisplayLabel = null;
      }
    });
  }

  getSelectedPartnerLabel(): string {
    if (!this.primaryInsurancePartnerId || !this.primaryInsurancePartnerOptions?.length) return this.primaryInsurancePartnerId ?? '';
    const o = this.primaryInsurancePartnerOptions.find(p => p.value === this.primaryInsurancePartnerId);
    return o?.label ?? this.primaryInsurancePartnerId ?? '';
  }

  private reset(): void {
    this.selectedFile = null;
    this.selectedFileName = '';
    this.importing = false;
    this.importSuccess = false;
    this.importResult = null;
    this.primaryInsurancePartnerId = null;
    this.partnerFromContract = false;
    this.displayedContractOptions = [];
    this.contractDisplayLabel = null;
    this.productTypeId = null;
  }

  onClose(): void {
    this.close();
  }

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
        detail: this.localizationService.localize('Policy::Policy:ImportPolicy:InvalidFileType')
      });
      input.value = '';
      return;
    }

    this.selectedFile = file;
    this.selectedFileName = file.name;
    this.importResult = null;
    input.value = '';
  }

  downloadTemplate(): void {
    const apis = (environment as { apis?: Record<string, { url?: string }> }).apis;
    const apiUrl = (apis?.['Policy']?.url || apis?.['default']?.url || 'https://localhost:44360').replace(/\/$/, '');
    const params = new URLSearchParams();
    if (this.contractId) params.set('contractId', this.contractId);
    if (this.primaryInsurancePartnerId) params.set('insurerId', this.primaryInsurancePartnerId);
    if (this.productTypeId) params.set('productTypeId', this.productTypeId);
    const query = params.toString();
    const url = `${apiUrl}/api/policy/policies/import-template${query ? `?${query}` : ''}`;

    this.importing = true;

    this.http.get(url, { responseType: 'blob' }).subscribe({
      next: (blob: Blob) => {
        this.importing = false;
        const link = document.createElement('a');
        link.href = window.URL.createObjectURL(blob);
        link.download = `Policy_Import_Template_${new Date().toISOString().slice(0, 10)}.xlsx`;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        window.URL.revokeObjectURL(link.href);
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Policy::Success'),
          detail: this.localizationService.localize('Policy::Policy:ImportPolicy:TemplateDownloaded')
        });
      },
      error: (err) => {
        this.importing = false;
        const detail = this.getApiErrorMessage(err);
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Policy::Error'),
          detail
        });
      }
    });
  }

  /**
   * Lấy thông báo lỗi cụ thể từ response (BusinessException/UserFriendlyException).
   * Tránh chỉ hiển thị "Exception of type 'Volo.Abp.BusinessException' was thrown."
   */
  private getApiErrorMessage(err: { error?: unknown; message?: string }): string {
    let e: { code?: string; message?: string; details?: string } | undefined;
    if (err?.error && typeof err.error === 'object' && !(err.error instanceof Blob)) {
      e = (err.error as { error?: { code?: string; message?: string; details?: string } }).error;
    }
    const msg = (e?.message ?? '').trim();
    const details = (e?.details ?? '').trim();
    const code = (e?.code ?? '').trim();
    const genericPattern = /Exception of type\s+['\"][^'\"]+['\"]\s+was thrown\.?/i;
    if (msg && !genericPattern.test(msg)) {
      return details ? `${msg} ${details}` : msg;
    }
    const localizedCode = code ? this.localizationService.localize(code) : '';
    if (localizedCode && localizedCode !== code) return details ? `${localizedCode} ${details}` : localizedCode;
    if (details) return details;
    if (code) return code;
    if (msg) return msg;
    return this.localizationService.localize('Policy::InternalServerErrorMessage');
  }

  getResultSummary(): string {
    if (!this.importResult) return '';
    const template = this.localizationService.localize('Policy::Policy:ImportPolicy:ResultSummary');
    return template
      .replace('{0}', String(this.importResult.totalRows))
      .replace('{1}', String(this.importResult.successCount))
      .replace('{2}', String(this.importResult.errorCount));
  }

  /** STT/dòng Excel — API có thể trả rowNumber (camelCase) hoặc RowNumber (PascalCase). */
  getErrorRowNumber(err: ImportPolicyResult['errors'][0] & { RowNumber?: number }): number {
    const n = err.rowNumber ?? err.RowNumber;
    return typeof n === 'number' && !Number.isNaN(n) ? n : 0;
  }

  onExecute(): void {
    if (!this.selectedFile) {
      this.messageService.add({
        severity: 'error',
        summary: this.localizationService.localize('Policy::Error'),
        detail: this.localizationService.localize('Policy::Policy:ImportPolicy:SelectFile')
      });
      return;
    }

    this.importing = true;
    this.importResult = null;

    const formData = new FormData();
    formData.append('file', this.selectedFile);

    const apis = (environment as { apis?: Record<string, { url?: string }> }).apis;
    const apiUrl = (apis?.['Policy']?.url || apis?.['default']?.url || 'https://localhost:44360').replace(/\/$/, '');
    const params = new URLSearchParams();
    if (this.contractId) params.set('contractId', this.contractId);
    if (this.primaryInsurancePartnerId) params.set('insurerId', this.primaryInsurancePartnerId);
    if (this.productTypeId) params.set('productTypeId', this.productTypeId);
    const query = params.toString();
    const url = `${apiUrl}/api/policy/policies/import-excel${query ? `?${query}` : ''}`;

    this.http.post<ImportPolicyResult>(url, formData).subscribe({
      next: (result) => {
        this.importing = false;
        this.importResult = result;

        if (result.errorCount === 0) {
          this.importSuccess = true;
          this.messageService.add({
            severity: 'success',
            summary: this.localizationService.localize('Policy::Success'),
            detail: this.localizationService.localize('Policy::Policy:ImportPolicy:Success')
          });
          this.imported.emit();
        } else if (result.successCount > 0) {
          this.importSuccess = true;
          this.messageService.add({
            severity: 'warn',
            summary: this.localizationService.localize('Policy::Warning'),
            detail: this.localizationService.localize('Policy::Policy:ImportPolicy:PartialSuccess')
          });
          this.imported.emit();
        } else {
          this.messageService.add({
            severity: 'error',
            summary: this.localizationService.localize('Policy::Error'),
            detail: this.localizationService.localize('Policy::Policy:ImportPolicy:Failed')
          });
        }
      },
      error: (err) => {
        this.importing = false;
        const detail = this.getApiErrorMessage(err);
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Policy::Error'),
          detail
        });
      }
    });
  }

  downloadErrorFileIfAny(result: ImportPolicyResult): void {
    if (!result?.errorFileBase64) return;

    try {
      const binary = atob(result.errorFileBase64);
      const bytes = new Uint8Array(binary.length);
      for (let i = 0; i < binary.length; i++) {
        bytes[i] = binary.charCodeAt(i);
      }

      const blob = new Blob([bytes], {
        type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
      });
      const link = document.createElement('a');
      link.href = window.URL.createObjectURL(blob);
      link.download = result.errorFileName || `import_policy_errors_${new Date().toISOString().slice(0, 10)}.xlsx`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.URL.revokeObjectURL(link.href);
    } catch {
      // Keep silent: import summary/errors table is still shown on UI.
    }
  }
}

interface ImportPolicyResult {
  totalRows: number;
  successCount: number;
  errorCount: number;
  errors: Array<{ rowNumber: number; field: string; message: string; value?: string }>;
  /** Batch import lot code shared by every policy created in this run. */
  importLotCode?: string;
  lotImportCodes?: Array<{ rowNumber: number; lotImportCode: string }>;
  errorFileName?: string;
  errorFileBase64?: string;
}
