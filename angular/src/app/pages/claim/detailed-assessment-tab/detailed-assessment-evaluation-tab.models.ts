export interface DetailedAssessmentEvaluationItemModel {
  evaluateItemId: string;
  name: string;
  result: 'Y' | 'N' | null;
}

export interface DetailedAssessmentEvaluationDetailModel {
  workTaskId: string;
  claimFolderId?: string | null;
  result: 'Y' | 'N' | null;
  description?: string | null;
  items: DetailedAssessmentEvaluationItemModel[];
}

export interface SaveDetailedAssessmentEvaluationItemModel {
  evaluateItemId: string;
  result: 'Y' | 'N';
}

export interface SaveDetailedAssessmentEvaluationInputModel {
  result: 'Y' | 'N';
  description?: string | null;
  items: SaveDetailedAssessmentEvaluationItemModel[];
}
