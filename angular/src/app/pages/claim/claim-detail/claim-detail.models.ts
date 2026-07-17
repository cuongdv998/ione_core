/**
 * Local models for claim detail page (list/grid state, filters, etc.).
 * DTOs from proxy: ClaimDetailDto, ClaimFolderListDto, ClaimStageProgressDto, ClaimSentMessageDto, ClaimDocumentDto.
 */

export interface ClaimDetailTabIndex {
  general: number;
  images: number;
  progress: number;
}
