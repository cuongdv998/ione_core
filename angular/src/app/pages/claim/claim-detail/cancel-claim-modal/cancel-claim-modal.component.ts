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
import { ClaimService } from '@/proxy/claim/controllers/claim.service';
import type { CancelClaimInput } from '@/proxy/claim/claims/models';
import { ResReasonService } from '@/proxy/master/controllers/res-reason.service';

@Component({
  selector: 'app-cancel-claim-modal',
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
  templateUrl: './cancel-claim-modal.component.html',
  styleUrl: './cancel-claim-modal.component.scss',
  providers: [MessageService]
})
export class CancelClaimModalComponent implements OnChanges {
  @Input() visible = false;
  @Output() visibleChange = new EventEmitter<boolean>();
  @Input() claimId: string = '';
  @Output() cancelled = new EventEmitter<void>();

  reasonId: string | null = null;
  reasonDescription = '';
  saving = false;
  reasonOptions: Array<{ label: string; value: string }> = [];
  loadingReasons = false;

  private readonly reasonGroupCode = 'CLAIM_CANCEL_REASON';

  constructor(
    private claimService: ClaimService,
    private resReasonService: ResReasonService,
    private messageService: MessageService,
    private localizationService: LocalizationService
  ) {}

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['visible'] && this.visible) {
      this.reasonId = null;
      this.reasonDescription = '';
      this.loadReasons();
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

  onVisibleChange(value: boolean): void {
    this.visibleChange.emit(value);
  }

  focusExecuteButton(): void {
    setTimeout(() => this.getExecuteButton()?.focus(), 100);
  }

  private getExecuteButton(): HTMLButtonElement | null {
    return document.querySelector<HTMLButtonElement>('.p-dialog .cancel-claim-execute-button');
  }

  close(): void {
    this.visible = false;
    this.visibleChange.emit(false);
  }

  submit(): void {
    if (!this.claimId || !this.reasonId || !this.reasonDescription?.trim()) {
      this.messageService.add({
        severity: 'warn',
        summary: this.localizationService.localize('AbpUi::ValidationError'),
        detail: this.localizationService.localize('Claim::RejectReasonRequired')
      });
      return;
    }
    const input: CancelClaimInput = {
      reasonId: this.reasonId,
      reasonDescription: this.reasonDescription.trim()
    };
    this.saving = true;
    this.claimService.cancel(this.claimId, input).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('AbpUi::Success'),
          detail: this.localizationService.localize('Claim::CancelSuccess')
        });
        this.saving = false;
        this.cancelled.emit();
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

  private extractErrorMessage(error: unknown): string {
    const err = error as { error?: { error?: { message?: string }; message?: string }; message?: string };
    return (
      err?.error?.error?.message ||
      err?.error?.message ||
      err?.message ||
      this.localizationService.localize('Claim::InternalServerErrorMessage')
    );
  }
}
