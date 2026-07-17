import { Injectable } from '@angular/core';
import { Rest, RestService } from '@abp/ng.core';

import {
  DetailedAssessmentEvaluationDetailModel,
  SaveDetailedAssessmentEvaluationInputModel,
} from './detailed-assessment-evaluation-tab.models';

@Injectable({ providedIn: 'root' })
export class DetailedAssessmentEvaluationTabService {
  private readonly apiName = 'Claim';

  constructor(private readonly restService: RestService) {}

  getDetail = (workTaskId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DetailedAssessmentEvaluationDetailModel>(
      {
        method: 'GET',
        url: `/api/claim-detailed-assessment-evaluation/${workTaskId}/detail`,
      },
      { apiName: this.apiName, ...config }
    );

  save = (workTaskId: string, input: SaveDetailedAssessmentEvaluationInputModel, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>(
      {
        method: 'PUT',
        url: `/api/claim-detailed-assessment-evaluation/${workTaskId}/save`,
        body: input,
      },
      { apiName: this.apiName, ...config }
    );
}
