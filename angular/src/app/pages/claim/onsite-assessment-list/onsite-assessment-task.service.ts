import { Injectable, inject } from '@angular/core';
import { RestService } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import {
  CreateOnsiteAssessmentRequestInput,
  GetOnsiteAssessmentTasksInput,
  OnsiteAssessmentCreateRequestDto,
  OnsiteAssessmentCreateResultDto,
  OnsiteAssessmentTaskDto,
  ReassignOnsiteAssessmentInput
} from './onsite-assessment-list.models';

@Injectable({ providedIn: 'root' })
export class OnsiteAssessmentTaskService {
  private restService = inject(RestService);

  getList(input: GetOnsiteAssessmentTasksInput) {
    return this.restService.request<any, PagedResultDto<OnsiteAssessmentTaskDto>>(
      {
        method: 'GET',
        url: '/api/claim-onsite-assessment-tasks/list',
        params: input,
      },
      { apiName: 'Claim' }
    );
  }

  export(input: GetOnsiteAssessmentTasksInput) {
    return this.restService.request<any, Blob>(
      {
        method: 'POST',
        responseType: 'blob',
        url: '/api/claim-onsite-assessment-tasks/export',
        body: input,
      },
      { apiName: 'Claim' }
    );
  }

  cancel(workTaskId: string) {
    return this.restService.request<any, void>(
      {
        method: 'PUT',
        url: `/api/claim-onsite-assessment-tasks/${workTaskId}/cancel`,
      },
      { apiName: 'Claim' }
    );
  }

  getCreateRequestList() {
    return this.restService.request<any, OnsiteAssessmentCreateRequestDto[]>(
      {
        method: 'GET',
        url: '/api/claim-onsite-assessment-tasks/create-request-list',
      },
      { apiName: 'Claim' }
    );
  }

  create(input: CreateOnsiteAssessmentRequestInput) {
    return this.restService.request<any, OnsiteAssessmentCreateResultDto>(
      {
        method: 'POST',
        url: '/api/claim-onsite-assessment-tasks/create',
        body: input,
      },
      { apiName: 'Claim' }
    );
  }

  reassign(workTaskId: string, input: ReassignOnsiteAssessmentInput) {
    return this.restService.request<any, void>(
      {
        method: 'PUT',
        url: `/api/claim-onsite-assessment-tasks/${workTaskId}/reassign`,
        body: input,
      },
      { apiName: 'Claim' }
    );
  }
}
