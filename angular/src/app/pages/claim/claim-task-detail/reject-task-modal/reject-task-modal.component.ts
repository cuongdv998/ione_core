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
import type { RejectClaimTaskInput } from '@/proxy/claim/claims/models';
import { OnsiteAssessmentDetailService, type OnsiteRejectTaskInput } from '@/pages/claim/onsite-assessment-detail/onsite-assessment-detail.service';
import { DetailedAssessmentTabService } from '@/pages/claim/detailed-assessment-tab/detailed-assessment-tab.service';

@Component({
  selector: 'app-reject-task-modal',
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
  templateUrl: './reject-task-modal.component.html',
  styleUrl: './reject-task-modal.component.scss',
  providers: [MessageService]
})
export class RejectTaskModalComponent implements OnChanges {
  @Input() visible = false;
  @Output() visibleChange = new EventEmitter<boolean>();
  @Input() workTaskId: string = '';
  @Input() mode: 'claim' | 'onsite' | 'detailed' = 'claim';
  @Output() rejected = new EventEmitter<void>();

  reasonId: string | null = null;
  reasonDescription = '';
  saving = false;
  reasonOptions: Array<{ label: string; value: string }> = [];
  loadingReasons = false;

  private readonly reasonGroupCode = 'CLAIM_REJECTION_REASON';

  constructor(
    private claimTaskService: ClaimTaskService,
    private onsiteAssessmentDetailService: OnsiteAssessmentDetailService,
    private detailedAssessmentTabService: DetailedAssessmentTabService,
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
    return document.querySelector<HTMLButtonElement>('.p-dialog .reject-execute-button');
  }

  close(): void {
    this.visible = false;
    this.visibleChange.emit(false);
  }

  submit(): void {
    if (!this.workTaskId || !this.reasonId || !this.reasonDescription?.trim()) {
      this.messageService.add({
        severity: 'warn',
        summary: this.localizationService.localize('AbpUi::ValidationError'),
        detail: this.localizationService.localize('Claim::RejectReasonRequired')
      });
      return;
    }
    const input: RejectClaimTaskInput | OnsiteRejectTaskInput = {
      reasonId: this.reasonId,
      reasonDescription: this.reasonDescription.trim()
    };
    this.saving = true;
    const request$ = this.mode === 'onsite'
      ? this.onsiteAssessmentDetailService.reject(this.workTaskId, input as OnsiteRejectTaskInput)
      : this.mode === 'detailed'
        ? this.detailedAssessmentTabService.reject(this.workTaskId, input as RejectClaimTaskInput)
        : this.claimTaskService.reject(this.workTaskId, input as RejectClaimTaskInput);

    request$.subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('AbpUi::Success'),
          detail: this.localizationService.localize('Claim::RejectSuccess')
        });
        this.saving = false;
        this.rejected.emit();
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
