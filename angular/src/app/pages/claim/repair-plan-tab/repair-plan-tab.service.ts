import { Injectable } from '@angular/core';
import { Rest, RestService } from '@abp/ng.core';

import type {
  RepairPlanInitDataDto,
  RepairPlanPascEligibilityDto,
  RepairPlanGarageOptionDto,
  RepairPlanSubmitInfoDto,
  SaveRepairPlanInput,
} from './repair-plan-tab.models';

export interface RepairPlanSavedDto {
  id: string;
  status: string;
}

@Injectable({ providedIn: 'root' })
export class RepairPlanTabService {
  private readonly apiName = 'Claim';

  constructor(private readonly restService: RestService) {}

  getInitData = (claimId: string, workTaskId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, RepairPlanInitDataDto>(
      {
        method: 'GET',
        url: `/api/claim-repair-plan/${claimId}/init`,
        params: { workTaskId },
      },
      { apiName: this.apiName, ...config }
    );

  getPascEligibility = (claimId: string, workTaskId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, RepairPlanPascEligibilityDto>(
      {
        method: 'GET',
        url: `/api/claim-repair-plan/${claimId}/pasc-eligibility`,
        params: { workTaskId },
      },
      { apiName: this.apiName, ...config }
    );

  getGarages = (search?: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, RepairPlanGarageOptionDto[]>(
      {
        method: 'GET',
        url: '/api/claim-repair-plan/garages',
        params: { search, maxResultCount: 50 },
      },
      { apiName: this.apiName, ...config }
    );

  getSubmitInfo = (claimId: string, workTaskId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, RepairPlanSubmitInfoDto>(
      {
        method: 'GET',
        url: `/api/claim-repair-plan/${claimId}/submit-info`,
        params: { workTaskId },
      },
      { apiName: this.apiName, ...config }
    );

  save = (input: SaveRepairPlanInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, RepairPlanSavedDto>(
      {
        method: 'POST',
        url: '/api/claim-repair-plan/save',
        body: input,
      },
      { apiName: this.apiName, ...config }
    );

  submit = (input: SaveRepairPlanInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, RepairPlanSavedDto>(
      {
        method: 'POST',
        url: '/api/claim-repair-plan/submit',
        body: input,
      },
      { apiName: this.apiName, ...config }
    );
}
