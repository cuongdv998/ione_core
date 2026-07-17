import { CommonModule } from '@angular/common';
import {
  Component,
  ElementRef,
  Input,
  OnChanges,
  SimpleChanges,
  ViewChild,
} from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { TooltipModule } from 'primeng/tooltip';
import { saveAs } from 'file-saver';

import { TranslatePipe } from '@/core/pipes/translate.pipe';
import type { ClaimDocumentDto } from '@/proxy/claim/claims/models';
import { ResDocumentService } from '@/proxy/master/controllers/res-document.service';

@Component({
  selector: 'app-claim-task-incident-images-tab',
  standalone: true,
  imports: [CommonModule, ButtonModule, DialogModule, TooltipModule, TranslatePipe],
  templateUrl: './claim-task-incident-images-tab.component.html',
  styleUrl: './claim-task-incident-images-tab.component.scss',
})
export class ClaimTaskIncidentImagesTabComponent implements OnChanges {
  @Input() documents: ClaimDocumentDto[] = [];
  @Input() loading = false;

  @ViewChild('previewViewport') previewViewport?: ElementRef<HTMLDivElement>;

  previewVisible = false;
  previewDoc: ClaimDocumentDto | null = null;

  /** Kích thước ảnh sau khi fit vào khung (scale = 1), px */
  private fitW = 0;
  private fitH = 0;

  private readonly minScale = 0.25;
  private readonly maxScale = 5;
  private readonly zoomFactor = 1.15;

  imageScale = 1;

  private viewportWheelCleanup?: () => void;
  private readonly imageExtensions = new Set(['jpg', 'jpeg', 'png', 'gif', 'bmp', 'webp', 'svg', 'heic', 'heif']);
  private readonly videoExtensions = new Set(['mp4', 'mov', 'avi', 'mkv', 'webm', 'm4v', '3gp']);
  private readonly resolvingDocumentIds = new Set<string>();

  hydratedDocuments: ClaimDocumentDto[] = [];

  constructor(private readonly resDocumentService: ResDocumentService) {}

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['documents']) {
      this.hydratedDocuments = (this.documents || []).map((doc) => ({ ...doc }));
      this.hydrateMissingPreviewUrls();
    }
  }

  isImage(mime: string | undefined): boolean {
    if (!mime) return false;
    return mime.startsWith('image/');
  }

  isVideo(mime: string | undefined): boolean {
    if (!mime) return false;
    return mime.startsWith('video/');
  }

  isImageDoc(doc: ClaimDocumentDto): boolean {
    return this.isImage(doc.mimeType) || this.imageExtensions.has(this.getFileExtension(doc));
  }

  isVideoDoc(doc: ClaimDocumentDto): boolean {
    return this.isVideo(doc.mimeType) || this.videoExtensions.has(this.getFileExtension(doc));
  }

  getPreviewSource(doc: ClaimDocumentDto): string | null {
    return doc.thumbnailUrl || doc.url || null;
  }

  getDocumentUrl(doc: ClaimDocumentDto): string | null {
    return doc.url || doc.thumbnailUrl || null;
  }

  get zoomPercent(): number {
    return Math.round(this.imageScale * 100);
  }

  get imageFitReady(): boolean {
    return this.fitW > 0 && this.fitH > 0;
  }

  get previewImgStyle(): { width: string; height: string } {
    if (!this.fitW || !this.fitH) {
      return { width: 'auto', height: 'auto' };
    }
    return {
      width: `${this.fitW * this.imageScale}px`,
      height: `${this.fitH * this.imageScale}px`,
    };
  }

  openPreview(doc: ClaimDocumentDto): void {
    const docUrl = this.getDocumentUrl(doc);
    if (docUrl) {
      this.openResolvedPreview({
        ...doc,
        url: doc.url || docUrl,
        thumbnailUrl: doc.thumbnailUrl || docUrl,
      });
      return;
    }

    if (!doc.documentId) return;

    this.resolveDocumentUrl(doc, (resolvedDoc) => this.openResolvedPreview(resolvedDoc));
  }

  onPreviewDialogShow(): void {
    setTimeout(() => {
      if (this.previewDoc && this.isImageDoc(this.previewDoc)) {
        this.bindViewportWheel();
      }
    }, 0);
  }

  closePreview(): void {
    this.viewportWheelCleanup?.();
    this.viewportWheelCleanup = undefined;
    this.previewVisible = false;
    this.previewDoc = null;
    this.resetZoom();
    this.fitW = 0;
    this.fitH = 0;
  }

  onPreviewImgLoad(event: Event): void {
    const img = event.target as HTMLImageElement;
    const nw = img.naturalWidth;
    const nh = img.naturalHeight;
    if (!nw || !nh) return;

    const viewportMaxW = Math.min(window.innerWidth * 0.94, 1024);
    const viewportMaxH = window.innerHeight * 0.72;
    const r = Math.min(viewportMaxW / nw, viewportMaxH / nh, 1);
    this.fitW = nw * r;
    this.fitH = nh * r;
  }

  zoomIn(): void {
    this.imageScale = Math.min(this.maxScale, this.imageScale * this.zoomFactor);
  }

  zoomOut(): void {
    this.imageScale = Math.max(this.minScale, this.imageScale / this.zoomFactor);
  }

  resetZoom(): void {
    this.imageScale = 1;
  }

  private bindViewportWheel(): void {
    this.viewportWheelCleanup?.();
    const el = this.previewViewport?.nativeElement;
    if (!el) return;

    const handler = (e: WheelEvent): void => {
      if (!this.previewDoc || !this.isImageDoc(this.previewDoc)) return;
      e.preventDefault();
      e.deltaY > 0 ? this.zoomOut() : this.zoomIn();
    };

    el.addEventListener('wheel', handler, { passive: false });
    this.viewportWheelCleanup = () => el.removeEventListener('wheel', handler);
  }

  async downloadCurrent(): Promise<void> {
    const d = this.previewDoc;
    if (!d?.url) return;
    const name = d.fileName?.trim() || 'download';

    try {
      const res = await fetch(d.url, { credentials: 'include', mode: 'cors' });
      if (!res.ok) throw new Error('fetch failed');
      const blob = await res.blob();
      saveAs(blob, name);
    } catch {
      const a = document.createElement('a');
      a.href = d.url;
      a.setAttribute('download', name);
      a.target = '_blank';
      a.rel = 'noopener noreferrer';
      document.body.appendChild(a);
      a.click();
      a.remove();
    }
  }

  private openResolvedPreview(doc: ClaimDocumentDto): void {
    const docUrl = this.getDocumentUrl(doc);
    if (!docUrl) return;

    if (this.isImageDoc(doc) || this.isVideoDoc(doc)) {
      this.resetZoom();
      this.fitW = 0;
      this.fitH = 0;
      this.previewDoc = doc;
      this.previewVisible = true;
      return;
    }

    window.open(docUrl, '_blank');
  }

  private hydrateMissingPreviewUrls(): void {
    this.hydratedDocuments
      .filter((doc) => !!doc.documentId && !this.getPreviewSource(doc))
      .forEach((doc) => this.resolveDocumentUrl(doc));
  }

  private resolveDocumentUrl(doc: ClaimDocumentDto, onResolved?: (doc: ClaimDocumentDto) => void): void {
    const documentId = doc.documentId;
    if (!documentId || this.resolvingDocumentIds.has(documentId)) return;

    this.resolvingDocumentIds.add(documentId);
    this.resDocumentService.getSingleFile(documentId).subscribe({
      next: (file) => {
        const resolvedDoc: ClaimDocumentDto = {
          ...doc,
          url: file.url || doc.url,
          thumbnailUrl: doc.thumbnailUrl || file.url,
          mimeType: doc.mimeType || file.mimeType,
        };
        this.hydratedDocuments = this.hydratedDocuments.map((item) =>
          item.id === doc.id ? resolvedDoc : item
        );
        if (this.previewDoc?.id === doc.id) {
          this.previewDoc = resolvedDoc;
        }
        onResolved?.(resolvedDoc);
        this.resolvingDocumentIds.delete(documentId);
      },
      error: () => {
        this.resolvingDocumentIds.delete(documentId);
      },
    });
  }

  private getFileExtension(doc: ClaimDocumentDto): string {
    const fileName = doc.fileName?.trim();
    if (fileName && fileName.includes('.')) {
      return fileName.split('.').pop()?.toLowerCase() || '';
    }

    const url = this.getDocumentUrl(doc);
    if (!url) return '';

    try {
      const parsed = new URL(url, window.location.origin);
      const path = parsed.pathname;
      if (!path.includes('.')) return '';
      return path.split('.').pop()?.toLowerCase() || '';
    } catch {
      const clean = url.split('?')[0].split('#')[0];
      if (!clean.includes('.')) return '';
      return clean.split('.').pop()?.toLowerCase() || '';
    }
  }
}
