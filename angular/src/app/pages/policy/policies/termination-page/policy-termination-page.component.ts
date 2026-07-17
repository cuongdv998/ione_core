import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { DatePickerModule } from 'primeng/datepicker';
import { ToastModule } from 'primeng/toast';
import { DialogModule } from 'primeng/dialog';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TextareaModule } from 'primeng/textarea';
import { InputNumberModule } from 'primeng/inputnumber';
import { FileUploadModule } from 'primeng/fileupload';
import { TableModule } from 'primeng/table';
import { TooltipModule } from 'primeng/tooltip';
import { CheckboxModule } from 'primeng/checkbox';

import { PoliciesComponent } from '../policies.component';
import type { PolicyDto, CalculateRefundAmountBatchInput, DistributeRefundInput, TerminatePolicyInput } from '@/proxy/policy/policies/models';

import { Router } from '@angular/router';
import { MessageService, ConfirmationService } from 'primeng/api';
import { PermissionService } from '@/core/services/permission.service';
import { LocalizationService } from '@/core/services/localization.service';
import { ConfigStateService } from '@abp/ng.core';
import { RestService } from '@abp/ng.core';
import { PolicyService } from '@/proxy/policy/controllers/policy.service';
import { ResPartnerService } from '@/proxy/partner/controllers/res-partner.service';
import { ProLineOfBusinessService } from '@/proxy/product/controllers/pro-line-of-business.service';
import { ResCustomerService } from '@/proxy/customer/controllers/res-customer.service';
import { ResCurrencyService } from '@/proxy/master/controllers/res-currency.service';
import { ResChannelService } from '@/proxy/partner/controllers/res-channel.service';
import { HrEmployeeService } from '@/proxy/hr/controllers/hr-employee.service';
import { ResProvinceService } from '@/proxy/master/controllers/res-province.service';
import { ResWardService } from '@/proxy/master/controllers/res-ward.service';
import { PolicyTypeService } from '@/proxy/policy/controllers/policy-type.service';
import { PolicyContractService } from '@/proxy/policy/controllers/policy-contract.service';
import { PolicyCertificateService } from '@/proxy/policy/controllers/policy-certificate.service';
import { ResCarBrandService } from '@/proxy/master/controllers/res-car-brand.service';
import { ResCarCategoryService } from '@/proxy/master/controllers/res-car-category.service';
import { ResCarLineService } from '@/proxy/master/controllers/res-car-line.service';
import { ResCarGroupService } from '@/proxy/master/controllers/res-car-group.service';
import { ResCarTypeService } from '@/proxy/master/controllers/res-car-type.service';
import { AdminConfigService } from '@/proxy/master/controllers/admin-config.service';
import { ResOrganizationTypeService } from '@/proxy/partner/controllers/res-organization-type.service';
import { ResIndustryService } from '@/proxy/customer/controllers/res-industry.service';
import { ProProductService } from '@/proxy/product/pro-products/pro-product.service';
import { ResDocumentService } from '@/proxy/master/controllers/res-document.service';
import { ResDocumentTypeService } from '@/proxy/master/controllers/res-document-type.service';
import { ResObjectTypeService } from '@/proxy/master/controllers/res-object-type.service';
import { ResReasonService } from '@/proxy/master/controllers/res-reason.service';
import { ReportTemplateService } from '@/proxy/report/controllers/report-template.service';
import { TranslatePipe } from '@/core/pipes/translate.pipe';

@Component({
  selector: 'app-policy-termination-page',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    PanelModule,
    ButtonModule,
    InputTextModule,
    SelectModule,
    DatePickerModule,
    ToastModule,
    DialogModule,
    ConfirmDialogModule,
    TextareaModule,
    InputNumberModule,
    FileUploadModule,
    TableModule,
    TooltipModule,
    CheckboxModule,
    TranslatePipe
  ],
  templateUrl: './policy-termination-page.component.html',
  styleUrl: './policy-termination-page.component.scss',
  providers: [MessageService, ConfirmationService]
})
export class PolicyTerminationPageComponent extends PoliciesComponent implements OnInit {
  /** Termination-specific state */
  terminationReasonId: string | null = null;
  terminationReasonDescription: string | null = null;
  terminationReasonOptions: Array<{ label: string; value: string }> = [];
  totalRefundAmount = 0;

  /** Debounce timer for total refund amount change → recalc coverage refunds */
  private refundRecalcTimer: ReturnType<typeof setTimeout> | null = null;

  /** All sections are read-only on the termination page */
  readonly isContractInfoEditable = false;
  readonly isGeneralInfoEditable = false;
  readonly isEffectDateEditable = false;
  readonly isInsuredObjectEditable = false;
  readonly isCoverageEditable = false;
  readonly isAllInforEditable = false;

  constructor(
    private route: ActivatedRoute,
    policyService: PolicyService,
    messageService: MessageService,
    confirmationService: ConfirmationService,
    reportTemplateService: ReportTemplateService,
    permissionService: PermissionService,
    localizationService: LocalizationService,
    router: Router,
    partnerService: ResPartnerService,
    lobService: ProLineOfBusinessService,
    customerService: ResCustomerService,
    currencyService: ResCurrencyService,
    channelService: ResChannelService,
    employeeService: HrEmployeeService,
    provinceService: ResProvinceService,
    wardService: ResWardService,
    policyTypeService: PolicyTypeService,
    policyContractService: PolicyContractService,
    carBrandService: ResCarBrandService,
    carCategoryService: ResCarCategoryService,
    carLineService: ResCarLineService,
    carGroupService: ResCarGroupService,
    carTypeService: ResCarTypeService,
    adminConfigService: AdminConfigService,
    organizationTypeService: ResOrganizationTypeService,
    industryService: ResIndustryService,
    productService: ProProductService,
    restService: RestService,
    configState: ConfigStateService,
    resDocumentService: ResDocumentService,
    resDocumentTypeService: ResDocumentTypeService,
    resObjectTypeService: ResObjectTypeService,
    policyCertificateService: PolicyCertificateService,
    private reasonService: ResReasonService,
    private _cdr: ChangeDetectorRef
  ) {
    super(
      policyService,
      messageService,
      confirmationService,
      reportTemplateService,
      permissionService,
      localizationService,
      router,
      partnerService,
      lobService,
      customerService,
      currencyService,
      channelService,
      employeeService,
      provinceService,
      wardService,
      policyTypeService,
      policyContractService,
      carBrandService,
      carCategoryService,
      carLineService,
      carGroupService,
      carTypeService,
      adminConfigService,
      organizationTypeService,
      industryService,
      productService,
      restService,
      configState,
      resDocumentService,
      resDocumentTypeService,
      resObjectTypeService,
      policyCertificateService,
      _cdr
    );
  }

  override ngOnInit(): void {
    super.ngOnInit();
    this.loading = false;

    const id = (this.route.snapshot.paramMap.get('id') || '').trim();
    if (!id) {
      this.backToList();
      return;
    }

    this.loadTerminationReasonOptions();

    const stub = { id } as PolicyDto;
    this.openViewDialog(stub);
  }

  override populateFormFromDetail(detail: any): void {
    super.populateFormFromDetail(detail);
  }

  protected override onDetailAppliedToProducts(): void {
    this.calculateAndPopulateRefundAmounts();
  }

  override getPolicyApprovalStatusFooterClass(approvalStatus: string | null | undefined): string {
    return super.getPolicyApprovalStatusFooterClass(approvalStatus);
  }

  backToList(): void {
    this.navigateToPolicyList();
  }

  confirmCancel(): void {
    this.confirmationService.confirm({
      message: this.localizationService.localize('Policy::Policy:UnsavedExitConfirm'),
      header: this.localizationService.localize('Policy::Warning'),
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: this.localizationService.localize('AbpUi::Yes'),
      rejectLabel: this.localizationService.localize('AbpUi::No'),
      rejectButtonStyleClass: 'p-button-secondary',
      accept: () => this.backToList()
    });
  }

  protected override afterSaveSuccess(): void {
    this.backToList();
  }

  private loadTerminationReasonOptions(): void {
    this.reasonService.getSelectListByGroupCode('TERMINATE_POLICY_REASON').subscribe({
      next: (result: any) => {
        this.terminationReasonOptions = (result || []).map((reason: any) => ({
          label: reason.name || '',
          value: reason.id || ''
        }));
      },
      error: () => {
        this.terminationReasonOptions = [];
      }
    });
  }

  /** Called when user edits "Tổng phí cần hoàn". Debounces then recalculates coverage refunds via API. */
  onTotalRefundAmountChange(_value: number): void {
    if (this.refundRecalcTimer != null) {
      clearTimeout(this.refundRecalcTimer);
      this.refundRecalcTimer = null;
    }
    const delayMs = 500;
    this.refundRecalcTimer = setTimeout(() => {
      this.refundRecalcTimer = null;
      this.recalculateCoverageRefundsFromTotal();
    }, delayMs);
  }

  private recalculateCoverageRefundsFromTotal(): void {
    const coverages: DistributeRefundInput['coverages'] = [];
    Object.keys(this.productCoverages).forEach(productId => {
      const items = this.productCoverages[productId] || [];
      items.forEach((item: any) => {
        if (!item.isSectionHeader && item.coverageId) {
          coverages.push({
            productCoverageId: item.productCoverageId || undefined,
            coverageId: item.coverageId,
            amountLiability: item.insuranceAmount ?? 0,
            quantity: item.quantity ?? 1,
            premium: item.premium ?? 0,
            premiumVAT: item.vat ?? 0
          });
        }
      });
    });
    if (coverages.length === 0) {
      return;
    }
    const input: DistributeRefundInput = {
      coverages,
      totalRefund: this.totalRefundAmount
    };
    this.policyService.distributeRefundToCoverages(input).subscribe({
      next: (result: any) => {
        const refundByCoverageId = new Map<string, number>(
          (result.coverageRefunds || []).map((r: any) => [r.coverageId ?? '', r.refundAmount ?? 0])
        );
        Object.keys(this.productCoverages).forEach(productId => {
          const items = this.productCoverages[productId] || [];
          items.forEach((item: any) => {
            if (item.coverageId && refundByCoverageId.has(item.coverageId)) {
              item.refundAmount = refundByCoverageId.get(item.coverageId) ?? 0;
            }
          });
        });
        this._cdr.markForCheck();
      },
      error: () => {
        // Silently fail; leave existing refund amounts
      }
    });
  }

  private calculateAndPopulateRefundAmounts(): void {
    if (!this.currentPolicyDetail) {
      return;
    }

    const selectedProductIds = Object.keys(this.selectedProducts).filter(id => this.selectedProducts[id]);
    if (selectedProductIds.length === 0) {
      return;
    }

    const detail = this.currentPolicyDetail;
    const riskMotor = detail?.riskObject?.riskObjectMotor || {};
    const attributes: Record<string, any> = {
      carPlate: riskMotor.carPlate || '',
      carAge: riskMotor.carOld || 0,
      carGroup: riskMotor.carGroupCode || '',
      carUsage: riskMotor.carUsage || '',
      carValue: riskMotor.riskObjectValue || 0,
      seat: String(riskMotor.carSeatNumber || ''),
      payload: String(riskMotor.carPayloadCapacity || '')
    };

    const productInputs = selectedProductIds
      .map(productId => {
        const product = this.products.find(p => p.id === productId);
        if (!product?.code) return null;

        const coverages = this.productCoverages[productId] || [];
        const productCoverages = coverages
          .filter((c: any) => !c.isSectionHeader && c.coverageId)
          .map((c: any) => ({
            productCoverageId: c.productCoverageId || '',
            coverageId: c.coverageId || '',
            amountLiability: c.insuranceAmount || 0,
            quantity: c.quantity || 1,
            premium: c.premium || 0,
            premiumVAT: c.vat || 0
          }));

        const totals = this.getProductTotals(productId);
        return {
          productId: product.code,
          productCoverages,
          totalPremium: totals.premium,
          totalPremiumVAT: totals.premiumWithVAT
        };
      })
      .filter((p): p is NonNullable<typeof p> => p !== null);

    if (productInputs.length === 0) {
      return;
    }

    const batchInput: CalculateRefundAmountBatchInput = {
      policyId: detail.id || '',
      terminationDate: this.localIsoDate(new Date()),
      attributes,
      products: productInputs
    };

    this.policyService.calculateRefundAmountBatch(batchInput).subscribe({
      next: (result: any) => {
        this.totalRefundAmount = result.totalRefund || 0;

        const refundByCoverageId = new Map<string, number>(
          (result.coverageRefunds || []).map((r: any) => [r.coverageId || '', r.refundAmount || 0])
        );

        Object.keys(this.productCoverages).forEach(productId => {
          const items = this.productCoverages[productId] || [];
          items.forEach(item => {
            if (item.coverageId && refundByCoverageId.has(item.coverageId)) {
              item.refundAmount = refundByCoverageId.get(item.coverageId) || 0;
            }
          });
        });
      },
      error: () => {
        // Silently fail — leave refund as 0
      }
    });
  }

  submitTermination(): void {
    if (!this.terminationReasonId) {
      this.messageService.add({
        severity: 'warn',
        summary: this.localizationService.localize('AbpUi::Warning'),
        detail: this.localizationService.localize('Policy::Policy:SelectTerminationReason')
      });
      return;
    }

    if (!this.selectedPolicy?.id) {
      return;
    }

    this.confirmationService.confirm({
      message: 'Bạn có chắc chắn muốn chấm dứt đơn bảo hiểm này không?',
      header: this.localizationService.localize('Policy::Warning'),
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: this.localizationService.localize('AbpUi::Yes'),
      rejectLabel: this.localizationService.localize('AbpUi::No'),
      rejectButtonStyleClass: 'p-button-secondary',
      accept: () => this.executeTermination()
    });
  }

  private executeTermination(): void {
    if (!this.selectedPolicy?.id) {
      return;
    }

    this.loading = true;

    const coverageRefunds: Array<{ coverageId: string; refundAmount: number }> = [];
    Object.keys(this.productCoverages).forEach(productId => {
      const items = this.productCoverages[productId] || [];
      items.forEach(item => {
        if (!item.isSectionHeader && item.coverageId && item.refundAmount != null) {
          coverageRefunds.push({
            coverageId: item.coverageId,
            refundAmount: item.refundAmount
          });
        }
      });
    });

    const input: TerminatePolicyInput = {
      terminationReasonId: this.terminationReasonId || undefined,
      terminationReasonDescription: this.terminationReasonDescription || undefined,
      terminationDate: this.localIsoDate(new Date()),
      totalRefundAmount: this.totalRefundAmount,
      customerNote: this.formData.customerNotes || undefined,
      internalNote: this.formData.internalNotes || undefined,
      coverageRefunds,
      documents: (this.uploadedDocumentIds || []).map(documentId => ({ documentId }))
    };

    this.policyService.terminatePolicy(this.selectedPolicy.id as string, input).subscribe({
      next: () => {
        this.loading = false;
        this.messageService.add({
          severity: 'success',
          summary: this.localizationService.localize('AbpUi::Success'),
          detail: this.localizationService.localize('Policy::Policy:TerminatedSuccessfully')
        });
        this.afterSaveSuccess();
      },
      error: (error: any) => {
        this.loading = false;
        this.messageService.add({
          severity: 'error',
          summary: this.localizationService.localize('AbpUi::Error'),
          detail: error?.error?.error?.message || 'Failed to terminate policy'
        });
      }
    });
  }

  /** Format a Date to YYYY-MM-DD in local time (no UTC shift). */
  private localIsoDate(date: Date): string {
    const pad = (n: number) => String(n).padStart(2, '0');
    return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`;
  }
}
