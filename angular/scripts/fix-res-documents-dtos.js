const fs = require('fs');
const path = require('path');

/**
 * Script để đảm bảo các DTO cần thiết luôn có trong res-documents/models.ts
 * Chạy sau khi generate proxy để fix các DTO bị thiếu
 * 
 * Usage: node scripts/fix-res-documents-dtos.js
 */

const RES_DOCUMENTS_MODELS_PATH = path.join(__dirname, '../src/app/proxy/master/res-documents/models.ts');

const REQUIRED_DTOS = `
import type { FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';

export interface FileResponseDto {
  id?: string;
  fileName?: string;
  url?: string;
  mimeType?: string;
  fileSize: number;
}

export interface GetResDocumentsInput extends PagedAndSortedResultRequestDto {
  documentTypeId?: string;
  groupCode?: string;
}

export interface ResDocumentDto extends FullAuditedEntityDto<string> {
  groupCode?: string;
  documentTypeId?: string;
  fileSize?: number;
  fileName?: string;
  storeFileName?: string;
  url?: string;
  thumbnailUrl?: string;
  checksum?: string;
  mimeType?: string;
  bucketName?: string;
  versionId?: string;
}
`.trim();

function main() {
  console.log('🔧 Fixing res-documents/models.ts...');
  
  // Ensure directory exists
  const dir = path.dirname(RES_DOCUMENTS_MODELS_PATH);
  if (!fs.existsSync(dir)) {
    fs.mkdirSync(dir, { recursive: true });
  }
  
  // Read current content if exists
  let currentContent = '';
  if (fs.existsSync(RES_DOCUMENTS_MODELS_PATH)) {
    currentContent = fs.readFileSync(RES_DOCUMENTS_MODELS_PATH, 'utf8');
  }
  
  // Check if required DTOs exist
  const hasGetResDocumentsInput = currentContent.includes('export interface GetResDocumentsInput');
  const hasResDocumentDto = currentContent.includes('export interface ResDocumentDto');
  const hasFileResponseDto = currentContent.includes('export interface FileResponseDto');
  
  if (hasGetResDocumentsInput && hasResDocumentDto && hasFileResponseDto) {
    // Check if imports are correct
    const hasCorrectImports = currentContent.includes('import type { FullAuditedEntityDto, PagedAndSortedResultRequestDto } from \'@abp/ng.core\'');
    
    if (hasCorrectImports) {
      console.log('   ✓ All required DTOs already exist');
      return;
    }
  }
  
  // Write required DTOs
  fs.writeFileSync(RES_DOCUMENTS_MODELS_PATH, REQUIRED_DTOS + '\n', 'utf8');
  console.log('   ✅ Fixed res-documents/models.ts');
}

main();
