
import { Component, EventEmitter, Input, OnInit, Output, inject, ViewChild, NO_ERRORS_SCHEMA } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule, FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { TabsModule } from 'primeng/tabs';
import { TableModule } from 'primeng/table';
import { DatePickerModule } from 'primeng/datepicker';
import { FileUploadModule } from 'primeng/fileupload';
import { SelectModule } from 'primeng/select';
import { MultiSelectModule } from 'primeng/multiselect';
import { CheckboxModule } from 'primeng/checkbox';
import { ReportTemplateService } from '../../../proxy/report/controllers/report-template.service';
import { ReportTemplateDto, CreateReportTemplateDto, UpdateReportTemplateDto } from '../../../proxy/report/report-templates/models';
import { ResDocumentService } from '../../../proxy/master/controllers/res-document.service';
import { ResDocumentTypeService } from '../../../proxy/master/controllers/res-document-type.service';
import { CreateResDocumentTypeDto } from '../../../proxy/master/res-document-types/models';
import { ResDocumentTypeStatus } from '../../../proxy/res-document-types/res-document-type-status.enum';
import { switchMap, map } from 'rxjs/operators';
import { of, Observable } from 'rxjs';
import { LocalizationService } from '@/core/services/localization.service';
import { TranslatePipe } from '@/core/pipes/translate.pipe';

@Component({
    selector: 'app-report-detail-modal',
    standalone: true,
    imports: [
        CommonModule,
        CommonModule,
        ReactiveFormsModule,
        FormsModule,
        DialogModule,
        ButtonModule,
        InputTextModule,
        TextareaModule,
        TabsModule,
        TableModule,
        DatePickerModule,
        FileUploadModule,
        SelectModule,
        MultiSelectModule,
        CheckboxModule,
        TranslatePipe
    ],
    templateUrl: './report-detail-modal.component.html',
    styleUrls: ['./report-detail-modal.component.scss'],
    schemas: [NO_ERRORS_SCHEMA]
})
export class ReportDetailModalComponent implements OnInit {
    private fb = inject(FormBuilder);
    private reportTemplateService = inject(ReportTemplateService);
    private messageService = inject(MessageService);
    private resDocumentService = inject(ResDocumentService);
    private resDocumentTypeService = inject(ResDocumentTypeService);
    private localizationService = inject(LocalizationService);

    @Output() saved = new EventEmitter<void>();

    visible = false;
    form!: FormGroup;
    isEdit = false;
    selectedId: string | null = null;
    uploadedFile: any = null;

    statusOptions: any[] = [];

    dataTypeOptions: any[] = [];

    ngOnInit() {
        this.initOptions();
        this.initForm();
    }

    initOptions() {
        this.statusOptions = [
            { label: this.localizationService.localize('Report::Status:Active'), value: 'active' },
            { label: this.localizationService.localize('Report::Status:Inactive'), value: 'deactive' }
        ];

        this.dataTypeOptions = [
            { label: this.localizationService.localize('Report::DataType:String'), value: 'string' },
            { label: this.localizationService.localize('Report::DataType:Number'), value: 'number' },
            { label: this.localizationService.localize('Report::DataType:Date'), value: 'date' },
            { label: this.localizationService.localize('Report::DataType:Boolean'), value: 'boolean' }
        ];
    }

    initForm() {
        this.form = this.fb.group({
            name: ['', Validators.required],
            code: ['', Validators.required],
            documentId: [''], // Validation handled manually
            description: [''],
            status: ['active', Validators.required],
            effectDate: [null],
            expireDate: [null],
            parameters: this.fb.array([]),
            sqls: this.fb.array([])
        });
    }

    editingStatus: boolean[] = [];

    get parameters(): FormArray {
        return this.form.get('parameters') as FormArray;
    }

    get sqls(): FormArray {
        return this.form.get('sqls') as FormArray;
    }

    // Parameter Helper Methods
    addParameter() {
        const paramGroup = this.fb.group({
            id: [null],
            code: ['', Validators.required],
            name: ['', Validators.required],
            dataType: ['string', Validators.required],
            status: ['active', Validators.required],
            description: [''],
            reportTemplateId: [this.selectedId || '']
        });
        this.parameters.push(paramGroup);
        this.editingStatus.push(true);
    }

    removeParameter(index: number) {
        this.parameters.removeAt(index);
        this.editingStatus.splice(index, 1);
    }

    onAcceptParameter(index: number) {
        const group = this.parameters.at(index) as FormGroup;
        if (group.invalid) {
            group.markAllAsTouched();
            return;
        }
        this.editingStatus[index] = false;
    }

    onEditParameter(index: number) {
        this.editingStatus[index] = true;
    }

    sqlEditingStatus: boolean[] = [];

    // SQL Helper Methods
    addSql() {
        const sqlGroup = this.fb.group({
            id: [null],
            sqlText: ['', Validators.required],
            varName: ['', Validators.required],
            status: ['active', Validators.required],
            isSingle: [false],
            parameterCodes: [[]],
            reportTemplateId: [this.selectedId || '']
        });
        this.sqls.push(sqlGroup);
        this.sqlEditingStatus.push(true);
    }

    removeSql(index: number) {
        this.sqls.removeAt(index);
        this.sqlEditingStatus.splice(index, 1);
    }

    onAcceptSql(index: number) {
        const group = this.sqls.at(index) as FormGroup;
        if (group.invalid) {
            group.markAllAsTouched();
            return;
        }
        this.sqlEditingStatus[index] = false;
    }

    onEditSql(index: number) {
        this.sqlEditingStatus[index] = true;
    }

    currentDocument: any = null;

    open(id?: string) {
        this.isEdit = !!id;
        this.selectedId = id || null;
        this.visible = true;
        this.form.reset();
        this.form.get('code')?.enable();
        this.uploadedFile = null;
        this.currentDocument = null; // Reset current document
        this.parameters.clear();
        this.editingStatus = [];
        this.sqls.clear();
        this.sqlEditingStatus = [];

        // Default values
        this.form.patchValue({
            status: 'active'
        });

        if (id) {
            this.loadData(id);
        }
    }

    loadData(id: string) {
        this.reportTemplateService.getDetail(id).subscribe({
            next: (res: any) => {
                const data = res;

                this.currentDocument = data.document;

                this.form.patchValue({
                    name: data.name,
                    code: data.code,
                    documentId: data.documentId,
                    description: data.description,
                    status: data.status,
                    effectDate: data.effectDate ? new Date(data.effectDate) : null,
                    expireDate: data.expireDate ? new Date(data.expireDate) : null,
                });
                this.form.get('code')?.disable();

                // Clear existing arrays
                this.parameters.clear();
                this.editingStatus = [];
                this.sqls.clear();
                this.sqlEditingStatus = [];

                if (Array.isArray(data.parameters)) {
                    data.parameters.forEach((p: any) => {
                        this.addParameter();
                        this.parameters.at(this.parameters.length - 1).patchValue(p);
                        this.editingStatus[this.editingStatus.length - 1] = false;
                    });
                }

                if (Array.isArray(data.sqls)) {
                    data.sqls.forEach((s: any) => {
                        this.addSql();
                        this.sqls.at(this.sqls.length - 1).patchValue(s);
                        this.sqlEditingStatus[this.sqlEditingStatus.length - 1] = false;
                    });
                }
            },
            error: (err) => {
                console.error('Load report template failed', err);

                const message =
                    err?.error?.error?.message ||
                    err?.error?.message ||
                    'Không thể tải chi tiết mẫu báo cáo.';

                this.messageService.add({ severity: 'error', summary: this.localizationService.localize('Report::Notify:Error'), detail: message });
            }
        });
    }


    close() {
        this.visible = false;
    }

    onUpload(event: any) {
        this.uploadedFile = event.files[0];
        // Don't set documentId here, it will be set after upload
    }

    save() {
        if (this.form.invalid) {
            this.form.markAllAsTouched();
            this.messageService.add({ severity: 'warn', summary: this.localizationService.localize('Report::Notify:Warning'), detail: this.localizationService.localize('Report::Validation:Required') });
            return;
        }

        const value = this.form.getRawValue();

        // Custom validation for documentId/File
        // Custom validation for documentId/File
        if (!value.documentId && !this.uploadedFile) {
            this.messageService.add({ severity: 'warn', summary: this.localizationService.localize('Report::Notify:Warning'), detail: this.localizationService.localize('Report::Validation:SelectTemplateFile') });
            return;
        }

        this.messageService.add({ severity: 'info', summary: this.localizationService.localize('Report::Notify:Info'), detail: this.localizationService.localize('Report::Message:Processing') });

        // Helper to proceed with save
        const proceedSave = (docId?: string) => {
            if (docId) {
                this.form.patchValue({ documentId: docId });
                value.documentId = docId;
            }
            this.performSave(value);
        };

        if (this.uploadedFile) {
            this.uploadFileAndGetId().subscribe({
                next: (id) => proceedSave(id),
                error: (err) => {
                    console.error(err);
                    this.messageService.add({ severity: 'error', summary: this.localizationService.localize('Report::Notify:Error'), detail: this.localizationService.localize('Report::Error:UploadFailed') });
                }
            });
        } else {
            proceedSave();
        }
    }

    uploadFileAndGetId(): Observable<string> {
        const code = 'REPORT_TEMPLATE';
        return this.resDocumentTypeService.getList({ code: code, maxResultCount: 1 }).pipe(
            switchMap(res => {
                if (res.items && res.items.length > 0) {
                    return of(res.items[0]);
                } else {
                    const newType: CreateResDocumentTypeDto = {
                        code: code,
                        name: 'Report Template',
                        status: ResDocumentTypeStatus.Active,
                        bucket: 'report-template'
                    };
                    return this.resDocumentTypeService.create(newType);
                }
            }),
            switchMap(docType => {
                if (!docType.id) throw new Error("Document Type ID missing");
                return this.resDocumentService.uploadSingleFile(
                    docType.id,
                    code,
                    this.uploadedFile
                );
            }),
            map(res => res.id!)
        );
    }

    performSave(value: any) {
        // Transform dates to string yyyy-MM-dd
        const formatDate = (date: Date) => {
            if (!date) return null;
            const d = new Date(date);
            const year = d.getFullYear();
            const month = String(d.getMonth() + 1).padStart(2, '0');
            const day = String(d.getDate()).padStart(2, '0');
            return `${year}-${month}-${day}`;
        };

        // Prepare payload
        // Ensure reportTemplateId is set for children
        const params = value.parameters.map((p: any) => ({
            ...p,
            reportTemplateId: this.selectedId || p.reportTemplateId || '3fa85f64-5717-4562-b3fc-2c963f66afa6' // Placeholder if created
        }));

        const sqls = value.sqls.map((s: any) => ({
            ...s,
            reportTemplateId: this.selectedId || s.reportTemplateId || '3fa85f64-5717-4562-b3fc-2c963f66afa6'
        }));

        const payload = {
            ...value,
            effectDate: formatDate(value.effectDate),
            expireDate: formatDate(value.expireDate),
            parameters: params,
            sqls: sqls
        };

        if (this.isEdit && this.selectedId) {
            console.log("update", payload);
            this.reportTemplateService.update(this.selectedId, payload as UpdateReportTemplateDto).subscribe({
                next: () => {
                    this.messageService.add({ severity: 'success', summary: this.localizationService.localize('Report::Notify:Success'), detail: this.localizationService.localize('Report::Success:Update') });
                    this.saved.emit();
                    this.close();
                },
                error: (err) => {
                    console.error(err);
                    this.messageService.add({ severity: 'error', summary: this.localizationService.localize('Report::Notify:Error'), detail: this.localizationService.localize('Report::Error:Update') });
                }
            });
        } else {
            console.log("create", payload);
            this.reportTemplateService.create(payload as CreateReportTemplateDto).subscribe({
                next: () => {
                    this.messageService.add({ severity: 'success', summary: this.localizationService.localize('Report::Notify:Success'), detail: this.localizationService.localize('Report::Success:Create') });
                    this.saved.emit();
                    this.close();
                },
                error: (err) => {
                    console.error(err);
                    this.messageService.add({ severity: 'error', summary: this.localizationService.localize('Report::Notify:Error'), detail: this.localizationService.localize('Report::Error:Create') });
                }
            });
        }
    }

    // Helper to get available parameter codes for the SQL tab dropdown
    get parameterCodeOptions() {
        return this.parameters.value.map((p: any) => ({ label: p.code, value: p.code }));
    }

    formatStatus(status: number | string | undefined | null): string {
        if (status === undefined || status === null) {
            return '';
        }

        let statusValue: number;

        if (typeof status === 'number') {
            statusValue = status;
        } else if (typeof status === 'string') {
            if (status === 'Active' || status === 'active' || status === '0') {
                statusValue = 0;
            } else if (status === 'Deactive' || status === 'deactive' || status === '1') {
                statusValue = 1;
            } else {
                statusValue = 1;
            }
        } else {
            statusValue = 1;
        }

        return statusValue === 0
            ? this.localizationService.localize('Report::Status:Active')
            : this.localizationService.localize('Report::Status:Inactive');
    }
}
