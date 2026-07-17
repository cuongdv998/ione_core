import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import type { MenuItem } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { MenuModule } from 'primeng/menu';
import { TableModule } from 'primeng/table';
import { TooltipModule } from 'primeng/tooltip';

import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';
import type { ClaimDetailDto, ClaimPolicyDto } from '@/proxy/claim/claims/models';

@Component({
  selector: 'app-claim-task-info-tab',
  standalone: true,
  imports: [CommonModule, ButtonModule, MenuModule, TableModule, TooltipModule, TranslatePipe],
  templateUrl: './claim-task-info-tab.component.html',
  styleUrl: './claim-task-info-tab.component.scss'
})
export class ClaimTaskInfoTabComponent {
  @Input() claim: ClaimDetailDto | null = null;
  @Input() policies: ClaimPolicyDto[] = [];
  @Input() policiesLoading = false;
  /** Khi true, hiển thị hàng Ước tổn thất (ẩn trên GĐCT — onsite detail-assessment-list). */
  @Input() showTotalEstimatedLoss = false;
  @Input() totalEstimatedLossAmount: number | null = null;

  @Output() refreshPolicies = new EventEmitter<void>();

  constructor(private localizationService: LocalizationService) {}

  onRefreshPolicies(): void {
    this.refreshPolicies.emit();
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

    const day = String(d.getDate()).padStart(2, '0');
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const year = d.getFullYear();
    const hour = String(d.getHours()).padStart(2, '0');
    const minute = String(d.getMinutes()).padStart(2, '0');

    return `${day}/${month}/${year} ${hour}:${minute}`;
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

  getIncidentCauseDisplay(): string {
    if (!this.claim) return '-';
    const name = (this.claim as { incidentCauseName?: string })['incidentCauseName'];
    return name || (this.claim.incidentCauseId ?? '-');
  }

  getAssessmentPartnerDisplay(): string {
    if (!this.claim) return '-';
    const name = this.claim.assessmentPartnerName?.trim();
    const id = (this.claim.assessmentPartnerId ?? '').toString().trim();
    // Backend đôi khi trả về chuỗi rỗng ('') => UI phải hiển thị '-'
    return name || id || '-';
  }

  openCertificateUrl(url: string | undefined): void {
    if (url) window.open(url, '_blank');
  }

  formatTotalEstimatedLossDisplay(): string {
    if (!this.showTotalEstimatedLoss || this.totalEstimatedLossAmount == null) {
      return '-';
    }
    return new Intl.NumberFormat('vi-VN').format(this.totalEstimatedLossAmount);
  }

  getPolicyMenuItems(policy: ClaimPolicyDto): MenuItem[] {
    return [
      {
        label: this.localizationService.localize('Claim::Action') || 'Action',
        icon: 'pi pi-file-pdf',
        command: () => this.openCertificateUrl(policy.certificateUrl),
        disabled: !policy.certificateUrl
      }
    ];
  }

  formatPaymentStatus(status: string | undefined | null): string {
    if (!status) return '-';
    const s = (status || '').toLowerCase();
    if (s === 'new') {
      return this.localizationService.localize('Policy::Policy:PaymentStatusNew', 'Chưa thanh toán');
    }
    if (s === 'inprogress' || s === 'partial') {
      return this.localizationService.localize(
        'Policy::Policy:PaymentStatusInProgress',
        'Thanh toán 1 phần'
      );
    }
    if (s === 'done' || s === 'paid') {
      return this.localizationService.localize(
        'Policy::Policy:PaymentStatusDone',
        'Đã thanh toán toàn bộ'
      );
    }
    if (s === 'cancelled') {
      return this.localizationService.localize('Policy::Policy:PaymentStatusCancelled', 'Đã hủy');
    }
    return status;
  }
}
