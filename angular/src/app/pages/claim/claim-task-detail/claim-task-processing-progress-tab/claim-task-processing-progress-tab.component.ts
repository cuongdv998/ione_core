import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { PanelModule } from 'primeng/panel';
import { TableModule } from 'primeng/table';

import { TranslatePipe } from '@/core/pipes/translate.pipe';
import type {
  ClaimFolderListDto,
  ClaimStageProgressDto,
  ClaimSentMessageDto
} from '@/proxy/claim/claims/models';

@Component({
  selector: 'app-claim-task-processing-progress-tab',
  standalone: true,
  imports: [CommonModule, PanelModule, TableModule, TranslatePipe],
  templateUrl: './claim-task-processing-progress-tab.component.html',
  styleUrl: './claim-task-processing-progress-tab.component.scss'
})
export class ClaimTaskProcessingProgressTabComponent {
  @Input() folders: ClaimFolderListDto[] = [];
  @Input() loadingFolders = false;
  @Input() stageProgress: ClaimStageProgressDto[] = [];
  @Input() loadingStageProgress = false;
  @Input() sentMessages: ClaimSentMessageDto[] = [];
  @Input() loadingSentMessages = false;

  formatDate(value: string | undefined): string {
    if (!value) return '-';
    const d = new Date(value);
    return isNaN(d.getTime()) ? String(value) : d.toLocaleDateString('vi-VN');
  }
}
