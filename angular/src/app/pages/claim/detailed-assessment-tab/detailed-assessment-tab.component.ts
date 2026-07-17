import { CommonModule } from '@angular/common';
import { RestService } from '@abp/ng.core';
import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { ButtonModule } from 'primeng/button';
import { CheckboxModule } from 'primeng/checkbox';
import { DatePickerModule } from 'primeng/datepicker';
import { DialogModule } from 'primeng/dialog';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { TextareaModule } from 'primeng/textarea';
import { ToastModule } from 'primeng/toast';
import { TooltipModule } from 'primeng/tooltip';
import { TagModule } from 'primeng/tag';
import { MessageService } from 'primeng/api';
import { forkJoin } from 'rxjs';

import { LocalizationService } from '@/core/services/localization.service';
import type { ClaimDetailDto } from '@/proxy/claim/claims/models';
import { ResDocumentService } from '@/proxy/master/controllers/res-document.service';
import { ResDocumentTypeService } from '@/proxy/master/controllers/res-document-type.service';
import type { ResDocumentTypeDto } from '@/proxy/master/res-document-types/models';
import { WorkTaskStatus } from '@/proxy/work-tasks/work-task-status.enum';

import {
  DetailedAssessmentAttachmentDto,
  DetailedAssessmentDetailDto,
  DetailedAssessmentDocumentRowDto,
  DetailedAssessmentFormData,
  DetailedAssessmentItemDto,
  DetailedAssessmentOptionDto,
  DetailedAssessmentSectionDto,
  SaveDetailedAssessmentInput,
} from './detailed-assessment-tab.models';
import { DetailedAssessmentTabService } from './detailed-assessment-tab.service';

@Component({
  selector: 'app-detailed-assessment-tab',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ButtonModule,
    CheckboxModule,
    DatePickerModule,
    DialogModule,
    InputNumberModule,
    InputTextModule,
    SelectModule,
    TableModule,
    TextareaModule,
    ToastModule,
    TooltipModule,
    TagModule,
  ],
  templateUrl: './detailed-assessment-tab.component.html',
  styleUrl: './detailed-assessment-tab.component.scss',
  providers: [MessageService],
})
export class DetailedAssessmentTabComponent implements OnChanges {
  private static readonly DETAIL_DOCUMENT_GROUP_CODE = 'CLAIM_ADJUST_ONSITE_PROFILE';
  @Input() claim: ClaimDetailDto | null = null;
  @Input() workTaskId: string | null = null;
  @Input() workTaskStatus: WorkTaskStatus | null = null;
  @Input() quotationApprovalStatus: string | null = null;
  @Input() allowCompletedEdit = false;
  @Input() refreshVersion = 0;
  @Input() forceReadOnly = false;
  @Input() showDetailedActions = true;
  @Output() closeRequested = new EventEmitter<void>();
  @Output() rejectRequested = new EventEmitter<void>();
  @Output() acceptRequested = new EventEmitter<void>();
  @Output() transferRequested = new EventEmitter<void>();
  @Output() reassignRequested = new EventEmitter<void>();
  @Output() completed = new EventEmitter<void>();

  readonly WorkTaskStatus = WorkTaskStatus;
  private static readonly completedEditAllowedQuotationStatuses = new Set([
    'new',
    'rejected',
    'cancelled',
  ]);

  loading = false;
  saving = false;
  completing = false;
  downloadingMinutes = false;
  documentUploadDialogVisible = false;
  itemUploadDialogVisible = false;
  activeDocumentRowId: string | null = null;
  activeItemDialogTarget: { sectionId: string; itemId: string } | null = null;
  selectedItemUploadFileId: string | null = null;
  selectedDocumentUploadFileId: string | null = null;
  itemUploadPreviewUrl: string | null = null;
  itemUploadPreviewResourceUrl: SafeResourceUrl | null = null;
  itemUploadPreviewName = '';
  itemUploadPreviewType: 'image' | 'pdf' | 'other' = 'other';
  documentUploadPreviewUrl: string | null = null;
  documentUploadPreviewResourceUrl: SafeResourceUrl | null = null;
  documentUploadPreviewName = '';
  documentUploadPreviewType: 'image' | 'pdf' | 'other' = 'other';
  documentFilePreviewVisible = false;
  documentFilePreviewUrl: string | null = null;
  documentFilePreviewResourceUrl: SafeResourceUrl | null = null;
  documentFilePreviewName = '';
  documentFilePreviewType: 'image' | 'pdf' | 'other' = 'other';

  form: DetailedAssessmentFormData = this.getEmptyForm();
  licenseLevelOptions: DetailedAssessmentOptionDto[] = [];
  coverageOptions: DetailedAssessmentOptionDto[] = [];
  riskOptions: DetailedAssessmentOptionDto[] = [];
  claimPlanOptions: DetailedAssessmentOptionDto[] = [];
  itemOptions: DetailedAssessmentOptionDto[] = [];
  fieldErrors: Record<string, string> = {};
  detailWorkTaskStatus: WorkTaskStatus | null = null;
  detailIsReporter = false;
  detailCanReassign = false;
  private itemAttachmentDocTypeId: string | null = null;
  private detailedDocumentTypeByRowId: Record<string, { code: string; docTypeId: string | null; name: string }> = {};
  private configuredDocumentRows: DetailedAssessmentDocumentRowDto[] = [];

  private loadedWorkTaskId: string | null = null;
  private activeItemUploadTarget: { sectionId: string; itemId: string } | null = null;
  private activeDocumentUploadId: string | null = null;

  constructor(
    private readonly service: DetailedAssessmentTabService,
    private readonly messageService: MessageService,
    private readonly resDocumentService: ResDocumentService,
    private readonly resDocumentTypeService: ResDocumentTypeService,
    private readonly restService: RestService,
    private readonly sanitizer: DomSanitizer,
    private readonly localizationService: LocalizationService,
  ) {
    this.loadItemAttachmentDocType();
    this.loadDetailedDocumentTypes();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['workTaskId'] && this.workTaskId && this.loadedWorkTaskId !== this.workTaskId) {
      this.loadDetail(this.workTaskId);
    }

    if (changes['refreshVersion'] && !changes['refreshVersion'].firstChange && this.workTaskId) {
      this.loadDetail(this.workTaskId);
    }

    if (changes['claim'] && !this.form.assessorDeptName && this.claim?.processDeptName) {
      this.form = {
        ...this.form,
        assessorDeptName: this.claim.processDeptName,
      };
    }
  }

  get isEditable(): boolean {
    if (this.forceReadOnly) {
      return false;
    }

    if (this.actionWorkTaskStatus === WorkTaskStatus.InProgress) {
      return true;
    }

    return this.actionWorkTaskStatus === WorkTaskStatus.Completed && this.canEditCompletedAssessment;
  }

  get actionWorkTaskStatus(): WorkTaskStatus | null {
    return this.detailWorkTaskStatus ?? this.workTaskStatus;
  }

  get canShowSaveAction(): boolean {
    return this.showDetailedActions && this.isEditable;
  }

  private get canEditCompletedAssessment(): boolean {
    if (this.allowCompletedEdit) {
      return true;
    }

    const status = (this.quotationApprovalStatus ?? '').trim().toLowerCase();
    return DetailedAssessmentTabComponent.completedEditAllowedQuotationStatuses.has(status);
  }

  getWorkTaskStatusDisplay(): string {
    const s = this.actionWorkTaskStatus;
    if (s == null) return '-';
    const labels: Partial<Record<WorkTaskStatus, string>> = {
      [WorkTaskStatus.New]:
        this.localizationService.localize('Claim::WorkTaskStatus:New') || 'Chờ tiếp nhận',
      [WorkTaskStatus.InProgress]:
        this.localizationService.localize('Claim::WorkTaskStatus:InProgress') || 'Đang xử lý',
      [WorkTaskStatus.Completed]:
        this.localizationService.localize('Claim::WorkTaskStatus:Completed') || 'Đã hoàn thành',
      [WorkTaskStatus.Cancelled]:
        this.localizationService.localize('Claim::WorkTaskStatus:Cancelled') || 'Đã hủy',
      [WorkTaskStatus.Accepted]: 'Đã tiếp nhận',
      [WorkTaskStatus.Rejected]:
        this.localizationService.localize('Claim::WorkTaskStatus:Rejected') || 'Từ chối',
      [WorkTaskStatus.Pending]: 'Tạm dừng',
      [WorkTaskStatus.Return]: 'Trả lại',
      [WorkTaskStatus.WaitApprove]: 'Chờ duyệt',
      [WorkTaskStatus.Approved]: 'Đã duyệt',
    };
    return labels[s] ?? String(s);
  }

  getWorkTaskStatusSeverity(): 'success' | 'secondary' | 'info' | 'warn' | 'danger' | 'contrast' {
    const s = this.actionWorkTaskStatus;
    if (s == null) return 'secondary';
    switch (s) {
      case WorkTaskStatus.New:
        return 'info';
      case WorkTaskStatus.InProgress:
        return 'warn';
      case WorkTaskStatus.Completed:
      case WorkTaskStatus.Accepted:
      case WorkTaskStatus.Approved:
        return 'success';
      case WorkTaskStatus.Rejected:
      case WorkTaskStatus.Return:
        return 'danger';
      case WorkTaskStatus.Cancelled:
      case WorkTaskStatus.Pending:
        return 'secondary';
      case WorkTaskStatus.WaitApprove:
        return 'warn';
      default:
        return 'secondary';
    }
  }

  get canShowReassignAction(): boolean {
    if (!this.showDetailedActions || this.forceReadOnly) {
      return false;
    }

    return this.detailCanReassign
      || (
        this.detailIsReporter
        && (
          this.actionWorkTaskStatus === WorkTaskStatus.Rejected
          || this.actionWorkTaskStatus === WorkTaskStatus.Pending
          || this.actionWorkTaskStatus === WorkTaskStatus.Return
        )
      );
  }

  loadDetail(workTaskId: string): void {
    this.loading = true;
    forkJoin({
      detail: this.service.getDetail(workTaskId),
      coverageOptions: this.service.getCoverageOptions(workTaskId),
    }).subscribe({
      next: ({ detail, coverageOptions }) => {
        this.loadedWorkTaskId = workTaskId;
        this.bindDetail({
          ...detail,
          coverageOptions,
        });
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: 'Không tải được thông tin giám định chi tiết.',
        });
      },
    });
  }

  addCoverageSection(): void {
    if (!this.isEditable) return;
    if (!this.form.selectedCoverageId) {
      this.messageService.add({ severity: 'warn', summary: 'Cảnh báo', detail: 'Chọn hạng mục giám định trước khi thêm.' });
      return;
    }

    const coverage = this.coverageOptions.find(x => this.getCoverageOptionKey(x) === this.form.selectedCoverageId);
    if (!coverage) {
      return;
    }

    const duplicatedSection = this.form.sections.find(section =>
      section.coverageId === coverage.id
      && (section.objectTypeCode || '') === (coverage.objectTypeCode || '')
      && (section.objectTypeName || '') === (coverage.objectTypeName || '')
      && (section.objectTypeGroup || '') === (coverage.objectTypeGroup || '')
    );
    if (duplicatedSection) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Cảnh báo',
        detail: `Hạng mục giám định (${coverage.name}) đã tồn tại`
      });
      return;
    }

    this.form.sections = [
      ...this.form.sections,
      {
        id: crypto.randomUUID(),
        coverageId: coverage.id,
        coverageName: coverage.name,
        objectTypeId: coverage.objectTypeId || null,
        objectTypeCode: coverage.objectTypeCode || null,
        objectTypeName: coverage.objectTypeName || null,
        objectTypeGroup: coverage.objectTypeGroup || null,
        objectKind: coverage.objectKind || this.getObjectKindFromOption(coverage),
        items: [this.createItem(coverage)],
      },
    ];
  }

  addItem(section: DetailedAssessmentSectionDto): void {
    if (!this.isEditable) return;
    section.items = [...section.items, this.createItem(section)];
  }

  removeItem(section: DetailedAssessmentSectionDto, itemId: string): void {
    if (!this.isEditable) return;
    section.items = section.items.filter(item => item.id !== itemId);
  }

  removeSection(sectionId: string): void {
    if (!this.isEditable) return;
    this.form.sections = this.form.sections.filter(section => section.id !== sectionId);
  }

  triggerItemUpload(sectionId: string, itemId: string, input: HTMLInputElement): void {
    if (!this.isEditable) return;
    this.activeItemUploadTarget = { sectionId, itemId };
    input.click();
  }

  onItemFilesSelected(event: Event | File[]): void {
    if (!this.isEditable) return;
    const { input, files } = this.extractSelectedFiles(event);
    if (!this.activeItemUploadTarget || files.length === 0) {
      if (input) {
        input.value = '';
      }
      return;
    }

    const section = this.form.sections.find(x => x.id === this.activeItemUploadTarget?.sectionId);
    const item = section?.items.find(x => x.id === this.activeItemUploadTarget?.itemId);
    if (!item) {
      this.activeItemUploadTarget = null;
      if (input) {
        input.value = '';
      }
      return;
    }

    if (!this.itemAttachmentDocTypeId) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Cảnh báo',
        detail: 'Chưa tải được loại tài liệu cho hạng mục chi tiết.'
      });
      this.activeItemUploadTarget = null;
      if (input) {
        input.value = '';
      }
      return;
    }

    const placeholders = files.map(file => this.createUploadingAttachment(file));
    item.attachments = [...item.attachments, ...placeholders];

    this.resDocumentService
      .uploadMultipleFiles(this.itemAttachmentDocTypeId, 'CLAIM_DETAILED_ITEM', files)
      .subscribe({
      next: (uploadedDocs) => {
        const uploadedAttachments = uploadedDocs.map(doc => ({
          id: doc.id || crypto.randomUUID(),
          documentId: doc.id || null,
          claimDocumentId: null,
          fileName: doc.fileName || 'Tệp đính kèm',
          url: doc.url || null,
          thumbnailUrl: doc.thumbnailUrl || null,
          mimeType: doc.mimeType || null,
          uploading: false,
        }));
        item.attachments = item.attachments.filter(existing => !placeholders.some(x => x.id === existing.id));
        item.attachments = [...item.attachments, ...uploadedAttachments];
        const nextPreviewFile = item.attachments.find((attachment) =>
          uploadedAttachments.some((uploaded) => uploaded.id === attachment.id)
        ) || item.attachments[0];
        if (nextPreviewFile && this.itemUploadDialogVisible) {
          this.selectItemUploadFile(nextPreviewFile);
        }
      },
      error: () => {
        item.attachments = item.attachments.filter(existing => !placeholders.some(x => x.id === existing.id));
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: 'Không tải được tài liệu của hạng mục chi tiết.'
        });
      }
    });

    this.activeItemUploadTarget = null;
    if (input) {
      input.value = '';
    }
  }

  onDocumentFilesSelected(event: Event | File[]): void {
    if (!this.isEditable) return;
    const { input, files } = this.extractSelectedFiles(event);
    if (!this.activeDocumentUploadId || files.length === 0) {
      if (input) {
        input.value = '';
      }
      return;
    }

    const row = this.form.documentRows.find(x => x.id === this.activeDocumentUploadId);
    const rowType = this.activeDocumentUploadId ? this.detailedDocumentTypeByRowId[this.activeDocumentUploadId] : null;
    const documentTypeId = row?.documentTypeId || rowType?.docTypeId || null;
    if (!row || !documentTypeId) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Cảnh báo',
        detail: 'Chưa cấu hình loại tài liệu cho dòng hồ sơ giấy tờ.'
      });
      this.activeDocumentUploadId = null;
      if (input) {
        input.value = '';
      }
      return;
    }

    const placeholders = files.map(file => this.createUploadingAttachment(file));
    row.attachments = [...row.attachments, ...placeholders];

    this.resDocumentService
      .uploadMultipleFiles(documentTypeId, row?.code || rowType?.code || 'OTHER', files)
      .subscribe({
      next: (uploadedDocs) => {
        const uploadedAttachments = uploadedDocs.map(doc => ({
          id: doc.id || crypto.randomUUID(),
          documentId: doc.id || null,
          claimDocumentId: null,
          fileName: doc.fileName || 'Tệp đính kèm',
          url: doc.url || null,
          thumbnailUrl: doc.thumbnailUrl || null,
          mimeType: doc.mimeType || null,
          uploading: false,
        }));

        row.attachments = row.attachments.filter(existing => !placeholders.some(x => x.id === existing.id));
        row.attachments = [...row.attachments, ...uploadedAttachments];
        const nextPreviewFile = row.attachments.find((attachment) =>
          uploadedAttachments.some((uploaded) => uploaded.id === attachment.id)
        ) || row.attachments[0];
        if (nextPreviewFile && this.documentUploadDialogVisible) {
          this.selectDocumentUploadFile(nextPreviewFile);
        }
      },
      error: () => {
        row.attachments = row.attachments.filter(existing => !placeholders.some(x => x.id === existing.id));
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: 'Không tải được tệp hồ sơ giấy tờ.'
        });
      }
    });

    this.activeDocumentUploadId = null;
    if (input) {
      input.value = '';
    }
  }

  triggerDocumentUpload(documentId: string, input: HTMLInputElement): void {
    if (!this.isEditable) return;
    this.activeDocumentUploadId = documentId;
    input.click();
  }

  removeAttachment(attachments: DetailedAssessmentAttachmentDto[], attachmentId: string): DetailedAssessmentAttachmentDto[] {
    return attachments.filter(file => file.id !== attachmentId);
  }

  removeItemAttachment(item: DetailedAssessmentItemDto, attachment: DetailedAssessmentAttachmentDto): void {
    item.attachments = item.attachments.filter(file => file.id !== attachment.id);

    if (attachment.documentId && !attachment.claimDocumentId) {
      this.resDocumentService.deleteSingleFile(attachment.documentId).subscribe({
        error: () => {
          this.messageService.add({
            severity: 'warn',
            summary: 'Cảnh báo',
            detail: 'Không xóa được tệp vừa tải lên khỏi kho tài liệu.'
          });
        }
      });
    }

    if (this.selectedItemUploadFileId === attachment.id) {
      const nextFile = item.attachments[0] || null;
      if (nextFile) {
        this.selectItemUploadFile(nextFile);
      } else {
        this.selectedItemUploadFileId = null;
        this.resetItemInlinePreview();
      }
    }
  }

  openItemUploadDialog(sectionId: string, itemId: string): void {
    this.activeItemDialogTarget = { sectionId, itemId };
    this.itemUploadDialogVisible = true;
    this.selectedItemUploadFileId = null;
    this.resetItemInlinePreview();
    const firstFile = this.getActiveItemUpload()?.attachments[0];
    if (firstFile) {
      this.selectItemUploadFile(firstFile);
    }
  }

  closeItemUploadDialog(): void {
    this.itemUploadDialogVisible = false;
    this.activeItemDialogTarget = null;
    this.selectedItemUploadFileId = null;
    this.resetItemInlinePreview();
  }

  getActiveItemUpload(): DetailedAssessmentItemDto | null {
    if (!this.activeItemDialogTarget) return null;

    const section = this.form.sections.find(x => x.id === this.activeItemDialogTarget?.sectionId);
    return section?.items.find(x => x.id === this.activeItemDialogTarget?.itemId) || null;
  }

  getActiveItemUploadTitle(): string {
    const item = this.getActiveItemUpload();
    return item?.itemName || item?.personName || 'Hạng mục chi tiết';
  }

  triggerActiveItemUpload(input: HTMLInputElement): void {
    if (!this.isEditable || !this.activeItemDialogTarget) return;
    this.activeItemUploadTarget = { ...this.activeItemDialogTarget };
    input.click();
  }

  selectItemUploadFile(file: DetailedAssessmentAttachmentDto): void {
    this.selectedItemUploadFileId = file.id;
    this.resolveAttachmentUrl(file, (url) => {
      this.applyItemInlinePreview(file, url);
    });
  }

  isSelectedItemUploadFile(file: DetailedAssessmentAttachmentDto): boolean {
    return this.selectedItemUploadFileId === file.id;
  }

  viewItemFile(file: DetailedAssessmentAttachmentDto): void {
    this.resolveAttachmentUrl(file, (url) => {
      const lowerName = (file.fileName || '').toLowerCase();
      const lowerUrl = url.toLowerCase();
      const isImage = /\.(png|jpg|jpeg|gif|bmp|webp|svg)$/.test(lowerName) || /\.(png|jpg|jpeg|gif|bmp|webp|svg)(\?|$)/.test(lowerUrl);
      const isPdf = lowerName.endsWith('.pdf') || /\.pdf(\?|$)/.test(lowerUrl);

      this.documentFilePreviewName = file.fileName;
      this.documentFilePreviewUrl = null;
      this.documentFilePreviewResourceUrl = null;

      if (isImage) {
        this.documentFilePreviewType = 'image';
        this.documentFilePreviewUrl = url;
        this.documentFilePreviewVisible = true;
        return;
      }

      if (isPdf) {
        this.documentFilePreviewType = 'pdf';
        this.documentFilePreviewResourceUrl = this.sanitizer.bypassSecurityTrustResourceUrl(url);
        this.documentFilePreviewVisible = true;
        return;
      }

      this.documentFilePreviewType = 'other';
      this.documentFilePreviewVisible = true;
    });
  }

  downloadItemFile(file: DetailedAssessmentAttachmentDto): void {
    this.resolveAttachmentUrl(file, (url) => {
      const a = document.createElement('a');
      a.href = url;
      a.download = file.fileName || 'tai-lieu';
      a.target = '_blank';
      a.rel = 'noopener';
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
    });
  }

  openDocumentUploadDialog(documentId: string): void {
    this.activeDocumentRowId = documentId;
    this.documentUploadDialogVisible = true;
    this.selectedDocumentUploadFileId = null;
    this.resetDocumentInlinePreview();
    const firstFile = this.getActiveDocumentRow()?.attachments[0];
    if (firstFile) {
      this.selectDocumentUploadFile(firstFile);
    }
  }

  closeDocumentUploadDialog(): void {
    this.documentUploadDialogVisible = false;
    this.activeDocumentRowId = null;
    this.selectedDocumentUploadFileId = null;
    this.resetDocumentInlinePreview();
  }

  getActiveDocumentRow(): DetailedAssessmentDocumentRowDto | null {
    if (!this.activeDocumentRowId) return null;
    return this.form.documentRows.find(x => x.id === this.activeDocumentRowId) || null;
  }

  selectDocumentUploadFile(file: DetailedAssessmentAttachmentDto): void {
    this.selectedDocumentUploadFileId = file.id;
    this.resolveAttachmentUrl(file, (url) => {
      this.applyDocumentInlinePreview(file, url);
    });
  }

  isSelectedDocumentUploadFile(file: DetailedAssessmentAttachmentDto): boolean {
    return this.selectedDocumentUploadFileId === file.id;
  }

  getDetailUploadFileType(file: DetailedAssessmentAttachmentDto): 'image' | 'pdf' | 'other' {
    return this.getPreviewFileType(file.fileName, file.url || file.thumbnailUrl, file.mimeType);
  }

  viewDocumentRowFile(file: DetailedAssessmentAttachmentDto): void {
    this.resolveAttachmentUrl(file, (url) => {
      const lowerName = (file.fileName || '').toLowerCase();
      const lowerUrl = url.toLowerCase();
      const isImage = /\.(png|jpg|jpeg|gif|bmp|webp|svg)$/.test(lowerName) || /\.(png|jpg|jpeg|gif|bmp|webp|svg)(\?|$)/.test(lowerUrl);
      const isPdf = lowerName.endsWith('.pdf') || /\.pdf(\?|$)/.test(lowerUrl);

      this.documentFilePreviewName = file.fileName;
      this.documentFilePreviewUrl = null;
      this.documentFilePreviewResourceUrl = null;

      if (isImage) {
        this.documentFilePreviewType = 'image';
        this.documentFilePreviewUrl = url;
        this.documentFilePreviewVisible = true;
        return;
      }

      if (isPdf) {
        this.documentFilePreviewType = 'pdf';
        this.documentFilePreviewResourceUrl = this.sanitizer.bypassSecurityTrustResourceUrl(url);
        this.documentFilePreviewVisible = true;
        return;
      }

      this.documentFilePreviewType = 'other';
      this.documentFilePreviewVisible = true;
    });
  }

  downloadDocumentRowFile(file: DetailedAssessmentAttachmentDto): void {
    this.resolveAttachmentUrl(file, (url) => {
      const a = document.createElement('a');
      a.href = url;
      a.download = file.fileName || 'tai-lieu';
      a.target = '_blank';
      a.rel = 'noopener';
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
    });
  }

  removeDocumentRowFile(file: DetailedAssessmentAttachmentDto): void {
    if (file.uploading || !this.activeDocumentRowId) return;

    this.form.documentRows = this.form.documentRows.map((row) =>
      row.id === this.activeDocumentRowId
        ? { ...row, attachments: row.attachments.filter((attached) => attached.id !== file.id) }
        : row
    );

    if (file.documentId && !file.claimDocumentId) {
      this.resDocumentService.deleteSingleFile(file.documentId).subscribe({
        error: () => {
          this.messageService.add({
            severity: 'warn',
            summary: 'Cảnh báo',
            detail: 'Không xóa được tệp vừa tải lên khỏi kho tài liệu.'
          });
        }
      });
    }

    if (this.selectedDocumentUploadFileId === file.id) {
      const nextFile = this.getActiveDocumentRow()?.attachments[0] || null;
      if (nextFile) {
        this.selectDocumentUploadFile(nextFile);
      } else {
        this.selectedDocumentUploadFileId = null;
        this.resetDocumentInlinePreview();
      }
    }
  }

  closeDocumentFilePreview(): void {
    this.documentFilePreviewVisible = false;
    this.documentFilePreviewName = '';
    this.documentFilePreviewType = 'other';
    this.documentFilePreviewUrl = null;
    this.documentFilePreviewResourceUrl = null;
  }

  private applyItemInlinePreview(file: DetailedAssessmentAttachmentDto, url: string): void {
    const fileType = this.getPreviewFileType(file.fileName, url, file.mimeType);

    this.itemUploadPreviewName = file.fileName;
    this.itemUploadPreviewUrl = null;
    this.itemUploadPreviewResourceUrl = null;

    if (fileType === 'image') {
      this.itemUploadPreviewType = 'image';
      this.itemUploadPreviewUrl = url;
      return;
    }

    if (fileType === 'pdf') {
      this.itemUploadPreviewType = 'pdf';
      this.itemUploadPreviewResourceUrl = this.sanitizer.bypassSecurityTrustResourceUrl(url);
      return;
    }

    this.itemUploadPreviewType = 'other';
    this.itemUploadPreviewUrl = url;
  }

  private applyDocumentInlinePreview(file: DetailedAssessmentAttachmentDto, url: string): void {
    const fileType = this.getPreviewFileType(file.fileName, url, file.mimeType);

    this.documentUploadPreviewName = file.fileName;
    this.documentUploadPreviewUrl = null;
    this.documentUploadPreviewResourceUrl = null;

    if (fileType === 'image') {
      this.documentUploadPreviewType = 'image';
      this.documentUploadPreviewUrl = url;
      return;
    }

    if (fileType === 'pdf') {
      this.documentUploadPreviewType = 'pdf';
      this.documentUploadPreviewResourceUrl = this.sanitizer.bypassSecurityTrustResourceUrl(url);
      return;
    }

    this.documentUploadPreviewType = 'other';
    this.documentUploadPreviewUrl = url;
  }

  private resetItemInlinePreview(): void {
    this.itemUploadPreviewName = '';
    this.itemUploadPreviewType = 'other';
    this.itemUploadPreviewUrl = null;
    this.itemUploadPreviewResourceUrl = null;
  }

  private resetDocumentInlinePreview(): void {
    this.documentUploadPreviewName = '';
    this.documentUploadPreviewType = 'other';
    this.documentUploadPreviewUrl = null;
    this.documentUploadPreviewResourceUrl = null;
  }

  save(): void {
    if (!this.workTaskId || !this.validate()) {
      return;
    }

    this.saving = true;
    this.service.save(this.workTaskId, this.toPayload()).subscribe({
      next: () => {
        this.saving = false;
        this.loadDetail(this.workTaskId!);
        this.messageService.add({ severity: 'success', summary: 'Thành công', detail: 'Đã lưu giám định chi tiết.' });
      },
      error: (error) => {
        this.saving = false;
        this.showSaveError(error, 'Không lưu được giám định chi tiết.');
      },
    });
  }

  complete(): void {
    if (!this.workTaskId || !this.validate()) {
      return;
    }

    this.completing = true;
    this.service.complete(this.workTaskId, this.toPayload()).subscribe({
      next: () => {
        this.completing = false;
        this.loadDetail(this.workTaskId!);
        this.completed.emit();
        this.messageService.add({
          severity: 'success',
          summary: 'Thành công',
          detail: 'Đã hoàn thành giám định chi tiết.',
        });
      },
      error: (error) => {
        this.completing = false;
        this.showSaveError(error, 'Không hoàn thành được giám định chi tiết.');
      },
    });
  }

  acceptTask(): void {
    this.acceptRequested.emit();
  }

  rejectTask(): void {
    this.rejectRequested.emit();
  }

  transferTask(): void {
    this.transferRequested.emit();
  }

  reassignTask(): void {
    this.reassignRequested.emit();
  }

  downloadDetailedMinutes(): void {
    this.downloadingMinutes = true;
    setTimeout(() => {
      this.downloadingMinutes = false;
      this.messageService.add({
        severity: 'info',
        summary: 'Thông báo',
        detail: 'Sẵn sàng tích hợp API sinh biên bản giám định chi tiết.'
      });
    }, 300);
  }

  close(): void {
    this.closeRequested.emit();
  }

  formatDateTime(value: Date | string | null | undefined): string {
    if (!value) return '-';
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) return String(value);
    return new Intl.DateTimeFormat('vi-VN', {
      hour: '2-digit',
      minute: '2-digit',
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour12: false,
    }).format(date).replace(',', '');
  }

  getNeedSupplement(row: DetailedAssessmentDocumentRowDto): boolean {
    return row.complete === false;
  }

  isPersonSection(section: DetailedAssessmentSectionDto): boolean {
    return this.getSectionObjectKind(section) === 'person';
  }

  getSectionObjectKind(section: DetailedAssessmentSectionDto): string {
    return section.objectKind || this.getObjectKind(section.objectTypeCode, section.objectTypeName, section.objectTypeGroup);
  }

  getFilteredItemOptions(section: DetailedAssessmentSectionDto, selectedItemId?: string | null): DetailedAssessmentOptionDto[] {
    if (!section.objectTypeId) {
      return this.includeSelectedOption(this.itemOptions, selectedItemId);
    }

    return this.includeSelectedOption(
      this.itemOptions.filter(x => !x.objectTypeId || x.objectTypeId === section.objectTypeId),
      selectedItemId,
      this.itemOptions
    );
  }

  getFilteredRiskOptions(section: DetailedAssessmentSectionDto, selectedRiskId?: string | null): DetailedAssessmentOptionDto[] {
    if (!section.objectTypeId) {
      return this.includeSelectedOption(this.riskOptions, selectedRiskId);
    }

    return this.includeSelectedOption(
      this.riskOptions.filter(x => !x.objectTypeId || x.objectTypeId === section.objectTypeId),
      selectedRiskId,
      this.riskOptions
    );
  }

  getItemTooltip(section: DetailedAssessmentSectionDto, itemId?: string | null): string {
    return this.getSelectedOptionName(this.getFilteredItemOptions(section, itemId), itemId, this.itemOptions);
  }

  getRiskTooltip(section: DetailedAssessmentSectionDto, riskId?: string | null): string {
    return this.getSelectedOptionName(this.getFilteredRiskOptions(section, riskId), riskId, this.riskOptions);
  }

  getCoverageTooltip(selectedCoverageId?: string | null): string {
    if (!selectedCoverageId) {
      return '';
    }

    const selectedOption = this.coverageOptions.find(option => this.getCoverageOptionKey(option) === selectedCoverageId);
    return selectedOption?.displayName || selectedOption?.name || '';
  }

  getClaimPlanTooltip(claimPlanId?: string | null): string {
    return this.getSelectedOptionName(this.claimPlanOptions, claimPlanId);
  }

  getGenuineTooltip(value?: string | null): string {
    if (value === 'Y') {
      return 'Chính hãng';
    }

    if (value === 'N') {
      return 'Khác';
    }

    return '';
  }

  hasError(key: string): boolean {
    return !!this.fieldErrors[key];
  }

  getError(key: string): string {
    return this.fieldErrors[key] || '';
  }

  clearError(key: string): void {
    if (this.fieldErrors[key]) {
      delete this.fieldErrors[key];
      this.fieldErrors = { ...this.fieldErrors };
    }
  }

  clearErrors(keys: string[]): void {
    let changed = false;
    for (const key of keys) {
      if (this.fieldErrors[key]) {
        delete this.fieldErrors[key];
        changed = true;
      }
    }

    if (changed) {
      this.fieldErrors = { ...this.fieldErrors };
    }
  }

  private bindDetail(detail: DetailedAssessmentDetailDto): void {
    this.detailWorkTaskStatus = this.toWorkTaskStatus(detail.workTaskStatus);
    this.detailIsReporter = detail.isReporter === true;
    this.detailCanReassign = detail.canReassign === true;
    const driverLicenseLevel = this.normalizeLicenseLevelValue(detail.driverLicenseLevel || null, detail.licenseLevelOptions || []);
    this.licenseLevelOptions = this.normalizeLicenseLevelOptions(detail.licenseLevelOptions || [], driverLicenseLevel);
    this.coverageOptions = this.normalizeCoverageOptions(detail.coverageOptions || []);
    this.riskOptions = detail.riskOptions || [];
    this.claimPlanOptions = detail.claimPlanOptions || [];
    this.itemOptions = detail.itemOptions || [];

    const defaultCoverage = this.getDefaultCoverageOption();
    let selectedCoverageId = defaultCoverage ? this.getCoverageOptionKey(defaultCoverage) : null;
    let sections: DetailedAssessmentSectionDto[] = (detail.sections || []).map(section => {
      const normalizedItems = (section.items || []).map(item => ({
        ...structuredClone(item),
        dischargeDate: this.toDateOnlyValue(item.dischargeDate),
        issueDate: this.toDateOnlyValue(item.issueDate),
      }));

      const normalizedSection: DetailedAssessmentSectionDto = {
        ...structuredClone(section),
        items: normalizedItems,
      };

      // Nếu section đã có sẵn nhưng chưa có item thì tự thêm 1 dòng nhập mặc định.
      if (normalizedSection.items.length === 0) {
        normalizedSection.items = [this.createItem(normalizedSection)];
      }

      return normalizedSection;
    });

    if (sections.length === 0) {
      sections = this.coverageOptions.map(coverage => this.createSectionFromCoverage(coverage));
      if (defaultCoverage) {
        selectedCoverageId = this.getCoverageOptionKey(defaultCoverage);
      }
    }

    this.form = {
      assessorDeptName: detail.assessorDeptName || this.claim?.processDeptName || '',
      assessorName: detail.assessorName || '',
      startDate: this.toDate(detail.startDate),
      endDate: this.toDate(detail.endDate),
      driverName: detail.driverName || '',
      driverSex: detail.driverSex || null,
      driverPhone: detail.driverPhone || '',
      driverIdNo: detail.driverIdNo || '',
      driverLicenseNo: detail.driverLicenseNo || '',
      driverLicenseEffectDate: this.toDateOnlyValue(detail.driverLicenseEffectDate),
      driverLicenseExpireDate: this.toDateOnlyValue(detail.driverLicenseExpireDate),
      driverLicenseLevel,
      carRegistryNo: detail.carRegistryNo || '',
      carRegistryEffectDate: this.toDateOnlyValue(detail.carRegistryEffectDate),
      carRegistryExpireDate: this.toDateOnlyValue(detail.carRegistryExpireDate),
      selectedCoverageId,
      sections,
      documentRows: this.mergeDocumentRowsWithConfiguredTypes((detail.documentRows || []).map(row => ({
        ...structuredClone(row),
        issueDate: this.toDateOnlyValue(row.issueDate),
      }))),
    };
  }

  private validate(): boolean {
    const errors: Record<string, string> = {};

    if (!this.form.driverName.trim()) errors['driverName'] = 'Tên người lái xe là bắt buộc.';
    if (!this.form.driverSex) errors['driverSex'] = 'Giới tính là bắt buộc.';
    if (!this.form.driverPhone.trim()) errors['driverPhone'] = 'Điện thoại là bắt buộc.';
    if (!this.form.driverLicenseLevel) errors['driverLicenseLevel'] = 'Hạng GPLX là bắt buộc.';

    for (const section of this.form.sections) {
      const isPersonSection = this.isPersonSection(section);
      section.items.forEach((item, index) => {
        if (item.attachments.some(file => file.uploading)) {
          errors[`attachments-${section.id}-${index}`] = 'Tài liệu đang tải lên, vui lòng chờ hoàn tất.';
        }
        if (!isPersonSection && !item.itemId) {
          errors[`item-${section.id}-${index}`] = 'Hạng mục là bắt buộc.';
        }
        if (isPersonSection && !item.personName?.trim()) {
          errors[`personName-${section.id}-${index}`] = 'Họ tên là bắt buộc.';
        }
        if (!item.quantity || item.quantity <= 0) {
          errors[`quantity-${section.id}-${index}`] = 'Số lượng phải lớn hơn 0.';
        }
        const dischargeDate = this.toDate(item.dischargeDate);
        const incidentDate = this.toDate(this.claim?.incidentDate);
        if (isPersonSection && dischargeDate && incidentDate && this.toDateOnly(dischargeDate) < this.toDateOnly(incidentDate)) {
          errors[`dischargeDate-${section.id}-${index}`] = 'Ngày ra viện phải lớn hơn hoặc bằng ngày xảy ra tai nạn.';
        }
        const issueDate = this.toDate(item.issueDate);
        if (issueDate && this.form.startDate && this.toDateOnly(issueDate) < this.toDateOnly(this.form.startDate)) {
          errors[`issueDate-${section.id}-${index}`] = 'Ngày cung cấp phải lớn hơn hoặc bằng ngày giám định.';
        }
      });
    }

    this.fieldErrors = errors;
    if (Object.keys(errors).length > 0) {
      this.messageService.add({ severity: 'warn', summary: 'Cảnh báo', detail: 'Vui lòng kiểm tra lại dữ liệu giám định chi tiết.' });
      return false;
    }

    return true;
  }

  private toPayload(): SaveDetailedAssessmentInput {
    return {
      driverName: this.form.driverName,
      driverSex: this.form.driverSex,
      driverPhone: this.form.driverPhone,
      driverIdNo: this.form.driverIdNo,
      driverLicenseNo: this.form.driverLicenseNo,
      driverLicenseEffectDate: this.toIsoDateOnly(this.form.driverLicenseEffectDate),
      driverLicenseExpireDate: this.toIsoDateOnly(this.form.driverLicenseExpireDate),
      driverLicenseLevel: this.form.driverLicenseLevel,
      carRegistryNo: this.form.carRegistryNo,
      carRegistryEffectDate: this.toIsoDateOnly(this.form.carRegistryEffectDate),
      carRegistryExpireDate: this.toIsoDateOnly(this.form.carRegistryExpireDate),
      sections: this.form.sections.map(section => ({
        ...section,
        items: section.items.map(item => ({
          ...item,
          itemName: this.itemOptions.find(x => x.id === item.itemId)?.name || item.itemName,
          dischargeDate: this.toIsoDateOnly(this.toDateOnlyValue(item.dischargeDate)),
          issueDate: this.toIsoDateOnly(this.toDateOnlyValue(item.issueDate)),
        })),
      })),
      documentRows: this.form.documentRows.map(row => ({
        ...row,
        issueDate: this.toIsoDateOnly(this.toDateOnlyValue(row.issueDate)),
      })),
    };
  }

  private createItem(sectionOrCoverage?: Pick<DetailedAssessmentSectionDto, 'objectTypeId' | 'objectTypeCode' | 'objectTypeName' | 'objectTypeGroup' | 'objectKind'>): DetailedAssessmentItemDto {
    const objectKind = sectionOrCoverage?.objectKind || this.getObjectKind(
      sectionOrCoverage?.objectTypeCode,
      sectionOrCoverage?.objectTypeName,
      sectionOrCoverage?.objectTypeGroup
    );
    const defaultItemId = objectKind === 'person'
      ? this.itemOptions.find(x => !sectionOrCoverage?.objectTypeId || x.objectTypeId === sectionOrCoverage.objectTypeId)?.id || null
      : null;
    return {
      id: crypto.randomUUID(),
      itemId: defaultItemId,
      itemName: null,
      personName: '',
      personIdNo: '',
      description: '',
      quantity: 1,
      riskId: null,
      claimPlanId: null,
      isGenuine: null,
      isRecovery: false,
      dischargeDate: null,
      issueDate: this.form.startDate ? new Date(this.form.startDate) : new Date(),
      attachments: [],
    };
  }

  private createSectionFromCoverage(coverage: DetailedAssessmentOptionDto): DetailedAssessmentSectionDto {
    return {
      id: crypto.randomUUID(),
      coverageId: coverage.id,
      coverageName: coverage.name,
      objectTypeId: coverage.objectTypeId || null,
      objectTypeCode: coverage.objectTypeCode || null,
      objectTypeName: coverage.objectTypeName || null,
      objectTypeGroup: coverage.objectTypeGroup || null,
      objectKind: coverage.objectKind || this.getObjectKindFromOption(coverage),
      items: [this.createItem(coverage)],
    };
  }

  private normalizeCoverageOptions(options: DetailedAssessmentOptionDto[]): DetailedAssessmentOptionDto[] {
    return options.map((option, index) => {
      const objectTypeText = option.objectTypeName || option.objectTypeCode || option.objectTypeGroup;
      return {
        ...option,
        optionKey: this.buildCoverageOptionKey(option, index),
        displayName: objectTypeText ? `${option.name} - ${objectTypeText}` : option.name,
      };
    });
  }

  private getCoverageOptionKey(option: DetailedAssessmentOptionDto): string {
    return option.optionKey || this.buildCoverageOptionKey(option, 0);
  }

  private getDefaultCoverageOption(): DetailedAssessmentOptionDto | undefined {
    return this.coverageOptions.find(x => x.type === 'main') ?? this.coverageOptions[0];
  }

  private buildCoverageOptionKey(option: DetailedAssessmentOptionDto, index: number): string {
    return [
      option.id || '',
      option.code || '',
      option.objectTypeId || '',
      option.objectTypeCode || '',
      option.objectTypeName || '',
      option.objectTypeGroup || '',
      index.toString(),
    ].join('|');
  }

  private normalizeLicenseLevelOptions(
    options: DetailedAssessmentOptionDto[],
    currentValue: string | null
  ): DetailedAssessmentOptionDto[] {
    const normalizedOptions = options
      .map(option => {
        const value = this.getLicenseLevelOptionValue(option);
        return {
          ...option,
          id: value,
          code: option.code || value,
          name: option.name || value,
        };
      })
      .filter(option => !!option.id);

    if (!currentValue) {
      return normalizedOptions;
    }

    const exists = normalizedOptions.some(option =>
      this.equalsIgnoreCase(option.id, currentValue)
      || this.equalsIgnoreCase(option.code, currentValue)
      || this.equalsIgnoreCase(option.name, currentValue)
    );
    if (exists) {
      return normalizedOptions;
    }

    return [
      { id: currentValue, code: currentValue, name: currentValue },
      ...normalizedOptions,
    ];
  }

  private normalizeLicenseLevelValue(
    value: string | null,
    options: DetailedAssessmentOptionDto[]
  ): string | null {
    const normalizedValue = value?.trim();
    if (!normalizedValue) {
      return null;
    }

    const matchedOption = options.find(option =>
      this.equalsIgnoreCase(option.id, normalizedValue)
      || this.equalsIgnoreCase(option.code, normalizedValue)
      || this.equalsIgnoreCase(option.name, normalizedValue)
    );

    return matchedOption ? this.getLicenseLevelOptionValue(matchedOption) : normalizedValue;
  }

  private getLicenseLevelOptionValue(option: DetailedAssessmentOptionDto): string {
    return (option.id || option.code || option.name || '').trim();
  }

  private equalsIgnoreCase(left?: string | null, right?: string | null): boolean {
    return (left || '').trim().toUpperCase() === (right || '').trim().toUpperCase();
  }

  private createAttachment(file: File): DetailedAssessmentAttachmentDto {
    const url = URL.createObjectURL(file);
    return {
      id: crypto.randomUUID(),
      documentId: null,
      claimDocumentId: null,
      fileName: file.name,
      url,
      thumbnailUrl: file.type.startsWith('image/') ? url : null,
      mimeType: file.type || null,
    };
  }

  private createUploadingAttachment(file: File): DetailedAssessmentAttachmentDto {
    const url = URL.createObjectURL(file);
    return {
      id: crypto.randomUUID(),
      documentId: null,
      claimDocumentId: null,
      fileName: file.name,
      url,
      thumbnailUrl: file.type.startsWith('image/') ? url : null,
      mimeType: file.type || null,
      uploading: true,
    };
  }

  private toDateOnly(value: Date): number {
    return new Date(value.getFullYear(), value.getMonth(), value.getDate()).getTime();
  }

  private includeSelectedOption(
    options: DetailedAssessmentOptionDto[],
    selectedId?: string | null,
    fallbackOptions?: DetailedAssessmentOptionDto[]
  ): DetailedAssessmentOptionDto[] {
    if (!selectedId || options.some(option => option.id === selectedId)) {
      return options;
    }

    const selectedOption = (fallbackOptions || options).find(option => option.id === selectedId);
    return selectedOption ? [selectedOption, ...options] : options;
  }

  private getSelectedOptionName(
    options: DetailedAssessmentOptionDto[],
    selectedId?: string | null,
    fallbackOptions?: DetailedAssessmentOptionDto[]
  ): string {
    if (!selectedId) {
      return '';
    }

    const selectedOption = options.find(option => option.id === selectedId)
      || (fallbackOptions || []).find(option => option.id === selectedId);

    return selectedOption?.name || '';
  }

  private getEmptyForm(): DetailedAssessmentFormData {
    return {
      assessorDeptName: '',
      assessorName: '',
      startDate: null,
      endDate: null,
      driverName: '',
      driverSex: null,
      driverPhone: '',
      driverIdNo: '',
      driverLicenseNo: '',
      driverLicenseEffectDate: null,
      driverLicenseExpireDate: null,
      driverLicenseLevel: null,
      carRegistryNo: '',
      carRegistryEffectDate: null,
      carRegistryExpireDate: null,
      selectedCoverageId: null,
      sections: [],
      documentRows: [],
    };
  }

  private showSaveError(error: any, fallbackDetail: string): void {
    this.messageService.add({
      severity: 'error',
      summary: 'Lỗi',
      detail: this.getErrorMessage(error, fallbackDetail)
    });
  }

  private getErrorMessage(error: any, fallbackDetail: string): string {
    return error?.error?.error?.message
      || error?.error?.message
      || error?.message
      || fallbackDetail;
  }

  private toWorkTaskStatus(value?: number | string | null): WorkTaskStatus | null {
    if (value == null) return null;

    if (typeof value === 'number') {
      return value in WorkTaskStatus ? (value as WorkTaskStatus) : null;
    }

    const normalized = value.trim();
    if (normalized.length === 0) return null;

    const parsedNumber = Number(normalized);
    if (!Number.isNaN(parsedNumber)) {
      return parsedNumber in WorkTaskStatus ? (parsedNumber as WorkTaskStatus) : null;
    }

    const keyLookup: Record<string, WorkTaskStatus> = {
      new: WorkTaskStatus.New,
      inprogress: WorkTaskStatus.InProgress,
      completed: WorkTaskStatus.Completed,
      accepted: WorkTaskStatus.Accepted,
      rejected: WorkTaskStatus.Rejected,
      cancelled: WorkTaskStatus.Cancelled,
      waitapprove: WorkTaskStatus.WaitApprove,
      approved: WorkTaskStatus.Approved,
      pending: WorkTaskStatus.Pending,
      return: WorkTaskStatus.Return
    };

    return keyLookup[normalized.toLowerCase()] ?? null;
  }

  private toDate(value: string | Date | null | undefined): Date | null {
    if (!value) return null;
    const date = value instanceof Date ? value : new Date(value);
    return Number.isNaN(date.getTime()) ? null : date;
  }

  private toDateOnlyValue(value: string | Date | null | undefined): Date | null {
    if (!value) return null;

    if (value instanceof Date) {
      return new Date(value.getFullYear(), value.getMonth(), value.getDate(), 12, 0, 0);
    }

    const match = /^(\d{4})-(\d{2})-(\d{2})/.exec(value);
    if (match) {
      const [, year, month, day] = match;
      return new Date(Number(year), Number(month) - 1, Number(day), 12, 0, 0);
    }

    const parsed = new Date(value);
    if (Number.isNaN(parsed.getTime())) {
      return null;
    }

    return new Date(parsed.getFullYear(), parsed.getMonth(), parsed.getDate(), 12, 0, 0);
  }

  private loadItemAttachmentDocType(): void {
    this.resDocumentTypeService.getList({ code: 'OTHER', maxResultCount: 5 }).subscribe({
      next: response => {
        const docType = (response.items || []).find(x => !!x.id && (x.code || '').toUpperCase() === 'OTHER');
        this.itemAttachmentDocTypeId = docType?.id || null;
      },
      error: () => {
        this.itemAttachmentDocTypeId = null;
      }
    });
  }

  private loadDetailedDocumentTypes(): void {
    this.getDocumentTypesByGroupCode(DetailedAssessmentTabComponent.DETAIL_DOCUMENT_GROUP_CODE)
      .subscribe({
        next: (response) => {
          const documentTypes = (response || []).filter((x) => !!x.id);
          this.configuredDocumentRows = documentTypes.map((docType, index) => ({
            id: `doc-${index + 1}`,
            code: docType.code || '',
            documentTypeId: docType.id || null,
            name: docType.name || docType.code || docType.id || '',
            complete: null,
            isCopy: null,
            note: null,
            issueDate: null,
            attachments: []
          }));
          this.rebuildDetailedDocumentTypeMap();
          this.form.documentRows = this.mergeDocumentRowsWithConfiguredTypes(this.form.documentRows);
        },
        error: () => {
          // Keep fallback names and null ids; upload handler will warn if missing.
        }
      });
  }

  private getDocumentTypesByGroupCode(documentGroupCode: string) {
    return this.restService.request<any, ResDocumentTypeDto[]>({
      method: 'GET',
      url: `/api/master/document-types/by-document-group/${documentGroupCode}`,
    }, { apiName: 'Master' });
  }

  private rebuildDetailedDocumentTypeMap(): void {
    this.detailedDocumentTypeByRowId = this.configuredDocumentRows.reduce<Record<string, { code: string; docTypeId: string | null; name: string }>>((acc, row) => {
      acc[row.id] = {
        code: row.code || '',
        docTypeId: row.documentTypeId || null,
        name: row.name
      };
      return acc;
    }, {});
  }

  private mergeDocumentRowsWithConfiguredTypes(rows: DetailedAssessmentDocumentRowDto[]): DetailedAssessmentDocumentRowDto[] {
    if (this.configuredDocumentRows.length === 0) {
      return rows;
    }

    return this.configuredDocumentRows.map(configuredRow => {
      const configuredCode = (configuredRow.code || '').toUpperCase();
      const matched = rows.find(row =>
        (!!configuredCode && (row.code || '').toUpperCase() === configuredCode)
        || configuredRow.id === row.id
        || configuredRow.name === row.name
      );

      return {
        ...configuredRow,
        ...matched,
        id: configuredRow.id,
        code: configuredRow.code,
        documentTypeId: configuredRow.documentTypeId,
        name: configuredRow.name,
        attachments: matched?.attachments || []
      };
    });
  }

  private getPreviewFileType(
    fileName?: string | null,
    url?: string | null,
    mimeType?: string | null
  ): 'image' | 'pdf' | 'other' {
    const normalizedMimeType = (mimeType || '').toLowerCase();
    if (normalizedMimeType.startsWith('image/')) {
      return 'image';
    }
    if (normalizedMimeType === 'application/pdf') {
      return 'pdf';
    }

    const lowerName = (fileName || '').toLowerCase();
    const lowerUrl = (url || '').toLowerCase();
    if (/\.(png|jpg|jpeg|gif|bmp|webp|svg)$/.test(lowerName) || /\.(png|jpg|jpeg|gif|bmp|webp|svg)(\?|$)/.test(lowerUrl)) {
      return 'image';
    }
    if (lowerName.endsWith('.pdf') || /\.pdf(\?|$)/.test(lowerUrl)) {
      return 'pdf';
    }

    return 'other';
  }

  private resolveAttachmentUrl(file: DetailedAssessmentAttachmentDto, onResolved: (url: string) => void): void {
    if (file.url) {
      onResolved(file.url);
      return;
    }

    if (!file.documentId) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Thiếu dữ liệu tệp',
        detail: 'Không tìm thấy URL hoặc documentId của tệp.'
      });
      return;
    }

    this.resDocumentService.getSingleFile(file.documentId).subscribe({
      next: (res) => {
        const resolvedUrl = res.url || '';
        if (!resolvedUrl) {
          this.messageService.add({
            severity: 'warn',
            summary: 'Không mở được tệp',
            detail: 'Không nhận được URL tệp từ hệ thống.'
          });
          return;
        }

        this.form.documentRows = this.form.documentRows.map((row) => ({
          ...row,
          attachments: row.attachments.map((attached) =>
            attached.id === file.id
              ? { ...attached, url: resolvedUrl, thumbnailUrl: attached.thumbnailUrl || null, mimeType: res.mimeType || attached.mimeType || null }
              : attached
          )
        }));
        this.form.sections = this.form.sections.map((section) => ({
          ...section,
          items: section.items.map((item) => ({
            ...item,
            attachments: item.attachments.map((attached) =>
              attached.id === file.id
                ? { ...attached, url: resolvedUrl, thumbnailUrl: attached.thumbnailUrl || null, mimeType: res.mimeType || attached.mimeType || null }
                : attached
            )
          }))
        }));
        onResolved(resolvedUrl);
      },
      error: () => {
        this.messageService.add({
          severity: 'error',
          summary: 'Không mở được tệp',
          detail: 'Lấy thông tin tệp thất bại.'
        });
      }
    });
  }

  private toIso(value: Date | null): string | null {
    if (!value) {
      return null;
    }

    const year = value.getFullYear();
    const month = `${value.getMonth() + 1}`.padStart(2, '0');
    const day = `${value.getDate()}`.padStart(2, '0');
    const hours = `${value.getHours()}`.padStart(2, '0');
    const minutes = `${value.getMinutes()}`.padStart(2, '0');
    const seconds = `${value.getSeconds()}`.padStart(2, '0');

    return `${year}-${month}-${day}T${hours}:${minutes}:${seconds}`;
  }

  private toIsoDateOnly(value: Date | null): string | null {
    if (!value) {
      return null;
    }

    const normalized = new Date(value.getFullYear(), value.getMonth(), value.getDate(), 12, 0, 0);
    return this.toIso(normalized);
  }

  private getObjectKindFromOption(option: DetailedAssessmentOptionDto | null | undefined): string {
    return this.getObjectKind(option?.objectTypeCode, option?.objectTypeName, option?.objectTypeGroup);
  }

  private getObjectKind(objectTypeCode?: string | null, objectTypeName?: string | null, objectTypeGroup?: string | null): string {
    const source = `${objectTypeCode || ''} ${objectTypeName || ''} ${objectTypeGroup || ''}`
      .toLowerCase()
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '')
      .replace(/đ/g, 'd');
    if (source.includes('nguoi') || source.includes('person') || source.includes('human') || source.includes('con nguoi')) {
      return 'person';
    }

    if (source.includes('xe') || source.includes('oto') || source.includes('motor') || source.includes('car') || source.includes('vehicle')) {
      return 'vehicle';
    }

    return 'asset';
  }

  private extractSelectedFiles(event: Event | File[]): { input: HTMLInputElement | null; files: File[] } {
    if (Array.isArray(event)) {
      return { input: null, files: event };
    }

    const input = event.target as HTMLInputElement;
    return {
      input,
      files: Array.from(input.files || []),
    };
  }
}
