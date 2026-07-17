import { Component, EventEmitter, Input, Output, OnInit, OnChanges, SimpleChanges, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { InputNumberModule } from 'primeng/inputnumber';
import { SelectModule } from 'primeng/select';
import { DatePickerModule } from 'primeng/datepicker';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';

import { PolicyService } from '@/proxy/policy/controllers/policy.service';
import { PolicyContractService } from '@/proxy/policy/controllers/policy-contract.service';
import type { ContractTerminationPolicyDto, TerminateContractInput } from '@/proxy/policy/policy-contracts/models';
import { ResReasonService } from '@/proxy/master/controllers/res-reason.service';
import { LocalizationService } from '@/core/services/localization.service';
import { TranslatePipe } from '@/core/pipes/translate.pipe';

@Component({
  selector: 'app-contract-termination-modal',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    DialogModule,
    ButtonModule,
    TableModule,
    InputNumberModule,
    SelectModule,
    DatePickerModule,
    ToastModule,
    TranslatePipe
  ],
  templateUrl: './contract-termination-modal.component.html',
  styleUrl: './contract-termination-modal.component.scss',
  providers: [MessageService]
})
export class ContractTerminationModalComponent implements OnInit, OnChanges {
  private policyService = inject(PolicyService);
  private policyContractService = inject(PolicyContractService);
  private reasonService = inject(ResReasonService);
  private messageService = inject(MessageService);
  private localizationService = inject(LocalizationService);

  @Input() contractId: string | null = null;
  @Input() visible = false;
  @Output() visibleChange = new EventEmitter<boolean>();
  @Output() submitted = new EventEmitter<void>();

  loading = false;
  loadingPolicies = false;
  policies: ContractTerminationPolicyDto[] = [];
  terminationReasonId: string | null = null;
  terminationReasonDescription: string | null = null;
  terminationDate: Date = new Date();
  reasonOptions: Array<{ label: string; value: string }> = [];

  ngOnInit(): void {
    this.loadReasons();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['visible']?.currentValue === true && this.contractId) {
      this.loadPolicies();
    }
  }

  private loadReasons(): void {
    this.reasonService.getSelectListByGroupCode('TERMINATE_POLICY_REASON').subscribe({
      next: (result) => {
        this.reasonOptions = (result || []).map((r: { id?: string; code?: string; name?: string }) => ({
          label: r.name || r.code || '',
          value: r.id || ''
        }));
      }
    });
  }

  onVisibleChange(value: boolean): void {
    this.visibleChange.emit(value);
    if (value && this.contractId) {
      this.loadPolicies();
    }
  }

  loadPolicies(): void {
    if (!this.contractId) return;
    this.loadingPolicies = true;
    this.policyService.getContractTerminationPolicies(this.contractId).subscribe({
      next: (list) => {
        this.policies = list || [];
        this.loadingPolicies = false;
      },
      error: () => {
        this.loadingPolicies = false;
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Policy::Error'),
          detail: this.localizationService.localize('Policy::Policy:InternalServerErrorMessage')
        });
      }
    });
  }

  formatCurrency(value: number): string {
    if (value == null) return '-';
    return new Intl.NumberFormat('vi-VN', {
      minimumFractionDigits: 0,
      maximumFractionDigits: 0
    }).format(value) + 'đ';
  }

  onActualRefundChange(row: ContractTerminationPolicyDto, value: number | null): void {
    row.actualRefundAmount = value ?? row.refundAmount;
  }

  submit(): void {
    if (!this.contractId || this.policies.length === 0) {
      this.messageService.add({
        severity: 'warn',
        summary: this.localizationService.localize('Policy::Warning'),
        detail: this.localizationService.localize('Policy::PolicyContract:TerminateContractNoPolicies')
      });
      return;
    }
    if (!this.terminationReasonId) {
      this.messageService.add({
        severity: 'warn',
        summary: this.localizationService.localize('Policy::Warning'),
        detail: this.localizationService.localize('Policy::Policy:TerminationReasonRequired')
      });
      return;
    }

    const input: TerminateContractInput = {
      contractId: this.contractId,
      terminationReasonId: this.terminationReasonId || undefined,
      terminationReasonDescription: this.terminationReasonDescription || undefined,
      // Date-only: keep LOCAL day (avoid UTC shift)
      terminationDate: this.toLocalIsoDate(this.terminationDate),
      policies: this.policies.map((p) => ({
        policyId: p.policyId,
        actualRefundAmount: p.actualRefundAmount
      }))
    };

    this.loading = true;
    this.policyContractService.terminateContract(input).subscribe({
      next: () => {
        this.loading = false;
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('Policy::Success'),
          detail: this.localizationService.localize('Policy::PolicyContract:TerminateContractSuccess', 'Chấm dứt hợp đồng thành công')
        });
        this.visibleChange.emit(false);
        this.submitted.emit();
      },
      error: (err) => {
        this.loading = false;
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('Policy::Error'),
          detail: err?.error?.error?.message || this.localizationService.localize('Policy::Policy:InternalServerErrorMessage')
        });
      }
    });
  }

  cancel(): void {
    this.visibleChange.emit(false);
  }

  private toLocalIsoDate(date: Date): string {
    const pad2 = (n: number) => String(n).padStart(2, '0');
    const y = date.getFullYear();
    const m = pad2(date.getMonth() + 1);
    const d = pad2(date.getDate());
    return `${y}-${m}-${d}`;
  }
}
