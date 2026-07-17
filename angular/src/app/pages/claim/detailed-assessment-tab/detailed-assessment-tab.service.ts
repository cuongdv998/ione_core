import { Injectable } from '@angular/core';
import { Rest, RestService } from '@abp/ng.core';
import type { RejectClaimTaskInput, TransferClaimTaskInput } from '@/proxy/claim/claims/models';

import {
  DetailedAssessmentDetailDto,
  SaveDetailedAssessmentInput,
} from './detailed-assessment-tab.models';
import type { ReassignDetailedAssessmentInput } from '@/proxy/claim/claims/models';

@Injectable({ providedIn: 'root' })
export class DetailedAssessmentTabService {
  private readonly apiName = 'Claim';

  constructor(private readonly restService: RestService) {}

  getDetail = (workTaskId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DetailedAssessmentDetailDto>(
      {
        method: 'GET',
        url: `/api/claim-detailed-assessment/${workTaskId}/detail`,
      },
      { apiName: this.apiName, ...config }
    );

  getCoverageOptions = (workTaskId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DetailedAssessmentDetailDto['coverageOptions']>(
      {
        method: 'GET',
        url: `/api/claim-detailed-assessment/${workTaskId}/coverage-options`,
      },
      { apiName: this.apiName, ...config }
    );

  save = (workTaskId: string, input: SaveDetailedAssessmentInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>(
      {
        method: 'PUT',
        url: `/api/claim-detailed-assessment/${workTaskId}/save`,
        body: input,
      },
      { apiName: this.apiName, ...config }
    );

  complete = (workTaskId: string, input: SaveDetailedAssessmentInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>(
      {
        method: 'PUT',
        url: `/api/claim-detailed-assessment/${workTaskId}/complete`,
        body: input,
      },
      { apiName: this.apiName, ...config }
    );

  accept = (workTaskId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>(
      {
        method: 'PUT',
        url: `/api/claim-detailed-assessment/${workTaskId}/accept`,
      },
      { apiName: this.apiName, ...config }
    );

  cancel = (workTaskId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>(
      {
        method: 'PUT',
        url: `/api/claim-detailed-assessment/${workTaskId}/cancel`,
      },
      { apiName: this.apiName, ...config }
    );

  reject = (workTaskId: string, input: RejectClaimTaskInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>(
      {
        method: 'PUT',
        url: `/api/claim-detailed-assessment/${workTaskId}/reject`,
        body: input,
      },
      { apiName: this.apiName, ...config }
    );

  transfer = (workTaskId: string, input: TransferClaimTaskInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>(
      {
        method: 'PUT',
        url: `/api/claim-detailed-assessment/${workTaskId}/transfer`,
        body: input,
      },
      { apiName: this.apiName, ...config }
    );

  reassign = (workTaskId: string, input: ReassignDetailedAssessmentInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>(
      {
        method: 'PUT',
        url: `/api/claim-detailed-assessment/${workTaskId}/reassign`,
        body: input,
      },
      { apiName: this.apiName, ...config }
    );
}
