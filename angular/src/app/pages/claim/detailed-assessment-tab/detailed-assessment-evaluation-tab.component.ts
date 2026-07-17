import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService, MessageService } from 'primeng/api';
import { RadioButtonModule } from 'primeng/radiobutton';
import { TextareaModule } from 'primeng/textarea';
import { ToastModule } from 'primeng/toast';

import {
  DetailedAssessmentEvaluationDetailModel,
  SaveDetailedAssessmentEvaluationInputModel,
} from './detailed-assessment-evaluation-tab.models';
import { DetailedAssessmentEvaluationTabService } from './detailed-assessment-evaluation-tab.service';

@Component({
  selector: 'app-detailed-assessment-evaluation-tab',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ButtonModule,
    RadioButtonModule,
    TextareaModule,
    ConfirmDialogModule,
    ToastModule,
  ],
  templateUrl: './detailed-assessment-evaluation-tab.component.html',
  styleUrl: './detailed-assessment-evaluation-tab.component.scss',
  providers: [ConfirmationService, MessageService],
})
export class DetailedAssessmentEvaluationTabComponent implements OnChanges, OnInit {
  @Input() workTaskId: string | null = null;
  @Input() editable = false;
  @Input() refreshVersion = 0;
  @Input() active = false;
  @Output() closeRequested = new EventEmitter<void>();
  @Output() saved = new EventEmitter<void>();

  loading = false;
  saving = false;
  fieldErrors: Record<string, string> = {};
  model: DetailedAssessmentEvaluationDetailModel = {
    workTaskId: '',
    claimFolderId: null,
    result: null,
    description: null,
    items: [],
  };

  constructor(
    private readonly service: DetailedAssessmentEvaluationTabService,
    private readonly confirmationService: ConfirmationService,
    private readonly messageService: MessageService,
  ) {}

  ngOnInit(): void {
    if (this.active && this.workTaskId) {
      this.loadDetail(this.workTaskId);
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['workTaskId'] && this.workTaskId && this.active) {
      this.loadDetail(this.workTaskId);
    }

    if (changes['refreshVersion'] && !changes['refreshVersion'].firstChange && this.workTaskId) {
      this.loadDetail(this.workTaskId);
    }

    if (changes['active']?.currentValue === true && this.workTaskId) {
      this.loadDetail(this.workTaskId);
    }
  }

  loadDetail(workTaskId: string): void {
    this.loading = true;
    this.service.getDetail(workTaskId).subscribe({
      next: (detail) => {
        this.model = {
          workTaskId: detail.workTaskId,
          claimFolderId: detail.claimFolderId || null,
          result: detail.result ?? null,
          description: detail.description ?? null,
          items: (detail.items || []).map(item => ({
            evaluateItemId: item.evaluateItemId,
            name: item.name,
            result: item.result ?? null,
          })),
        };
        this.fieldErrors = {};
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: 'Không tải được nhận xét đánh giá.',
        });
      },
    });
  }

  isInvalid(field: string): boolean {
    return !!this.fieldErrors[field];
  }

  getError(field: string): string {
    return this.fieldErrors[field] || '';
  }

  onItemResultChange(itemId: string): void {
    delete this.fieldErrors[`item-${itemId}`];
  }

  onConclusionChange(): void {
    delete this.fieldErrors['result'];
  }

  close(): void {
    this.confirmationService.confirm({
      message: 'Bạn có muốn đóng cửa sổ hiện tại không?',
      header: 'Đóng',
      icon: 'pi pi-question-circle',
      accept: () => this.closeRequested.emit(),
    });
  }

  save(): void {
    if (!this.workTaskId || !this.validate()) {
      return;
    }

    const input: SaveDetailedAssessmentEvaluationInputModel = {
      result: this.model.result as 'Y' | 'N',
      description: this.model.description || null,
      items: this.model.items.map(item => ({
        evaluateItemId: item.evaluateItemId,
        result: item.result as 'Y' | 'N',
      })),
    };

    this.saving = true;
    this.service.save(this.workTaskId, input).subscribe({
      next: () => {
        this.saving = false;
        this.messageService.add({
          severity: 'success',
          summary: 'Thành công',
          detail: 'Đã lưu nhận xét đánh giá.',
        });
        this.saved.emit();
      },
      error: (error) => {
        this.saving = false;
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: error?.error?.error?.message || error?.error?.message || 'Không lưu được nhận xét đánh giá.',
        });
      },
    });
  }

  private validate(): boolean {
    this.fieldErrors = {};

    for (const item of this.model.items) {
      if (!item.result) {
        this.fieldErrors[`item-${item.evaluateItemId}`] = 'Bắt buộc chọn Đúng hoặc Không đúng.';
      }
    }

    if (!this.model.result) {
      this.fieldErrors['result'] = 'Kết luận là bắt buộc.';
    }

    if (Object.keys(this.fieldErrors).length > 0) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Thiếu thông tin bắt buộc',
        detail: 'Vui lòng nhập đầy đủ các trường bắt buộc trước khi lưu.',
      });
      return false;
    }

    return true;
  }
}
