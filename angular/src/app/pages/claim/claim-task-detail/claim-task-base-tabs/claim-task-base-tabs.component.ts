import { CommonModule } from '@angular/common';
import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { TabsModule } from 'primeng/tabs';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { TableModule } from 'primeng/table';
import { TooltipModule } from 'primeng/tooltip';

import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';
import type { ClaimDetailDto, ClaimDocumentDto, ClaimPolicyDto } from '@/proxy/claim/claims/models';
import { ClaimDocumentService } from '@/proxy/claim/controllers/claim-document.service';
import { PolicyService } from '@/proxy/policy/controllers/policy.service';
import type { PolicyClaimLookupDto } from '@/proxy/policy/policies/models';
import { ProcessClaimType } from '@/proxy/claims/process-claim-type.enum';

@Component({
  selector: 'app-claim-task-base-tabs',
  standalone: true,
  imports: [
    CommonModule,
    TabsModule,
    ButtonModule,
    InputTextModule,
    TextareaModule,
    TableModule,
    TooltipModule,
    TranslatePipe
  ],
  templateUrl: './claim-task-base-tabs.component.html',
  styleUrl: './claim-task-base-tabs.component.scss'
})
export class ClaimTaskBaseTabsComponent implements OnChanges {
  @Input() claimId: string | null = null;
  @Input() claim: ClaimDetailDto | null = null;
  @Input() mode: 'both' | 'general' | 'images' = 'both';

  activeTab = 0;
  documents: ClaimDocumentDto[] = [];
  loadingDocuments = false;
  docPageSize = 50;

  policies: ClaimPolicyDto[] = [];
  policiesLoading = false;

  constructor(
    private claimDocumentService: ClaimDocumentService,
    private policyService: PolicyService,
    private localizationService: LocalizationService
  ) {}

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['claim']) {
      this.loadPoliciesFromClaim();
    }
    if (changes['claimId'] && this.activeTab === 1) {
      this.loadDocuments();
    }
    if (changes['mode']) {
      this.activeTab = this.mode === 'images' ? 1 : 0;
      if (this.mode === 'images' && this.claimId) {
        this.loadDocuments();
      }
    }
  }

  onTabChange(index: string | number | undefined): void {
    if (index === undefined || index === null) return;
    const tabIndex = typeof index === 'number' ? index : Number(index);
    if (Number.isNaN(tabIndex)) return;
    this.activeTab = tabIndex;
    if (tabIndex === 1 && this.claimId) {
      this.loadDocuments();
    }
  }

  formatDate(value: string | undefined): string {
    if (!value) return '-';
    const d = new Date(value);
    return isNaN(d.getTime()) ? String(value) : d.toLocaleDateString('vi-VN');
  }

  formatDateTime(value: string | Date | undefined): string {
    if (!value) return '-';
    const d = new Date(value);
    if (isNaN(d.getTime())) return String(value);
    const date = d.toLocaleDateString('vi-VN');
    const h = String(d.getHours()).padStart(2, '0');
    const mi = String(d.getMinutes()).padStart(2, '0');
    return `${h}h${mi}, ${date}`;
  }

  getPriorityLabel(priority: number | undefined | null): string {
    if (priority == null) return '-';
    if (priority >= 3) return this.localizationService.localize('Claim::PriorityHigh') || 'Cao';
    if (priority >= 2) return this.localizationService.localize('Claim::PriorityMedium') || 'Trung bình';
    return this.localizationService.localize('Claim::PriorityLow') || 'Thấp';
  }

  getPriorityClass(priority: number | undefined | null): string {
    if (priority == null) return '';
    if (priority >= 3) return 'text-red-600 font-medium';
    if (priority >= 2) return 'text-orange-600 font-medium';
    return 'text-green-600 font-medium';
  }

  getVehicleDisplay(): string {
    if (!this.claim) return '-';
    const parts = [this.claim.carPlate || '-', this.claim.vin || '-', this.claim.engineNumber || '-'];
    return parts.join(' / ');
  }

  getProcessClaimTypeDisplay(): string {
    if (this.claim?.processClaimType == null) return '-';
    const key =
      this.claim.processClaimType === ProcessClaimType.Own
        ? 'Claim::ProcessClaimTypeOwn'
        : 'Claim::ProcessClaimTypeInsurer';
    return this.localizationService.localize(key) || String(this.claim.processClaimType);
  }

  getOnLocationDisplay(): string {
    if (!this.claim?.onLocation) return '-';
    return this.claim.onLocation === 'Y'
      ? (this.localizationService.localize('Claim::Yes') || 'Có')
      : this.claim.onLocation === 'N'
        ? (this.localizationService.localize('Claim::No') || 'Không')
        : '-';
  }

  getDriverSexDisplay(): string {
    if (!this.claim?.driverSex) return '-';
    return this.claim.driverSex === 'M'
      ? (this.localizationService.localize('Claim::Male') || 'Nam')
      : this.claim.driverSex === 'F'
        ? (this.localizationService.localize('Claim::Female') || 'Nữ')
        : '-';
  }

  getOpenEmployeeDisplay(): string {
    if (!this.claim) return '-';
    const c = (this.claim as unknown) as Record<string, unknown>;
    const name = c['openEmployeeName'] as string | undefined;
    const phone = c['openEmployeePhone'] as string | undefined;
    const id = c['openEmployeeId'] as string | undefined;
    if (name || id) return [name || id, phone].filter(Boolean).join(' ');
    return '-';
  }

  getProcessEmpDisplay(): string {
    if (!this.claim) return '-';
    const parts = [this.claim.processEmpName, this.claim.processDeptName].filter(Boolean);
    const phone = this.claim.processEmpPhone;
    if (phone) parts.push(phone);
    return parts.length ? parts.join(' - ') : '-';
  }

  getAssessmentPartnerDisplay(): string {
    if (!this.claim) return '-';
    const name = (this.claim as { assessmentPartnerName?: string })['assessmentPartnerName']?.trim();
    const id = (this.claim.assessmentPartnerId ?? '').toString().trim();
    // Backend đôi khi trả chuỗi rỗng ('') => UI phải hiển thị '-' thay vì để trống.
    return name || id || '-';
  }

  getIncidentCauseDisplay(): string {
    if (!this.claim) return '-';
    const name = (this.claim as { incidentCauseName?: string })['incidentCauseName'];
    return name || (this.claim.incidentCauseId ?? '-');
  }

  isImage(mime: string | undefined): boolean {
    if (!mime) return false;
    return mime.startsWith('image/');
  }

  openDocUrl(url: string | undefined): void {
    if (url) window.open(url, '_blank');
  }

  openCertificateUrl(url: string | undefined): void {
    if (url) window.open(url, '_blank');
  }

  refreshPolicies(): void {
    this.loadPoliciesFromClaim();
  }

  private loadDocuments(): void {
    if (!this.claimId) return;
    this.loadingDocuments = true;
    this.claimDocumentService
      .getList({
        claimId: this.claimId,
        skipCount: 0,
        maxResultCount: this.docPageSize
      })
      .subscribe({
        next: (res) => {
          this.documents = res.items ?? [];
          this.loadingDocuments = false;
        },
        error: () => {
          this.loadingDocuments = false;
        }
      });
  }

  private loadPoliciesFromClaim(): void {
    if (!this.claim) return;
    const certificateNo = this.claim.certificateNo?.trim() || undefined;
    const carPlate = this.claim.carPlate?.trim() || undefined;
    const vin = this.claim.vin?.trim() || undefined;
    const engineNumber = this.claim.engineNumber?.trim() || undefined;
    const incidentDate = this.claim.incidentDate || undefined;
    const hasAny = !!certificateNo || !!carPlate || !!vin || !!engineNumber;
    if (!hasAny) {
      this.policies = [];
      return;
    }
    this.policiesLoading = true;
    this.policyService
      .getClaimLookup({
        certificateNo,
        carPlate,
        vin,
        engineNumber,
        incidentDate,
        maxResultCount: 20
      })
      .subscribe({
        next: (items) => {
          this.policies = (items || []).map((x) => this.mapPolicyLookupToClaimPolicy(x));
          this.policiesLoading = false;
        },
        error: () => {
          this.policiesLoading = false;
        }
      });
  }

  private mapPolicyLookupToClaimPolicy(input: PolicyClaimLookupDto): ClaimPolicyDto {
    return {
      policyId: input.policyId,
      lobName: input.lobName,
      contractId: input.contractId,
      certificateNo: input.certificateNo,
      products: input.products || [],
      productId: input.productId,
      carPlate: input.carPlate,
      ownerName: input.ownerName,
      effectDate: input.effectDate,
      expireDate: input.expireDate,
      status: input.status,
      certificateUrl: input.certificateUrl,
      insurerId: input.insurerId,
      insurerName: input.insurerName
    };
  }

  private formatDateOnly(value: string | Date | undefined): string {
    if (!value) return '';
    const d = new Date(value);
    if (isNaN(d.getTime())) return '';
    const y = d.getFullYear();
    const m = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${y}-${m}-${day}`;
  }
}
