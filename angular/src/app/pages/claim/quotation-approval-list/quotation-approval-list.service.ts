import { Injectable, inject } from '@angular/core';
import { RestService } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';

import type {
  QuotationApprovalListQuery,
  QuotationApprovalListRow,
} from './quotation-approval-list.models';

export interface ApproveQuotationApprovalPayload {
  workTaskId: string;
  comment?: string | null;
}

export interface RejectQuotationApprovalPayload {
  workTaskId: string;
  reasonId: string;
  comment?: string | null;
}

export interface ReassignQuotationApprovalPayload {
  workTaskId: string;
  newAssigneeId: string;
  reasonId: string;
  reasonDescription?: string | null;
  comment?: string | null;
}

@Injectable({ providedIn: 'root' })
export class QuotationApprovalListService {
  private restService = inject(RestService);

  getList(query: QuotationApprovalListQuery) {
    return this.restService.request<
      typeof query,
      PagedResultDto<QuotationApprovalListRow & Record<string, unknown>>
    >(
      {
        method: 'GET',
        url: '/api/claim-repair-plan/approval-list',
        params: {
          skipCount: query.skipCount ?? 0,
          maxResultCount: query.maxResultCount ?? 10,
          sorting: query.sorting,
          lobId: query.lobId,
          insurerId: query.insurerId,
          processDeptId: query.processDeptId,
          claimId: query.claimId,
          approvalStatus: query.approvalStatus,
          carPlate: query.carPlate,
          vin: query.vin,
          engineNumber: query.engineNumber,
          folderNo: query.folderNo,
          reporterId: query.reporterId,
        },
      },
      { apiName: 'Claim' }
    );
  }

  approve(input: ApproveQuotationApprovalPayload) {
    return this.restService.request<ApproveQuotationApprovalPayload, void>(
      {
        method: 'POST',
        url: '/api/claim-repair-plan/approve',
        body: {
          workTaskId: input.workTaskId,
          comment: input.comment ?? undefined,
        },
      },
      { apiName: 'Claim' }
    );
  }

  reject(input: RejectQuotationApprovalPayload) {
    return this.restService.request<RejectQuotationApprovalPayload, void>(
      {
        method: 'POST',
        url: '/api/claim-repair-plan/reject',
        body: {
          workTaskId: input.workTaskId,
          reasonId: input.reasonId,
          comment: input.comment ?? undefined,
        },
      },
      { apiName: 'Claim' }
    );
  }

  reassign(input: ReassignQuotationApprovalPayload) {
    return this.restService.request<ReassignQuotationApprovalPayload, void>(
      {
        method: 'POST',
        url: '/api/claim-repair-plan/reassign',
        body: {
          workTaskId: input.workTaskId,
          newAssigneeId: input.newAssigneeId,
          reasonId: input.reasonId,
          reasonDescription: input.reasonDescription ?? undefined,
          comment: input.comment ?? undefined,
        },
      },
      { apiName: 'Claim' }
    );
  }
}
