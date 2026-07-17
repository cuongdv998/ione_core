import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { DialogModule } from 'primeng/dialog';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TableModule } from 'primeng/table';
import { DatePickerModule } from 'primeng/datepicker';

import { ApiKeyService } from '@/proxy/api-keys';
import type { ApiKeyDto, CreateApiKeyResultDto } from '@/proxy/api-keys';

@Component({
  selector: 'app-api-keys',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    PanelModule,
    ButtonModule,
    InputTextModule,
    ToastModule,
    DialogModule,
    ConfirmDialogModule,
    TableModule,
    DatePickerModule,
  ],
  templateUrl: './api-keys.component.html',
  styleUrl: './api-keys.component.scss',
  providers: [MessageService, ConfirmationService],
})
export class ApiKeysComponent implements OnInit {
  keys: ApiKeyDto[] = [];
  loading = true;
  createDialogVisible = false;
  createdKeyDialogVisible = false;
  createdKeyResult: CreateApiKeyResultDto | null = null;

  createForm = {
    name: '',
    expiresAt: null as Date | null,
  };

  constructor(
    private apiKeyService: ApiKeyService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
  ) {}

  ngOnInit(): void {
    this.loadList();
  }

  loadList(): void {
    this.loading = true;
    this.apiKeyService.getList().subscribe({
      next: (list) => {
        this.keys = list ?? [];
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load API keys.' });
      },
    });
  }

  openCreateDialog(): void {
    this.createForm = { name: '', expiresAt: null };
    this.createDialogVisible = true;
  }

  createKey(): void {
    if (!this.createForm.name?.trim()) {
      this.messageService.add({ severity: 'warn', summary: 'Validation', detail: 'Name is required.' });
      return;
    }
    const input = {
      name: this.createForm.name.trim(),
      expiresAt: this.createForm.expiresAt ? this.createForm.expiresAt.toISOString() : undefined,
    };
    this.apiKeyService.create(input).subscribe({
      next: (result) => {
        this.createDialogVisible = false;
        this.createdKeyResult = result;
        this.createdKeyDialogVisible = true;
        this.loadList();
      },
      error: (err) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: err?.error?.error?.message ?? 'Failed to create API key.',
        });
      },
    });
  }

  copyCreatedKey(): void {
    if (!this.createdKeyResult?.fullKey) return;
    navigator.clipboard.writeText(this.createdKeyResult.fullKey).then(
      () => this.messageService.add({ severity: 'success', summary: 'Copied', detail: 'API key copied to clipboard.' }),
      () => this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to copy.' }),
    );
  }

  closeCreatedKeyDialog(): void {
    this.createdKeyDialogVisible = false;
    this.createdKeyResult = null;
  }

  revokeKey(key: ApiKeyDto): void {
    const id = key.id;
    if (id == null || id === undefined) return;
    this.confirmationService.confirm({
      message: `Revoke API key "${key.name}"? It will stop working immediately.`,
      header: 'Revoke API Key',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.apiKeyService.revoke(id).subscribe({
          next: () => {
            this.messageService.add({ severity: 'success', summary: 'Revoked', detail: 'API key has been revoked.' });
            this.loadList();
          },
          error: (err) => {
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: err?.error?.error?.message ?? 'Failed to revoke API key.',
            });
          },
        });
      },
    });
  }

  formatDate(value: string | undefined): string {
    if (!value) return '—';
    const d = new Date(value);
    return isNaN(d.getTime()) ? value : d.toLocaleString();
  }
}
