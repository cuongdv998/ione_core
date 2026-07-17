import { Component, EventEmitter, Input, Output, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';

import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';
import { ClaimFolderService } from '@/proxy/claim/controllers/claim-folder.service';
import { ResPartnerService } from '@/proxy/partner/controllers/res-partner.service';
import type { CreateClaimFolderDto } from '@/proxy/claim/claims/models';
import { ClaimFolderPriority } from '@/proxy/claim-folders/claim-folder-priority.enum';

@Component({
  selector: 'app-create-claim-folder-modal',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    DialogModule,
    ButtonModule,
    SelectModule,
    InputTextModule,
    TextareaModule,
    ToastModule,
    TranslatePipe
  ],
  templateUrl: './create-claim-folder-modal.component.html',
  styleUrl: './create-claim-folder-modal.component.scss',
  providers: [MessageService]
})
export class CreateClaimFolderModalComponent implements OnChanges {
  @Input() visible = false;
  @Output() visibleChange = new EventEmitter<boolean>();
  @Input() claimId: string = '';
  @Output() created = new EventEmitter<void>();

  saving = false;
  claimTypeId = '';
  folderName = '';
  policyNo = '';
  insurerPolicyNo = '';
  description = '';
  priority: ClaimFolderPriority | null = null;
  hasAdjustLocation: string | null = null;
  insurerId: string | null = null;

  insurerOptions: Array<{ label: string; value: string }> = [];
  loadingInsurers = false;
  priorityOptions: Array<{ label: string; value: ClaimFolderPriority }> = [];

  constructor(
    private claimFolderService: ClaimFolderService,
    private partnerService: ResPartnerService,
    private messageService: MessageService,
    private localizationService: LocalizationService
  ) {
    this.priorityOptions = [
      { label: this.localizationService.localize('Claim::Priority:Low') || 'Low', value: ClaimFolderPriority.Low },
      { label: this.localizationService.localize('Claim::Priority:Normal') || 'Normal', value: ClaimFolderPriority.Medium },
      { label: this.localizationService.localize('Claim::Priority:High') || 'High', value: ClaimFolderPriority.High }
    ];
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['visible'] && this.visible) {
      this.resetForm();
      this.loadInsurers();
    }
  }

  resetForm(): void {
    this.claimTypeId = '';
    this.folderName = '';
    this.policyNo = '';
    this.insurerPolicyNo = '';
    this.description = '';
    this.priority = null;
    this.hasAdjustLocation = null;
    this.insurerId = null;
  }

  loadInsurers(): void {
    this.loadingInsurers = true;
    this.partnerService.getList({ maxResultCount: 500 }).subscribe({
      next: (res) => {
        this.insurerOptions = (res.items ?? []).map((x: { id?: string; name?: string }) => ({
          label: x.name ?? x.id ?? '',
          value: x.id ?? ''
        }));
        this.loadingInsurers = false;
      },
      error: () => {
        this.loadingInsurers = false;
      }
    });
  }

  onVisibleChange(value: boolean): void {
    this.visibleChange.emit(value);
  }

  close(): void {
    this.visible = false;
    this.visibleChange.emit(false);
  }

  submit(): void {
    if (!this.claimId || !this.claimTypeId?.trim()) {
      this.messageService.add({
        severity: 'warn',
        summary: this.localizationService.localize('AbpUi::ValidationError'),
        detail: this.localizationService.localize('Claim::ClaimTypeRequired')
      });
      return;
    }
    const dto: CreateClaimFolderDto = {
      claimId: this.claimId,
      claimTypeId: this.claimTypeId.trim(),
      insurerId: this.insurerId ?? undefined,
      folderName: this.folderName.trim() || undefined,
      policyNo: this.policyNo.trim() || undefined,
      insurerPolicyNo: this.insurerPolicyNo.trim() || undefined,
      description: this.description.trim() || undefined,
      priority: this.priority ?? undefined,
      hasAdjustLocation: this.hasAdjustLocation ?? undefined,
      incidentObjectIds: []
    };
    this.saving = true;
    this.claimFolderService.create(dto).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('AbpUi::Success'),
          detail: this.localizationService.localize('Claim::FolderCreated')
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
