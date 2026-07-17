import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { CheckboxModule } from 'primeng/checkbox';
import { DatePickerModule } from 'primeng/datepicker';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ReportTemplateService } from '../../../proxy/report/controllers/report-template.service';
import { ReportTemplateDDL } from '../../../proxy/report/report-templates/models';
import { ReportTemplateParameterDto } from '../../../proxy/report/report-template-parameters/models';
import { TranslatePipe } from '@/core/pipes/translate.pipe';
import { LocalizationService } from '@/core/services/localization.service';

@Component({
    selector: 'app-report-export-dynamic-modal',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        DialogModule,
        ButtonModule,
        SelectModule,
        InputTextModule,
        InputNumberModule,
        CheckboxModule,
        DatePickerModule,
        ConfirmDialogModule,
        TranslatePipe
    ],
    templateUrl: './report-export-dynamic-modal.component.html',
    providers: [ConfirmationService]
})
export class ReportExportDynamicModalComponent {
    private fb = inject(FormBuilder);
    private reportTemplateService = inject(ReportTemplateService);
    private confirmationService = inject(ConfirmationService);
    private messageService = inject(MessageService);
    private localizationService = inject(LocalizationService);

    visible = false;
    loading = false;

    templates: ReportTemplateDDL[] = [];
    selectedTemplateId: string | null = null;

    selectedTemplateName: string | null | undefined = null;

    parameters: ReportTemplateParameterDto[] = [];
    form: FormGroup = this.fb.group({});

    // Control for template selection, outside the dynamic form to avoid issues
    templateControl = this.fb.control<string | null>(null, Validators.required);

    open(templateId?: string) {
        this.visible = true;
        this.reset();
        this.loadTemplates(templateId);
    }

    close() {
        this.visible = false;
    }

    reset() {
        this.selectedTemplateId = null;
        this.selectedTemplateName = null;
        this.templateControl.reset();
        this.parameters = [];
        this.form = this.fb.group({});
        this.loading = false;
    }

    loadTemplates(preSelectedId?: string) {
        this.reportTemplateService.getDdl().subscribe({
            next: (res) => {
                this.templates = res || [];
                if (preSelectedId) {
                    this.templateControl.setValue(preSelectedId);
                    this.onTemplateChange({ value: preSelectedId });
                }
            },
            error: () => {
                this.messageService.add({ severity: 'error', summary: this.localizationService.localize('Report::Notify:Error'), detail: 'Failed to load templates' });
            }
        });
    }

    onTemplateChange(event: any) {
        const id = event.value;
        this.selectedTemplateId = id;

        const template = this.templates.find(t => t.id === id);
        this.selectedTemplateName = template?.name;

        this.parameters = [];
        this.form = this.fb.group({});

        if (!id) return;

        this.loading = true;
        this.reportTemplateService.getListParams(id).subscribe({
            next: (res) => {
                this.parameters = res || [];
                this.buildForm();
                this.loading = false;
            },
            error: () => {
                this.loading = false;
                this.messageService.add({ severity: 'error', summary: this.localizationService.localize('Report::Notify:Error'), detail: 'Failed to load parameters' });
            }
        });
    }

    buildForm() {
        const group: any = {};
        this.parameters.forEach(param => {
            if (param.code) {
                // Parameters are optional now
                group[param.code] = [null];
            }
        });
        this.form = this.fb.group(group);
    }

    onSave() {
        if (!this.selectedTemplateId) {
            this.templateControl.markAsTouched();
            return;
        }

        if (this.form.invalid) {
            this.form.markAllAsTouched();
            return;
        }

        this.confirmationService.confirm({
            message: this.localizationService.localize('Report::Message:ExportConfirmation') || 'Are you sure you want to export?',
            header: this.localizationService.localize('AbpUi::AreYouSure'),
            icon: 'pi pi-exclamation-triangle',
            accept: () => {
                this.export();
            }
        });
    }

    export() {
        console.log(this.form.value);
        if (!this.selectedTemplateId) return;

        this.loading = true;

        // Process form values
        const formValue = this.form.value;
        const params: Record<string, any> = {};

        // Format dates
        Object.keys(formValue).forEach(key => {
            const value = formValue[key];
            if (value instanceof Date) {
                const d = new Date(value);
                const year = d.getFullYear();
                const month = String(d.getMonth() + 1).padStart(2, '0');
                const day = String(d.getDate()).padStart(2, '0');
                params[key] = `${year}-${month}-${day}`;
            } else if (value !== null && value !== undefined && value !== '') {
                params[key] = value;
            }
        });

        this.reportTemplateService.genDynamicFile(this.selectedTemplateId, params).subscribe({
            next: (data: Blob) => {
                const a = document.createElement('a');
                const objectUrl = URL.createObjectURL(data);
                a.href = objectUrl;
                // Try to find template name for filename
                const template = this.templates.find(t => t.id === this.selectedTemplateId);
                const filename = template?.name ? `${template.name}_export` : 'dynamic-report';

                a.download = filename;
                a.click();
                URL.revokeObjectURL(objectUrl);

                this.loading = false;
                this.visible = false;
                this.messageService.add({ severity: 'success', summary: this.localizationService.localize('Report::Notify:Success'), detail: 'Export successful' });
            },
            error: (err) => {
                console.error(err);
                this.loading = false;
                this.messageService.add({ severity: 'error', summary: this.localizationService.localize('Report::Notify:Error'), detail: 'Export failed' });
            }
        });
    }
}
