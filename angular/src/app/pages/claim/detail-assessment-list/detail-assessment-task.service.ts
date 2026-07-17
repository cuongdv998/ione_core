import { Injectable, inject } from '@angular/core';
import { RestService } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import type { ClaimTaskDto, GetClaimTasksInput } from '@/proxy/claim/claims/claim-task-models';

@Injectable({ providedIn: 'root' })
export class DetailAssessmentTaskService {
  private restService = inject(RestService);

  getList(input: GetClaimTasksInput) {
    return this.restService.request<any, PagedResultDto<ClaimTaskDto>>(
      {
        method: 'GET',
        url: '/api/claim-folder-tasks/list',
        params: input,
      },
      { apiName: 'Claim' }
    );
  }
}
