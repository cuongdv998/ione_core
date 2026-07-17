import { CommonModule } from '@angular/common';
import { RestService } from '@abp/ng.core';
import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { ButtonModule } from 'primeng/button';
import { CheckboxModule } from 'primeng/checkbox';
import { DatePickerModule } from 'primeng/datepicker';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { TextareaModule } from 'primeng/textarea';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { DialogModule } from 'primeng/dialog';
import { ToastModule } from 'primeng/toast';
import { TagModule } from 'primeng/tag';

import { LocalizationService } from '@/core/services/localization.service';
import type { ClaimDetailDto } from '@/proxy/claim/claims/models';
import { WorkTaskStatus } from '@/proxy/work-tasks/work-task-status.enum';
import { ResPartnerService } from '@/proxy/partner/controllers/res-partner.service';
import { AdminConfigService } from '@/proxy/master/controllers/admin-config.service';
import { ResDocumentService } from '@/proxy/master/controllers/res-document.service';
import { ResDocumentTypeService } from '@/proxy/master/controllers/res-document-type.service';
import type { ResDocumentTypeDto } from '@/proxy/master/res-document-types/models';
import {
  OnsiteAssessmentDetailDto,
  OnsiteAssessmentDetailService,
  SaveOnsiteAssessmentInput,
  SaveOnsiteImageInput,
  SaveOnsiteDocumentInput,
} from '@/pages/claim/onsite-assessment-detail/onsite-assessment-detail.service';

interface OnsitePhotoItem {
  id: string;
  claimDocumentId?: string | null;
  documentId?: string | null;
  documentTypeId?: string | null;
  type: string | null;
  fileType?: 'image' | 'pdf' | 'other';
  name: string;
  previewUrl: string;
  sourceUrl?: string;
  uploadedAt: Date;
  uploader: string;
  uploading?: boolean;
}

interface OnsiteProfileDocumentItem {
  id: string;
  name: string;
  complete: boolean | null;
  isCopy: boolean | null;
  note: string;
  issueDate: Date | null;
  fileName: string | null;
  claimDocumentId?: string | null;
  documentId?: string | null;
  documentTypeId?: string | null;
  url?: string | null;
  thumbnailUrl?: string | null;
  attachments: OnsiteProfileFileItem[];
}

interface OnsiteProfileFileItem {
  id: string;
  name: string;
  claimDocumentId?: string | null;
  documentId?: string | null;
  documentTypeId?: string | null;
  url?: string | null;
  thumbnailUrl?: string | null;
  mimeType?: string | null;
  uploadedAt?: Date | null;
  uploading?: boolean;
}

interface OnsiteAssessmentFormData {
  driverName: string;
  driverSex: 'M' | 'F' | null;
  driverPhone: string;
  driverIdNo: string;
  driverLicenseNo: string;
  driverLicenseEffectDate: Date | null;
  driverLicenseExpireDate: Date | null;
  driverLicenseLevel: string | null;
  carRegistryNo: string;
  carRegistryEffectDate: Date | null;
  carRegistryExpireDate: Date | null;
  lossPositions: string[];
  hasLossThirdParty: boolean;
  witnessTestimony: string;
  causeDescription: string;
  description: string;
  locationDescription: string;
  damageDescription: string;
  partiesInvolvedDescription: string;
  addressPlan: string;
  customerRecommendation: string;
  otherDescription: string;
  garageId: string | null;
  issueDate: Date | null;
  selectedImageType: string | null;
}

@Component({
  selector: 'app-claim-onsite-assessment-tab',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ButtonModule,
    CheckboxModule,
    DatePickerModule,
    InputTextModule,
    SelectModule,
    TableModule,
    TextareaModule,
    ConfirmDialogModule,
    DialogModule,
    ToastModule,
    TagModule,
  ],
  templateUrl: './claim-onsite-assessment-tab.component.html',
  styleUrl: './claim-onsite-assessment-tab.component.scss',
  providers: [ConfirmationService, MessageService]
})
export class ClaimOnsiteAssessmentTabComponent implements OnChanges {
  private readonly confirmationService = inject(ConfirmationService);
  private readonly localizationService = inject(LocalizationService);
  private static readonly PROFILE_DOCUMENT_GROUP_CODE = 'CLAIM_ADJUST_ONSITE_PROFILE';

  @Input() claim: ClaimDetailDto | null = null;
  @Input() claimId: string | null = null;
  @Input() assessorDeptName: string | null = null;
  @Input() assessorName: string | null = null;
  @Input() startDate: string | Date | null = null;
  @Input() endDate: string | Date | null = null;
  @Input() workTaskId: string | null = null;
  @Input() workTaskStatus: WorkTaskStatus | null = null;
  @Input() refreshVersion = 0;
  @Input() autoCompleteRequest = false;
  @Input() forceReadOnly = false;
  @Input() showOnsiteActions = true;
  @Input() useClaimScopedDetail = false;
  @Output() closeRequested = new EventEmitter<void>();
  @Output() rejectRequested = new EventEmitter<void>();
  @Output() acceptRequested = new EventEmitter<void>();
  @Output() transferRequested = new EventEmitter<void>();
  @Output() reassignRequested = new EventEmitter<void>();

  onsiteForm: OnsiteAssessmentFormData = this.getEmptyOnsiteForm();
  driverSexOptions: Array<{ label: string; value: 'M' | 'F' }> = [];
  driverLicenseLevelOptions: Array<{ label: string; value: string }> = [];
  lossPositionOptions: Array<{ label: string; value: string }> = [];
  garageOptions: Array<{ label: string; value: string }> = [];
  loadingGarages = false;
  loadingOnsiteImageType = false;
  imageTypeOptions: Array<{ label: string; value: string }> = [];
  onsiteImageTypeRequired = false;
  onsitePhotos: OnsitePhotoItem[] = [];
  profileDocuments: OnsiteProfileDocumentItem[] = [];
  fieldErrors: Record<string, string> = {};
  saving = false;
  savingDraft = false;
  completing = false;
  downloadingMinutes = false;
  onsiteImageUploadDialogVisible = false;
  selectedOnsitePhotoId: string | null = null;
  previewImageVisible = false;
  previewImageUrl: string | null = null;
  previewImageResourceUrl: SafeResourceUrl | null = null;
  previewImageName = '';
  previewImageType: 'image' | 'pdf' | 'other' = 'other';
  profileUploadDialogVisible = false;
  activeProfileDocumentId: string | null = null;
  profileFilePreviewVisible = false;
  profileFilePreviewUrl: string | null = null;
  profileFilePreviewResourceUrl: SafeResourceUrl | null = null;
  profileFilePreviewName = '';
  profileFilePreviewType: 'image' | 'pdf' | 'other' = 'other';
  selectedProfileUploadFileId: string | null = null;
  private profileDocumentHintsByDocumentId: Record<string, string> = {};
  private initializedFromClaim = false;
  private loadedDetailByWorkTaskId: string | null = null;
  private pendingDetailWorkTaskId: string | null = null;
  private profileDocTypesReady = false;
  private autoCompleteHandledForWorkTaskId: string | null = null;
  assessmentStartDate: Date | null = null;
  assessmentEndDate: Date | null = null;
  detailWorkTaskStatus: WorkTaskStatus | null = null;
  detailIsReporter = false;
  readonly WorkTaskStatus = WorkTaskStatus;
  private profileDocRowIds: string[] = [];
  private onsiteProfileDocTypeByRowId: Record<string, { code: string; docTypeId: string | null; name: string }> = {};

  constructor(
    private messageService: MessageService,
    private onsiteAssessmentService: OnsiteAssessmentDetailService,
    private partnerService: ResPartnerService,
    private adminConfigService: AdminConfigService,
    private resDocumentService: ResDocumentService,
    private resDocumentTypeService: ResDocumentTypeService,
    private restService: RestService,
    private sanitizer: DomSanitizer
  ) {
    this.initOnsiteOptions();
    this.loadLossPositionOptions();
    this.loadGarageOptions();
    this.loadOnsiteDocumentTypes();
    this.profileDocuments = this.getDefaultProfileDocuments();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['claim'] && this.claim && !this.initializedFromClaim) {
      this.prefillDriverInfoFromClaim();
      this.initializedFromClaim = true;
    }

    if (this.useClaimScopedDetail) {
      if (changes['claimId'] && this.claimId) {
        if (!this.profileDocTypesReady) {
          this.pendingDetailWorkTaskId = this.claimId;
        } else {
          this.loadOnsiteAssessmentDetailByClaimId(this.claimId);
        }
      }
    } else if (changes['workTaskId'] && this.workTaskId && this.loadedDetailByWorkTaskId !== this.workTaskId) {
      if (!this.profileDocTypesReady) {
        this.pendingDetailWorkTaskId = this.workTaskId;
      } else {
        this.loadOnsiteAssessmentDetail(this.workTaskId);
      }
    }

    if (changes['refreshVersion'] && !changes['refreshVersion'].firstChange) {
      this.reloadCurrentDetail();
    }

    this.tryAutoComplete();
  }

  formatDateTime(value: string | Date | undefined | null): string {
    if (!value) return '-';
    const d = new Date(value);
    if (isNaN(d.getTime())) return String(value);
    const date = d.toLocaleDateString('vi-VN');
    const h = String(d.getHours()).padStart(2, '0');
    const mi = String(d.getMinutes()).padStart(2, '0');
    return `${h}h${mi}, ${date}`;
  }

  onOnsiteImageUpload(event: Event | File[], imageDocTypeId?: string): void {
    if (!this.isEditable) return;
    if (this.loadingOnsiteImageType) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Cảnh báo',
        detail: 'Đang tải danh mục loại ảnh từ Master, vui lòng thử lại sau.'
      });
      return;
    }
    const { input, files } = this.extractSelectedFiles(event);
    if (files.length === 0) return;
    const selectedImageDocTypeId = imageDocTypeId || this.onsiteForm.selectedImageType;
    if (!selectedImageDocTypeId) {
      this.onsiteImageTypeRequired = true;
      this.messageService.add({
        severity: 'warn',
        summary: 'Cảnh báo',
        detail: 'Chọn loại ảnh trước khi tải lên.'
      });
      if (input) {
        input.value = '';
      }
      return;
    }
    if (!selectedImageDocTypeId) {
      this.messageService.add({
        severity: 'error',
        summary: 'Lỗi',
        detail: 'Chưa cấu hình loại tài liệu ảnh hiện trường (CAR_ASSESSMENT_IMAGE).'
      });
      if (input) {
        input.value = '';
      }
      return;
    }

    const uploader = this.assessorName || this.claim?.processEmpName || 'N/A';
    const imageTypeLabel = this.getImageTypeLabel(selectedImageDocTypeId);
    const uploadFiles = files.filter(file => file.type.startsWith('image/') || file.type === 'application/pdf');
    if (uploadFiles.length === 0) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Cảnh báo',
        detail: 'Chỉ hỗ trợ tải lên ảnh hoặc file PDF.'
      });
      if (input) {
        input.value = '';
      }
      return;
    }
    const placeholders = uploadFiles.map((file) => {
      const tempId = crypto.randomUUID();
      const tempUrl = URL.createObjectURL(file);
      return {
        id: tempId,
        documentTypeId: selectedImageDocTypeId,
        type: imageTypeLabel,
        fileType: this.getPreviewFileType(file.name, tempUrl, file.type),
        name: file.name,
        previewUrl: tempUrl,
        sourceUrl: tempUrl,
        uploadedAt: new Date(),
        uploader,
        uploading: true
      } as OnsitePhotoItem;
    });

    this.onsitePhotos = [...this.onsitePhotos, ...placeholders];

    this.resDocumentService
      .uploadMultipleFiles(selectedImageDocTypeId, 'CAR_ASSESSMENT_IMAGE', uploadFiles)
      .subscribe({
        next: (uploadedDocs) => {
          const uploadedPhotos = uploadedDocs.map((resDoc, index) => {
            const placeholder = placeholders[index];
            return {
              ...placeholder,
              documentId: resDoc.id || null,
              documentTypeId: selectedImageDocTypeId,
              fileType: this.getPreviewFileType(
                resDoc.fileName || placeholder.name,
                resDoc.url || resDoc.thumbnailUrl || placeholder.sourceUrl || placeholder.previewUrl,
                resDoc.mimeType
              ),
              previewUrl: resDoc.thumbnailUrl || resDoc.url || placeholder.previewUrl,
              sourceUrl: resDoc.url || placeholder.sourceUrl,
              name: resDoc.fileName || placeholder.name,
              uploading: false
            } as OnsitePhotoItem;
          });

          this.onsitePhotos = this.onsitePhotos.filter(photo => !placeholders.some(x => x.id === photo.id));
          this.onsitePhotos = [...this.onsitePhotos, ...uploadedPhotos];
          if (uploadedPhotos[0]) {
            this.selectOnsitePhoto(uploadedPhotos[0].id);
          }
        },
        error: () => {
          this.onsitePhotos = this.onsitePhotos.filter(photo => !placeholders.some(x => x.id === photo.id));
        }
      });
    if (input) {
      input.value = '';
    }
  }

  triggerOnsiteImageUpload(input: HTMLInputElement, imageDocTypeId?: string): void {
    if (!this.isEditable || this.loadingOnsiteImageType) return;
    if (!imageDocTypeId && !this.onsiteForm.selectedImageType) {
      this.onsiteImageTypeRequired = true;
      return;
    }

    this.onsiteImageTypeRequired = false;
    input.click();
  }

  removeOnsitePhoto(id: string): void {
    if (!this.isEditable) return;
    this.onsitePhotos = this.onsitePhotos.filter((x) => x.id !== id);
    if (this.selectedOnsitePhotoId === id) {
      this.selectedOnsitePhotoId = this.onsitePhotos[0]?.id || null;
    }
  }

  openOnsiteImageUploadDialog(): void {
    this.onsiteImageUploadDialogVisible = true;
    this.selectedOnsitePhotoId = this.onsitePhotos[0]?.id || null;
  }

  closeOnsiteImageUploadDialog(): void {
    this.onsiteImageUploadDialogVisible = false;
  }

  selectOnsitePhoto(photoId: string): void {
    this.selectedOnsitePhotoId = photoId;
  }

  onImageTypeChanged(): void {
    if (this.onsiteForm.selectedImageType) {
      this.onsiteImageTypeRequired = false;
    }
    if (this.onsiteImageUploadDialogVisible) {
      this.selectedOnsitePhotoId = this.onsitePhotos[0]?.id || null;
    }
  }

  get filteredOnsitePhotos(): OnsitePhotoItem[] {
    const selectedType = this.onsiteForm.selectedImageType;
    if (!selectedType) {
      return this.onsitePhotos;
    }

    return this.onsitePhotos.filter(photo => photo.documentTypeId === selectedType);
  }

  getImageTypeLabel(imageDocTypeId: string | null | undefined): string | null {
    if (!imageDocTypeId) return null;
    return this.imageTypeOptions.find(x => x.value === imageDocTypeId)?.label || imageDocTypeId;
  }

  getSelectedOnsitePhoto(): OnsitePhotoItem | null {
    if (!this.selectedOnsitePhotoId) return null;
    return this.onsitePhotos.find((photo) => photo.id === this.selectedOnsitePhotoId) || null;
  }

  openOnsitePhoto(photo: OnsitePhotoItem): void {
    const url = photo.sourceUrl || photo.previewUrl;
    if (!url) {
      return;
    }
    const fileType = photo.fileType || this.getPreviewFileType(photo.name, url);
    this.previewImageUrl = url;
    this.previewImageResourceUrl = null;
    this.previewImageName = photo.name;
    this.previewImageType = fileType;
    if (fileType === 'pdf') {
      this.previewImageResourceUrl = this.sanitizer.bypassSecurityTrustResourceUrl(this.toInlinePreviewUrl(url));
    }
    this.previewImageVisible = true;
  }

  closePreviewImage(): void {
    this.previewImageVisible = false;
    this.previewImageUrl = null;
    this.previewImageResourceUrl = null;
    this.previewImageName = '';
    this.previewImageType = 'other';
  }

  toggleThirdPartyLoss(): void {
    if (!this.isEditable) return;
    this.onsiteForm.hasLossThirdParty = !this.onsiteForm.hasLossThirdParty;
  }

  onProfileFileUpload(event: Event | File[]): void {
    if (!this.isEditable) return;
    if (this.loadingOnsiteImageType) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Cảnh báo',
        detail: 'Đang tải danh mục tài liệu từ Master, vui lòng thử lại sau.'
      });
      return;
    }
    const { input, files } = this.extractSelectedFiles(event);
    if (files.length === 0) return;
    const docId = this.activeProfileDocumentId;
    const profileDocType = docId ? this.onsiteProfileDocTypeByRowId[docId] : null;
    const profileDocTypeId = profileDocType?.docTypeId || null;
    if (!profileDocTypeId) {
      this.messageService.add({
        severity: 'error',
        summary: 'Lỗi',
        detail: `Chưa cấu hình loại tài liệu hồ sơ GDHT cho dòng ${docId || 'không xác định'}.`
      });
      if (input) {
        input.value = '';
      }
      return;
    }
    if (!docId) {
      if (input) {
        input.value = '';
      }
      return;
    }
    const uploadFiles = files;
    const placeholders = uploadFiles.map((file) => {
      const tempUrl = URL.createObjectURL(file);
      return {
        id: crypto.randomUUID(),
        name: file.name,
        url: tempUrl,
        thumbnailUrl: file.type.startsWith('image/') ? tempUrl : null,
        mimeType: file.type || null,
        uploadedAt: new Date(),
        uploading: true
      };
    });

    this.profileDocuments = this.profileDocuments.map((x) =>
      x.id === docId
        ? {
            ...x,
            attachments: [...x.attachments, ...placeholders]
          }
        : x
    );

    this.resDocumentService
      .uploadMultipleFiles(profileDocTypeId, profileDocType?.code || 'CLAIM_ADJUST_ONSITE_PROFILE', uploadFiles)
      .subscribe({
        next: (uploadedDocs) => {
          this.profileDocuments = this.profileDocuments.map((x) => {
            if (x.id !== docId) {
              return x;
            }

            const mappedAttachments = uploadedDocs.map((resDoc, index) => {
              const placeholder = placeholders[index];
              const sourceFile = uploadFiles[index];
              if (resDoc.id) {
                this.profileDocumentHintsByDocumentId[resDoc.id] = docId;
              }
              return {
                ...placeholder,
                name: resDoc.fileName || sourceFile?.name || placeholder.name,
                documentId: resDoc.id || null,
                documentTypeId: profileDocTypeId,
                url: resDoc.url || null,
                thumbnailUrl: resDoc.thumbnailUrl || null,
                mimeType: resDoc.mimeType || sourceFile?.type || null,
                uploadedAt: placeholder.uploadedAt || new Date(),
                uploading: false
              };
            });

            const remainingAttachments = x.attachments.filter(
              (attached) => !placeholders.some((placeholder) => placeholder.id === attached.id)
            );
            const lastUploaded = uploadedDocs[uploadedDocs.length - 1];

            return {
              ...x,
              fileName: lastUploaded?.fileName || x.fileName,
              documentId: lastUploaded?.id || x.documentId,
              url: lastUploaded?.url || x.url,
              thumbnailUrl: lastUploaded?.thumbnailUrl || x.thumbnailUrl,
              attachments: [...remainingAttachments, ...mappedAttachments]
            };
          });
          const uploadedAttachmentIds = new Set<string>(placeholders.map((placeholder) => placeholder.id));
          const nextPreviewFile = this.getActiveProfileDocument()?.attachments.find((file) =>
            uploadedAttachmentIds.has(file.id)
          ) || this.getActiveProfileDocument()?.attachments[0];
          if (nextPreviewFile) {
            this.selectProfileUploadFile(nextPreviewFile);
          }
        },
        error: () => {
          this.profileDocuments = this.profileDocuments.map((x) =>
            x.id === docId
              ? {
                  ...x,
                  attachments: x.attachments.filter(
                    (attached) => !placeholders.some((placeholder) => placeholder.id === attached.id)
                  )
                }
              : x
          );
        }
      });
    if (input) {
      input.value = '';
    }
  }

  openProfileUploadDialog(docId: string): void {
    this.activeProfileDocumentId = docId;
    this.profileUploadDialogVisible = true;
    this.selectedProfileUploadFileId = null;
    this.resetInlineProfilePreview();
    const firstFile = this.getActiveProfileDocument()?.attachments[0];
    if (firstFile) {
      this.selectProfileUploadFile(firstFile);
    }
  }

  closeProfileUploadDialog(): void {
    this.profileUploadDialogVisible = false;
    this.activeProfileDocumentId = null;
    this.selectedProfileUploadFileId = null;
    this.resetInlineProfilePreview();
  }

  getActiveProfileDocument(): OnsiteProfileDocumentItem | null {
    if (!this.activeProfileDocumentId) return null;
    return this.profileDocuments.find((x) => x.id === this.activeProfileDocumentId) || null;
  }

  selectProfileUploadFile(file: OnsiteProfileFileItem): void {
    this.selectedProfileUploadFileId = file.id;
    this.resolveProfileFileUrl(file, ({ url, mimeType }) => {
      const fileType = this.getPreviewFileType(file.name, url, mimeType || file.mimeType);

      this.profileFilePreviewName = file.name;
      this.profileFilePreviewUrl = null;
      this.profileFilePreviewResourceUrl = null;

      if (fileType === 'image') {
        this.profileFilePreviewType = 'image';
        this.profileFilePreviewUrl = url;
        return;
      }

      if (fileType === 'pdf') {
        this.profileFilePreviewType = 'pdf';
        this.profileFilePreviewResourceUrl = this.sanitizer.bypassSecurityTrustResourceUrl(this.toInlinePreviewUrl(url));
        return;
      }

      this.profileFilePreviewType = 'other';
      this.profileFilePreviewUrl = url;
    });
  }

  isSelectedProfileUploadFile(file: OnsiteProfileFileItem): boolean {
    return this.selectedProfileUploadFileId === file.id;
  }

  viewProfileDocumentFile(file: OnsiteProfileFileItem): void {
    this.resolveProfileFileUrl(file, ({ url, mimeType }) => {
      const fileType = this.getPreviewFileType(file.name, url, mimeType || file.mimeType);

      this.profileFilePreviewName = file.name;
      this.profileFilePreviewUrl = null;
      this.profileFilePreviewResourceUrl = null;

      if (fileType === 'image') {
        this.profileFilePreviewType = 'image';
        this.profileFilePreviewUrl = url;
        this.profileFilePreviewVisible = true;
        return;
      }

      if (fileType === 'pdf') {
        this.profileFilePreviewType = 'pdf';
        this.profileFilePreviewResourceUrl = this.sanitizer.bypassSecurityTrustResourceUrl(this.toInlinePreviewUrl(url));
        this.profileFilePreviewVisible = true;
        return;
      }

      this.profileFilePreviewType = 'other';
      this.profileFilePreviewVisible = true;
    });
  }

  downloadProfileDocumentFile(file: OnsiteProfileFileItem): void {
    this.resolveProfileFileUrl(file, ({ url }) => {
      const a = document.createElement('a');
      a.href = url;
      a.download = file.name || 'tai-lieu';
      a.target = '_blank';
      a.rel = 'noopener';
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
    });
  }

  downloadOnsitePhoto(photo: OnsitePhotoItem): void {
    const url = photo.sourceUrl || photo.previewUrl;
    if (!url || photo.uploading) return;

    const a = document.createElement('a');
    a.href = url;
    a.download = photo.name || 'anh-hien-truong';
    a.target = '_blank';
    a.rel = 'noopener';
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
  }

  removeProfileDocumentFile(file: OnsiteProfileFileItem): void {
    if (file.uploading) return;
    if (!this.activeProfileDocumentId) return;

    if (!file.documentId) {
      this.removeProfileFileFromActiveDocument(file);
      return;
    }

    if (!this.workTaskId) {
      this.messageService.add({
        severity: 'error',
        summary: 'Không xoá được tệp',
        detail: 'Không tìm thấy workTaskId để xoá tệp.'
      });
      return;
    }

    this.onsiteAssessmentService.removeProfileFile(this.workTaskId, file.documentId).subscribe({
      next: () => {
        this.removeProfileFileFromActiveDocument(file);
      },
      error: () => {
        this.messageService.add({
          severity: 'error',
          summary: 'Không xoá được tệp',
          detail: 'Xoá tệp trên hệ thống thất bại. Vui lòng thử lại.'
        });
      }
    });
  }

  private removeProfileFileFromActiveDocument(file: OnsiteProfileFileItem): void {
    if (!this.activeProfileDocumentId) return;

    this.profileDocuments = this.profileDocuments.map((doc) => {
      if (doc.id !== this.activeProfileDocumentId) {
        return doc;
      }

      const attachments = doc.attachments.filter((item) => item.id !== file.id);
      if (attachments.length === 0) {
        return {
          ...doc,
          attachments,
          claimDocumentId: null,
          fileName: null,
          documentId: null,
          url: null,
          thumbnailUrl: null
        };
      }

      const fallback = attachments[attachments.length - 1];
      return {
        ...doc,
        attachments,
        fileName: fallback.name || doc.fileName,
        documentId: fallback.documentId || doc.documentId,
        url: fallback.url || doc.url,
        thumbnailUrl: fallback.thumbnailUrl || doc.thumbnailUrl
      };
    });

    if (file.documentId) {
      delete this.profileDocumentHintsByDocumentId[file.documentId];
    }

    if (this.selectedProfileUploadFileId === file.id) {
      const nextFile = this.getActiveProfileDocument()?.attachments[0] || null;
      if (nextFile) {
        this.selectProfileUploadFile(nextFile);
      } else {
        this.selectedProfileUploadFileId = null;
        this.resetInlineProfilePreview();
      }
    }
  }

  private resolveProfileFileUrl(
    file: OnsiteProfileFileItem,
    onResolved: (data: { url: string; mimeType?: string | null }) => void
  ): void {
    const existingUrl = file.url || file.thumbnailUrl;
    if (existingUrl && file.mimeType) {
      onResolved({ url: existingUrl, mimeType: file.mimeType });
      return;
    }

    if (!file.documentId) {
      if (existingUrl) {
        onResolved({ url: existingUrl, mimeType: file.mimeType || null });
        return;
      }
      this.messageService.add({
        severity: 'warn',
        summary: 'Thiếu dữ liệu tệp',
        detail: 'Không tìm thấy URL hoặc documentId của tệp.'
      });
      return;
    }
    this.resDocumentService.getSingleFile(file.documentId).subscribe({
      next: (res) => {
        const resolvedUrl = res.url || existingUrl || '';
        const resolvedMimeType = res.mimeType || file.mimeType || null;
        if (!resolvedUrl) {
          this.messageService.add({
            severity: 'warn',
            summary: 'Không mở được tệp',
            detail: 'Không nhận được URL tệp từ hệ thống.'
          });
          return;
        }
        this.profileDocuments = this.profileDocuments.map((doc) => ({
          ...doc,
          attachments: doc.attachments.map((attached) =>
            attached.id === file.id ? { ...attached, url: resolvedUrl, mimeType: resolvedMimeType } : attached
          )
        }));
        onResolved({ url: resolvedUrl, mimeType: resolvedMimeType });
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

  closeProfileFilePreview(): void {
    this.profileFilePreviewVisible = false;
    this.resetInlineProfilePreview();
  }

  private resetInlineProfilePreview(): void {
    this.profileFilePreviewName = '';
    this.profileFilePreviewType = 'other';
    this.profileFilePreviewUrl = null;
    this.profileFilePreviewResourceUrl = null;
  }

  getOnsitePhotoPreviewType(photo: OnsitePhotoItem): 'image' | 'pdf' | 'other' {
    return photo.fileType || this.getPreviewFileType(photo.name, photo.sourceUrl || photo.previewUrl);
  }

  getProfileFilePreviewType(file: OnsiteProfileFileItem): 'image' | 'pdf' | 'other' {
    return this.getPreviewFileType(file.name, file.url || file.thumbnailUrl, file.mimeType);
  }

  getSafeResourceUrl(url?: string | null): SafeResourceUrl | null {
    if (!url) return null;
    return this.sanitizer.bypassSecurityTrustResourceUrl(this.toInlinePreviewUrl(url));
  }

  private toInlinePreviewUrl(url: string): string {
    if (!url) return url;
    return url.includes('?') ? `${url}&inline=true` : `${url}?inline=true`;
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

  getNeedSupplement(doc: OnsiteProfileDocumentItem): boolean {
    return doc.complete === false;
  }

  downloadOnsiteMinutes(): void {
    if (!this.workTaskId) {
      this.messageService.add({
        severity: 'error',
        summary: 'Lỗi',
        detail: 'Không tìm thấy workTaskId để tải biên bản.'
      });
      return;
    }
    this.downloadingMinutes = true;
    this.messageService.add({
      severity: 'info',
      summary: 'Thông tin',
      detail: 'Sẵn sàng tích hợp API sinh biên bản CLAIM_ASSESSMENT_ONSITE_DOCUMENT.'
    });
    this.downloadingMinutes = false;
  }

  completeOnsiteAssessment(): void {
    if (!this.isEditable) return;
    if (this.hasPendingUploads) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Cảnh báo',
        detail: 'Đang tải tệp lên, vui lòng chờ hoàn tất trước khi hoàn thành.'
      });
      return;
    }
    if (!this.validateForm()) return;
    if (!this.workTaskId) {
      this.messageService.add({
        severity: 'error',
        summary: 'Lỗi',
        detail: 'Không tìm thấy workTaskId để lưu dữ liệu.'
      });
      return;
    }

    this.confirmationService.confirm({
      message: 'Bạn có muốn hoàn thành giám định hiện trường không?',
      header: 'Hoàn thành GĐHT',
      icon: 'pi pi-check-circle',
      accept: () => this.saveAndAssignOnsiteAssessment()
    });
  }

  saveOnsiteAssessment(): void {
    if (!this.isEditable) return;
    if (this.hasPendingUploads) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Cảnh báo',
        detail: 'Đang tải tệp lên, vui lòng chờ hoàn tất trước khi lưu.'
      });
      return;
    }
    if (!this.validateForm()) return;
    if (!this.workTaskId) {
      this.messageService.add({
        severity: 'error',
        summary: 'Lỗi',
        detail: 'Không tìm thấy workTaskId để lưu dữ liệu.'
      });
      return;
    }

    this.savingDraft = true;
    this.saving = true;
    this.onsiteAssessmentService.save(this.workTaskId, this.toSaveInput()).subscribe({
      next: () => {
        this.refreshDetailAfterSuccess('Đã lưu thông tin giám định hiện trường.');
      },
      error: () => {
        this.savingDraft = false;
        this.saving = false;
      }
    });
  }

  saveAndAssignOnsiteAssessment(): void {
    if (!this.isEditable) return;
    if (this.hasPendingUploads) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Cảnh báo',
        detail: 'Đang tải tệp lên, vui lòng chờ hoàn tất trước khi lưu.'
      });
      return;
    }
    if (!this.validateForm()) return;
    if (!this.workTaskId) {
      this.messageService.add({
        severity: 'error',
        summary: 'Lỗi',
        detail: 'Không tìm thấy workTaskId để lưu dữ liệu.'
      });
      return;
    }

    this.completing = true;
    this.saving = true;
    this.onsiteAssessmentService.saveAndAssign(this.workTaskId, this.toSaveInput()).subscribe({
      next: () => {
        this.refreshDetailAfterSuccess('Đã hoàn thành giám định hiện trường.');
      },
      error: () => {
        this.completing = false;
        this.saving = false;
      }
    });
  }

  rejectTask(): void {
    this.rejectRequested.emit();
  }

  acceptTask(): void {
    this.acceptRequested.emit();
  }

  transferTask(): void {
    this.transferRequested.emit();
  }

  reassignTask(): void {
    this.reassignRequested.emit();
  }

  closeScreen(): void {
    this.confirmationService.confirm({
      message: 'Bạn có muốn đóng cửa sổ hiện tại không?',
      header: 'Đóng',
      icon: 'pi pi-question-circle',
      acceptLabel: 'Có',
      rejectLabel: 'Không',
      acceptButtonStyleClass: 'close-confirm-accept-button',
      accept: () => this.closeRequested.emit(),
    });
    this.focusCloseConfirmAcceptButton();
  }

  private focusCloseConfirmAcceptButton(): void {
    const selectors = [
      '.close-confirm-accept-button',
      '.p-confirmdialog-accept-button',
      '.p-confirm-dialog-accept'
    ];

    const focusButton = (): void => {
      const dialog = document.querySelector<HTMLElement>('.close-screen-confirm-dialog');
      if (!dialog) {
        return;
      }

      const target = selectors
        .map(selector => dialog.querySelector<HTMLElement>(selector))
        .find(Boolean);
      const button = target instanceof HTMLButtonElement
        ? target
        : target?.querySelector<HTMLButtonElement>('button');

      if (button && document.activeElement !== button) {
        button.focus();
      }
    };

    [0, 50, 100, 150, 250, 400, 650, 900].forEach(delay => {
      setTimeout(focusButton, delay);
    });
  }

  get actionWorkTaskStatus(): WorkTaskStatus | null {
    return this.detailWorkTaskStatus ?? this.workTaskStatus;
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

  get isEditable(): boolean {
    return !this.forceReadOnly && this.actionWorkTaskStatus === WorkTaskStatus.InProgress;
  }

  get canShowReassignAction(): boolean {
    return this.showOnsiteActions
      && this.detailIsReporter
      && (this.actionWorkTaskStatus === WorkTaskStatus.Rejected || this.actionWorkTaskStatus === WorkTaskStatus.Return);
  }

  get hasPendingUploads(): boolean {
    return this.onsitePhotos.some(x => !!x.uploading)
      || this.profileDocuments.some(doc => doc.attachments.some(file => !!file.uploading));
  }

  private loadOnsiteAssessmentDetail(workTaskId: string): void {
    this.onsiteAssessmentService.getDetail(workTaskId).subscribe({
      next: (detail) => {
        this.loadedDetailByWorkTaskId = workTaskId;
        this.applyDetailData(detail);
        this.tryAutoComplete();
      }
    });
  }

  private loadOnsiteAssessmentDetailByClaimId(claimId: string): void {
    this.onsiteAssessmentService.getDetailByClaimId(claimId).subscribe({
      next: (detail) => {
        this.loadedDetailByWorkTaskId = claimId;
        this.applyDetailData(detail);
        this.tryAutoComplete();
      }
    });
  }

  private refreshDetailAfterSuccess(successDetail: string): void {
    const workTaskId = this.workTaskId;
    if (!workTaskId) {
      this.savingDraft = false;
      this.completing = false;
      this.saving = false;
      this.messageService.add({
        severity: 'success',
        summary: 'Thành công',
        detail: successDetail
      });
      return;
    }

    this.onsiteAssessmentService.getDetail(workTaskId).subscribe({
      next: (detail) => {
        this.loadedDetailByWorkTaskId = workTaskId;
        this.applyDetailData(detail);
        this.savingDraft = false;
        this.completing = false;
        this.saving = false;
        this.messageService.add({
          severity: 'success',
          summary: 'Thành công',
          detail: successDetail
        });
      },
      error: () => {
        this.savingDraft = false;
        this.completing = false;
        this.saving = false;
        this.messageService.add({
          severity: 'warn',
          summary: 'Đã lưu nhưng chưa cập nhật dữ liệu',
          detail: 'Thao tác thành công nhưng chưa tải lại thông tin mới. Vui lòng tải lại trang nếu cần.'
        });
      }
    });
  }

  private reloadCurrentDetail(): void {
    if (!this.profileDocTypesReady) {
      this.pendingDetailWorkTaskId = this.useClaimScopedDetail ? this.claimId : this.workTaskId;
      return;
    }

    if (this.useClaimScopedDetail) {
      if (this.claimId) {
        this.loadOnsiteAssessmentDetailByClaimId(this.claimId);
      }
      return;
    }

    if (this.workTaskId) {
      this.loadOnsiteAssessmentDetail(this.workTaskId);
    }
  }

  private tryAutoComplete(): void {
    if (!this.autoCompleteRequest || !this.workTaskId) {
      return;
    }

    if (this.autoCompleteHandledForWorkTaskId === this.workTaskId) {
      return;
    }

    // Ensure detail payload has been loaded and applied before auto-triggering completion.
    if (this.loadedDetailByWorkTaskId !== this.workTaskId) {
      return;
    }

    if (this.actionWorkTaskStatus !== WorkTaskStatus.InProgress) {
      return;
    }

    this.autoCompleteHandledForWorkTaskId = this.workTaskId;
    setTimeout(() => this.completeOnsiteAssessment(), 0);
  }

  private getEmptyOnsiteForm(): OnsiteAssessmentFormData {
    return {
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
      lossPositions: [],
      hasLossThirdParty: false,
      witnessTestimony: '',
      causeDescription: '',
      description: '',
      locationDescription: '',
      damageDescription: '',
      partiesInvolvedDescription: '',
      addressPlan: '',
      customerRecommendation: '',
      otherDescription: '',
      garageId: null,
      issueDate: null,
      selectedImageType: null
    };
  }

  private initOnsiteOptions(): void {
    this.driverSexOptions = [
      { label: 'Nam', value: 'M' },
      { label: 'Nữ', value: 'F' }
    ];
    this.driverLicenseLevelOptions = [
      { label: 'B1', value: 'B1' },
      { label: 'B2', value: 'B2' },
      { label: 'C', value: 'C' },
      { label: 'D', value: 'D' },
      { label: 'E', value: 'E' }
    ];
    this.lossPositionOptions = [];
    this.garageOptions = [];
    this.imageTypeOptions = [];
  }

  private loadLossPositionOptions(): void {
    this.adminConfigService.getSelectList('VEHICLE_LOSS_POSITION').subscribe({
      next: (items) => {
        this.lossPositionOptions = (items || [])
          .filter(item => !!item.subCode)
          .map(item => ({
            label: item.name || item.subCode || '',
            value: item.subCode || ''
          }));
      },
      error: () => {
        this.lossPositionOptions = [];
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: 'Không tải được danh sách vị trí tổn thất từ Admin Config (VEHICLE_LOSS_POSITION).'
        });
      }
    });
  }

  private getDefaultProfileDocuments(): OnsiteProfileDocumentItem[] {
    return this.profileDocRowIds.map((rowId) => ({
      id: rowId,
      name: this.onsiteProfileDocTypeByRowId[rowId].name,
      documentTypeId: this.onsiteProfileDocTypeByRowId[rowId].docTypeId || null,
      complete: null,
      isCopy: null,
      note: '',
      issueDate: null,
      fileName: null,
      attachments: []
    }));
  }

  isInvalid(field: string): boolean {
    return !!this.fieldErrors[field];
  }

  getError(field: string): string {
    return this.fieldErrors[field] || '';
  }

  inputClass(field: string): string {
    return this.isInvalid(field) ? 'field-invalid' : '';
  }

  selectClass(field: string): string {
    return this.isInvalid(field) ? 'w-full field-invalid-select' : 'w-full';
  }

  private validateForm(): boolean {
    this.fieldErrors = {};

    if (!this.onsiteForm.lossPositions?.length) {
      this.fieldErrors['lossPositions'] = 'Vị trí tổn thất là bắt buộc.';
    }
    this.requireText('witnessTestimony', this.onsiteForm.witnessTestimony, 'Lời khai nhân chứng là bắt buộc.');
    this.requireText('causeDescription', this.onsiteForm.causeDescription, 'Nguyên nhân ban đầu là bắt buộc.');
    this.requireText('description', this.onsiteForm.description, 'Diễn biến là bắt buộc.');
    this.requireText('locationDescription', this.onsiteForm.locationDescription, 'Mô tả hiện trạng là bắt buộc.');
    this.requireText('damageDescription', this.onsiteForm.damageDescription, 'Mức độ thiệt hại là bắt buộc.');
    this.requireText('partiesInvolvedDescription', this.onsiteForm.partiesInvolvedDescription, 'Sơ bộ lỗi các bên là bắt buộc.');
    this.requireText('addressPlan', this.onsiteForm.addressPlan, 'Phương án giải quyết là bắt buộc.');
    this.requireText('customerRecommendation', this.onsiteForm.customerRecommendation, 'Kiến nghị của khách hàng là bắt buộc.');

    if (Object.keys(this.fieldErrors).length > 0) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Thiếu thông tin bắt buộc',
        detail: 'Vui lòng nhập đầy đủ các trường bắt buộc trước khi lưu.'
      });
      return false;
    }

    return true;
  }

  private requireText(field: string, value: unknown, message: string): void {
    if (value == null || String(value).trim().length === 0) {
      this.fieldErrors[field] = message;
    }
  }

  private requireDate(field: string, value: Date | null, message: string): void {
    if (!value) {
      this.fieldErrors[field] = message;
    }
  }

  onFieldChanged(field: string): void {
    if (this.fieldErrors[field]) {
      delete this.fieldErrors[field];
    }
  }

  private toSaveInput(): SaveOnsiteAssessmentInput {
    const images: SaveOnsiteImageInput[] = this.onsitePhotos
      .filter(photo => !!photo.documentId)
      .map(photo => ({
        claimDocumentId: photo.claimDocumentId || undefined,
        documentId: photo.documentId || undefined,
        documentTypeId: photo.documentTypeId || undefined
      }));

    const documents: SaveOnsiteDocumentInput[] = this.profileDocuments.flatMap((doc) => {
      const baseData = {
        note: doc.note || undefined,
        complete: doc.complete == null ? undefined : (doc.complete ? 'Y' : 'N') as 'Y' | 'N',
        isCopy: doc.isCopy == null ? undefined : (doc.isCopy ? 'Y' : 'N') as 'Y' | 'N',
        issueDate: this.toIso(doc.issueDate)
      };

      const attachmentDocs = doc.attachments
        .filter((file) => !!file.documentId)
        .map((file) => ({
          ...baseData,
          claimDocumentId: file.claimDocumentId || undefined,
          documentId: file.documentId || undefined,
          documentTypeId: file.documentTypeId || doc.documentTypeId || undefined
        }));

      if (attachmentDocs.length > 0) {
        return attachmentDocs;
      }

      if (doc.claimDocumentId || doc.documentId || doc.documentTypeId) {
        return [{
          ...baseData,
          claimDocumentId: doc.claimDocumentId || undefined,
          documentId: doc.documentId || undefined,
          documentTypeId: doc.documentTypeId || undefined
        }];
      }

      return [];
    });

    return {
      driverName: this.onsiteForm.driverName.trim(),
      driverSex: this.onsiteForm.driverSex || '',
      driverPhone: this.onsiteForm.driverPhone.trim(),
      driverIdNo: this.onsiteForm.driverIdNo.trim(),
      driverLicenseNo: this.onsiteForm.driverLicenseNo.trim(),
      driverLicenseEffectDate: this.toIso(this.onsiteForm.driverLicenseEffectDate),
      driverLicenseExpireDate: this.toIso(this.onsiteForm.driverLicenseExpireDate),
      driverLicenseLevel: this.onsiteForm.driverLicenseLevel || '',
      carRegistryNo: this.onsiteForm.carRegistryNo.trim(),
      carRegistryEffectDate: this.toIso(this.onsiteForm.carRegistryEffectDate),
      carRegistryExpireDate: this.toIso(this.onsiteForm.carRegistryExpireDate),
      lossPositions: this.onsiteForm.lossPositions || [],
      hasLossThirdParty: this.onsiteForm.hasLossThirdParty,
      witnessTestimony: this.onsiteForm.witnessTestimony,
      causeDescription: this.onsiteForm.causeDescription,
      description: this.onsiteForm.description,
      locationDescription: this.onsiteForm.locationDescription,
      damageDescription: this.onsiteForm.damageDescription,
      partiesInvolvedDescription: this.onsiteForm.partiesInvolvedDescription,
      addressPlan: this.onsiteForm.addressPlan,
      customerRecommendation: this.onsiteForm.customerRecommendation,
      otherDescription: this.onsiteForm.otherDescription,
      garageId: this.onsiteForm.garageId || undefined,
      issueDate: this.toIso(this.onsiteForm.issueDate),
      images,
      documents
    };
  }

  private loadGarageOptions(): void {
    this.loadingGarages = true;
    this.partnerService.getSelectList('GARAGE').subscribe({
      next: (items) => {
        this.garageOptions = (items || [])
          .filter(x => !!x.id)
          .map(x => ({ label: x.name || x.id || '', value: x.id || '' }));

        if (this.onsiteForm.garageId && !this.garageOptions.some(x => x.value === this.onsiteForm.garageId)) {
          this.garageOptions = [{ label: this.onsiteForm.garageId, value: this.onsiteForm.garageId }, ...this.garageOptions];
        }
        this.loadingGarages = false;
      },
      error: () => {
        this.loadingGarages = false;
      }
    });
  }

  private loadOnsiteDocumentTypes(): void {
    this.loadingOnsiteImageType = true;
    this.getDocumentTypesByGroupCode('CAR_ASSESSMENT_IMAGE').subscribe({
      next: (res) => {
        const imageTypes = (res || []).filter(x => !!x.id);
        if (imageTypes.length > 0) {
          this.imageTypeOptions = imageTypes.map(x => ({
            label: x.name || x.code || x.id || '',
            value: x.id || ''
          }));
        }
        this.loadingOnsiteImageType = false;
      },
      error: () => {
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: 'Không tải được danh sách loại ảnh từ Master theo DocumentGroupCode (CAR_ASSESSMENT_IMAGE).'
        });
        this.loadingOnsiteImageType = false;
      }
    });

    this.getDocumentTypesByGroupCode(ClaimOnsiteAssessmentTabComponent.PROFILE_DOCUMENT_GROUP_CODE)
      .subscribe({
      next: (res) => {
        const profileTypes = (res || []).filter(x => !!x.id);
        if (profileTypes.length > 0) {
          this.profileDocRowIds = profileTypes.map((_, idx) => `profile-doc-${idx + 1}`);
          this.onsiteProfileDocTypeByRowId = profileTypes.reduce<Record<string, { code: string; docTypeId: string | null; name: string }>>(
            (acc, docType, idx) => {
              const rowId = this.profileDocRowIds[idx];
              acc[rowId] = {
                code: docType.code || '',
                docTypeId: docType.id || null,
                name: docType.name || docType.code || docType.id || ''
              };
              return acc;
            },
            {}
          );
        }
        this.profileDocTypesReady = true;
        this.profileDocuments = this.mergeWithDefaultProfileDocuments(this.profileDocuments);
        if (this.pendingDetailWorkTaskId && this.loadedDetailByWorkTaskId !== this.pendingDetailWorkTaskId) {
          const pendingId = this.pendingDetailWorkTaskId;
          this.pendingDetailWorkTaskId = null;
          if (this.useClaimScopedDetail) {
            this.loadOnsiteAssessmentDetailByClaimId(pendingId);
          } else {
            this.loadOnsiteAssessmentDetail(pendingId);
          }
        }
      },
      error: () => {
        this.profileDocTypesReady = true;
        this.messageService.add({
          severity: 'error',
          summary: 'Lỗi',
          detail: `Không tải được danh sách hồ sơ từ Master theo DocumentGroupCode (${ClaimOnsiteAssessmentTabComponent.PROFILE_DOCUMENT_GROUP_CODE}).`
        });
        if (this.pendingDetailWorkTaskId && this.loadedDetailByWorkTaskId !== this.pendingDetailWorkTaskId) {
          const pendingId = this.pendingDetailWorkTaskId;
          this.pendingDetailWorkTaskId = null;
          if (this.useClaimScopedDetail) {
            this.loadOnsiteAssessmentDetailByClaimId(pendingId);
          } else {
            this.loadOnsiteAssessmentDetail(pendingId);
          }
        }
      }
    });
  }

  private toIso(value: Date | null): string | undefined {
    if (!value) return undefined;
    const year = value.getFullYear();
    const month = `${value.getMonth() + 1}`.padStart(2, '0');
    const day = `${value.getDate()}`.padStart(2, '0');
    const hours = `${value.getHours()}`.padStart(2, '0');
    const minutes = `${value.getMinutes()}`.padStart(2, '0');
    const seconds = `${value.getSeconds()}`.padStart(2, '0');
    return `${year}-${month}-${day}T${hours}:${minutes}:${seconds}`;
  }

  private prefillDriverInfoFromClaim(): void {
    if (!this.claim) return;
    this.onsiteForm.driverName = this.claim.driverName || '';
    this.onsiteForm.driverSex = (this.claim.driverSex as 'M' | 'F' | null) ?? null;
    this.onsiteForm.driverPhone = this.claim.driverPhone || '';
    this.onsiteForm.driverIdNo = this.claim.driverIdNo || '';
    this.onsiteForm.driverLicenseNo = this.claim.driverLicenseNo || '';
    this.onsiteForm.driverLicenseEffectDate = this.toDate(this.claim.driverLicenseEffectDate);
    this.onsiteForm.driverLicenseExpireDate = this.toDate(this.claim.driverLicenseExpireDate);
    this.onsiteForm.driverLicenseLevel = this.claim.driverLicenseLevel || null;
    this.onsiteForm.carRegistryNo = this.claim.driverRegistryNo || '';
    this.onsiteForm.carRegistryEffectDate = this.toDate(this.claim.driverRegistryEffectDate);
    this.onsiteForm.carRegistryExpireDate = this.toDate(this.claim.driverRegistryExpireDate);
    this.ensureDriverLicenseLevelOption(this.onsiteForm.driverLicenseLevel);
  }

  private applyDetailData(detail: OnsiteAssessmentDetailDto): void {
    this.detailWorkTaskStatus = this.toWorkTaskStatus(detail.workTaskStatus);
    this.detailIsReporter = detail.isReporter === true;
    this.assessorDeptName = detail.assessorDeptName || this.assessorDeptName;
    this.assessorName = detail.assessorName || this.assessorName;
    this.assessmentStartDate = this.toDate(detail.startDate);
    this.assessmentEndDate = this.toDate(detail.endDate);

    this.onsiteForm.driverName = detail.driverName || '';
    this.onsiteForm.driverSex = this.toDriverSex(detail.driverSex);
    this.onsiteForm.driverPhone = detail.driverPhone || '';
    this.onsiteForm.driverIdNo = detail.driverIdNo || '';
    this.onsiteForm.driverLicenseNo = detail.driverLicenseNo || '';
    this.onsiteForm.driverLicenseEffectDate = this.toDate(detail.driverLicenseEffectDate);
    this.onsiteForm.driverLicenseExpireDate = this.toDate(detail.driverLicenseExpireDate);
    this.onsiteForm.driverLicenseLevel = detail.driverLicenseLevel || null;
    this.ensureDriverLicenseLevelOption(this.onsiteForm.driverLicenseLevel);
    this.onsiteForm.carRegistryNo = detail.carRegistryNo || '';
    this.onsiteForm.carRegistryEffectDate = this.toDate(detail.carRegistryEffectDate);
    this.onsiteForm.carRegistryExpireDate = this.toDate(detail.carRegistryExpireDate);
    this.onsiteForm.lossPositions = detail.lossPositions || [];
    this.onsiteForm.hasLossThirdParty = !!detail.hasLossThirdParty;
    this.onsiteForm.witnessTestimony = detail.witnessTestimony || '';
    this.onsiteForm.causeDescription = detail.causeDescription || '';
    this.onsiteForm.description = detail.description || '';
    this.onsiteForm.locationDescription = detail.locationDescription || '';
    this.onsiteForm.damageDescription = detail.damageDescription || '';
    this.onsiteForm.partiesInvolvedDescription = detail.partiesInvolvedDescription || '';
    this.onsiteForm.addressPlan = detail.addressPlan || '';
    this.onsiteForm.customerRecommendation = detail.customerRecommendation || '';
    this.onsiteForm.otherDescription = detail.otherDescription || '';
    this.onsiteForm.garageId = detail.garageId || null;
    this.onsiteForm.issueDate = this.toDate(detail.issueDate);

    this.onsitePhotos = (detail.images || []).map((image) => ({
      id: image.claimDocumentId || crypto.randomUUID(),
      claimDocumentId: image.claimDocumentId || null,
      documentId: image.documentId || null,
      documentTypeId: image.documentTypeId || null,
      type: image.documentTypeName || null,
      fileType: this.getPreviewFileType(image.fileName || 'N/A', image.url || image.thumbnailUrl || ''),
      name: image.fileName || 'N/A',
      previewUrl: image.thumbnailUrl || image.url || '',
      sourceUrl: image.url || image.thumbnailUrl || '',
      uploadedAt: this.toDate(image.uploadedAt) || new Date(),
      uploader: image.uploaderName || 'N/A'
    }));

    const mappedDocuments = (detail.documents || []).map((doc, index) => ({
      id: doc.claimDocumentId || `doc-${index}`,
      name: doc.documentTypeName || '',
      complete: this.toNullableYnBoolean(doc.complete),
      isCopy: this.toNullableYnBoolean(doc.isCopy),
      note: doc.note || '',
      issueDate: this.toDate(doc.issueDate),
      fileName: doc.fileName || null,
      claimDocumentId: doc.claimDocumentId || null,
      documentId: doc.documentId || null,
      documentTypeId: doc.documentTypeId || null,
      url: doc.url || null,
      thumbnailUrl: doc.thumbnailUrl || null,
      attachments: doc.documentId || doc.url || doc.thumbnailUrl
        ? [
            {
              id: doc.documentId || doc.claimDocumentId || crypto.randomUUID(),
              name: doc.fileName || 'N/A',
              claimDocumentId: doc.claimDocumentId || null,
              documentId: doc.documentId || null,
              documentTypeId: doc.documentTypeId || null,
              url: doc.url || null,
              thumbnailUrl: doc.thumbnailUrl || null,
              mimeType: null,
              uploadedAt: this.toDate(this.getPossibleUploadedAt(doc)),
              uploading: false
            }
          ]
        : []
    }));
    this.profileDocuments = this.mergeWithDefaultProfileDocuments(mappedDocuments);
  }

  private getDocumentTypesByGroupCode(documentGroupCode: string) {
    return this.restService.request<any, ResDocumentTypeDto[]>({
      method: 'GET',
      url: `/api/master/document-types/by-document-group/${documentGroupCode}`,
    }, { apiName: 'Master' });
  }

  private toDriverSex(value?: string | null): 'M' | 'F' | null {
    if (!value) return null;
    const normalized = value.toUpperCase();
    return normalized === 'M' || normalized === 'F' ? normalized : null;
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
    const mapped = keyLookup[normalized.toLowerCase()];
    return mapped !== undefined
      ? mapped
      : null;
  }

  private toDate(value?: string): Date | null {
    if (!value) return null;
    const date = new Date(value);
    return isNaN(date.getTime()) ? null : date;
  }

  private toNullableYnBoolean(value?: string | null): boolean | null {
    const normalized = (value || '').trim().toUpperCase();
    if (!normalized) return null;
    return normalized === 'Y';
  }

  private ensureDriverLicenseLevelOption(value?: string | null): void {
    const normalized = (value || '').trim();
    if (!normalized) {
      return;
    }

    const exists = this.driverLicenseLevelOptions.some(
      option => option.value.toUpperCase() === normalized.toUpperCase()
    );
    if (exists) {
      return;
    }

    this.driverLicenseLevelOptions = [
      { label: normalized, value: normalized },
      ...this.driverLicenseLevelOptions
    ];
  }

  private mergeWithDefaultProfileDocuments(mappedDocuments: OnsiteProfileDocumentItem[]): OnsiteProfileDocumentItem[] {
    const defaults = this.getDefaultProfileDocuments();
    if (mappedDocuments.length === 0) {
      return defaults;
    }

    const merged = defaults.map((doc) => ({ ...doc, attachments: [] as OnsiteProfileFileItem[] }));
    const defaultNameToId = new Map(
      merged.map((doc) => [this.normalizeDocName(doc.name), doc.id] as const)
    );

    for (const mapped of mappedDocuments) {
      const byNameId = defaultNameToId.get(this.normalizeDocName(mapped.name));
      const byTypeId = this.getProfileRowIdByDocTypeId(mapped.documentTypeId);
      const hintedId = mapped.attachments
        .map((file) => file.documentId ? this.profileDocumentHintsByDocumentId[file.documentId] : null)
        .find((id): id is string => !!id);
      const targetId = byTypeId || byNameId || hintedId || this.getFallbackProfileDocumentRowId();
      const target = merged.find((x) => x.id === targetId);
      if (!target) continue;

      if (!target.note && mapped.note) target.note = mapped.note;
      if (!target.issueDate && mapped.issueDate) target.issueDate = mapped.issueDate;
      if (!target.fileName && mapped.fileName) target.fileName = mapped.fileName;
      if (!target.claimDocumentId && mapped.claimDocumentId) target.claimDocumentId = mapped.claimDocumentId;
      if (!target.documentId && mapped.documentId) target.documentId = mapped.documentId;
      if (!target.documentTypeId && mapped.documentTypeId) target.documentTypeId = mapped.documentTypeId;
      if (!target.url && mapped.url) target.url = mapped.url;
      if (!target.thumbnailUrl && mapped.thumbnailUrl) target.thumbnailUrl = mapped.thumbnailUrl;
      target.complete = target.complete ?? mapped.complete;
      target.isCopy = target.isCopy ?? mapped.isCopy;

      const existingAttachmentIds = new Set(target.attachments.map((f) => f.documentId || f.id));
      const incoming = (mapped.attachments || []).filter((f) => !existingAttachmentIds.has(f.documentId || f.id));
      target.attachments = [...target.attachments, ...incoming];
    }

    return merged;
  }

  private normalizeDocName(value: string | null | undefined): string {
    return (value || '')
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '')
      .trim()
      .toLowerCase();
  }

  private getProfileRowIdByDocTypeId(documentTypeId?: string | null): string | null {
    if (!documentTypeId) return null;
    const row = Object.entries(this.onsiteProfileDocTypeByRowId).find(([, v]) => v.docTypeId === documentTypeId);
    return row?.[0] || null;
  }

  private getFallbackProfileDocumentRowId(): string {
    const otherRow = Object.entries(this.onsiteProfileDocTypeByRowId)
      .find(([, v]) => (v.code || '').toUpperCase() === 'OTHER');
    return otherRow?.[0] || this.profileDocRowIds[this.profileDocRowIds.length - 1] || 'profile-doc-1';
  }

  private getPossibleUploadedAt(doc: any): string | undefined {
    return doc?.uploadedAt
      || doc?.creationTime
      || doc?.createdAt
      || doc?.createdDate
      || doc?.lastModificationTime
      || undefined;
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

}
