/**
 * Script để fix file upload methods trong các file service proxy
 * Chuyển đổi File object thành FormData để upload đúng cách
 * 
 * Usage: node scripts/fix-proxy-file-uploads.js
 */

const fs = require('fs');
const path = require('path');

const PROXY_DIR = path.join(__dirname, '../src/app/proxy');

/**
 * Fix file upload methods in a service file
 */
function fixServiceFile(filePath) {
  if (!fs.existsSync(filePath)) {
    return false;
  }

  let content = fs.readFileSync(filePath, 'utf8');
  const originalContent = content;

  let hasChanges = false;

  // Step 1: Fix the method signature - change IFormFile to File for all methods
  if (content.includes('IFormFile')) {
    // Replace file: IFormFile with file: File
    content = content.replace(/\bfile:\s*IFormFile\b/g, 'file: File');
    // Replace files: IFormFile[] with files: File[]
    content = content.replace(/\bfiles:\s*IFormFile\[\]/g, 'files: File[]');
    // Replace List<IFormFile> with File[]
    content = content.replace(/List<IFormFile>/g, 'File[]');
    // Replace any remaining IFormFile with File (for other patterns)
    content = content.replace(/\bIFormFile\b/g, 'File');
    hasChanges = true;
  }

  // Step 2: Fix the body parameter - replace body: file/files with FormData
  // This handles multipart/form-data requests
  // Match: body: file, or body: files, (with optional whitespace)
  const bodyPatterns = [
    // Single file: body: file,
    /(body:\s*)file(\s*,)/g,
    // Single file at end: body: file }
    /(body:\s*)file(\s*\})/g,
    // Multiple files: body: files,
    /(body:\s*)files(\s*,)/g,
    // Multiple files at end: body: files }
    /(body:\s*)files(\s*\})/g,
  ];
  
  bodyPatterns.forEach(pattern => {
    if (pattern.test(content)) {
      content = content.replace(pattern, (match, before, after) => {
        hasChanges = true;
        const isFiles = match.includes('files');
        const formDataVar = isFiles ? 'files' : 'file';
        const appendKey = isFiles ? 'files' : 'file';
        
        if (isFiles) {
          return `${before}(() => {
      const formData = new FormData();
      files.forEach((file, index) => {
        formData.append('files', file);
      });
      return formData;
    })()${after}`;
        } else {
          return `${before}(() => {
      const formData = new FormData();
      formData.append('file', file);
      return formData;
    })()${after}`;
        }
      });
    }
  });
  

  // Step 3: Remove unused IFormFile import and fix File import
  // File is a built-in browser type, so we should not import it from models
  const contentWithoutImports = content.split('\n').filter(line => !line.trim().startsWith('import')).join('\n');
  
  // Remove IFormFile from imports if no longer used
  const hasIFormFileImport = /import type \{ [^}]*IFormFile[^}]*\} from/.test(content);
  const hasIFormFileUsage = contentWithoutImports.includes('IFormFile');
  
  if (hasIFormFileImport && !hasIFormFileUsage) {
    // Remove IFormFile from import statement (handle both single and multiple imports)
    // Pattern: import type { IFormFile } from '...';
    const beforeRemove = content;
    content = content.replace(/import type \{ IFormFile \} from '[^']+';?\n?/g, '');
    // Pattern: import type { IFormFile, ... } from '...';
    content = content.replace(/import type \{ IFormFile,\s*/g, 'import type { ');
    content = content.replace(/,\s*IFormFile\s*/g, ', ');
    // Pattern: import type { ..., IFormFile } from '...';
    content = content.replace(/,\s*IFormFile\s*\}/g, ' }');
    
    if (content !== beforeRemove) {
      hasChanges = true;
    }
  }
  
  // Remove File import from models (File is a built-in browser type, no need to import)
  // Pattern: import type { File } from '../../microsoft/asp-net-core/http/models';
  const fileImportPattern = /import type \{ File \} from '[^']+';?\n?/g;
  if (fileImportPattern.test(content)) {
    content = content.replace(fileImportPattern, '');
    hasChanges = true;
  }
  
  // Also handle case where File is imported with other types from models
  // Pattern: import type { File, OtherType } from '...models';
  // We need to remove File but keep other types
  const multiImportWithFile = /import type \{ ([^}]*)\bFile\b([^}]*)\} from '[^']*models'/g;
  if (multiImportWithFile.test(content)) {
    const beforeReplace = content;
    content = content.replace(multiImportWithFile, (match, before, after) => {
      // Remove File and clean up commas
      let cleaned = (before + after).trim();
      cleaned = cleaned.replace(/,\s*,/g, ','); // Remove double commas
      cleaned = cleaned.replace(/^\s*,\s*/, ''); // Remove leading comma
      cleaned = cleaned.replace(/,\s*$/, ''); // Remove trailing comma
      cleaned = cleaned.trim();
      
      if (cleaned) {
        // Extract the path from the original match
        const pathMatch = match.match(/from '([^']+)'/);
        const path = pathMatch ? pathMatch[1] : '';
        return `import type { ${cleaned} } from '${path}'`;
      } else {
        // If nothing left, remove the entire import line
        return '';
      }
    });
    if (content !== beforeReplace) {
      hasChanges = true;
    }
  }
  
  // Also handle general case: remove File from any import (not just models)
  // But only if it's from models path
  content = content.replace(/import type \{ File,\s*/g, 'import type { ');
  content = content.replace(/,\s*File\s*/g, ', ');
  content = content.replace(/,\s*File\s*\}/g, ' }');

  // Step 4: Fix ResponseDto -> FileResponseDto if FileResponseDto is imported
  // This fixes cases where proxy generator incorrectly generates ResponseDto instead of FileResponseDto
  const hasFileResponseDtoImport = /import.*FileResponseDto.*from/.test(content);
  const hasResponseDtoImport = /import.*\bResponseDto\b.*from/.test(content);
  const hasResponseDtoUsage = /\bResponseDto\b(?!\w)/.test(content);
  
  // If FileResponseDto is imported but ResponseDto is not imported, and ResponseDto is used, replace it
  if (hasFileResponseDtoImport && hasResponseDtoUsage && !hasResponseDtoImport) {
    // Replace ResponseDto with FileResponseDto (but not FileResponseDto itself)
    // Use word boundary to avoid replacing FileResponseDto
    const beforeReplace = content;
    content = content.replace(/\bResponseDto\b(?!\w)/g, 'FileResponseDto');
    if (content !== beforeReplace) {
      hasChanges = true;
    }
  }
  
  // Also handle the case in res-document.service.ts specifically
  // Check if this is the res-document service file
  if (filePath.includes('res-document.service.ts')) {
    const beforeDocFix = content;
    // Replace ResponseDto with FileResponseDto in request type parameters
    content = content.replace(/request<any,\s*ResponseDto(?!\w)/g, 'request<any, FileResponseDto');
    content = content.replace(/request<any,\s*ResponseDto\[\](?!\w)/g, 'request<any, FileResponseDto[]');
    if (content !== beforeDocFix) {
      hasChanges = true;
    }
  }

  // Only write if content changed
  if (content !== originalContent) {
    fs.writeFileSync(filePath, content, 'utf8');
    return true;
  }

  return false;
}

/**
 * Find all service files in proxy directory
 */
function findServiceFiles(dir = PROXY_DIR) {
  const serviceFiles = [];

  if (!fs.existsSync(dir)) {
    return serviceFiles;
  }

  const entries = fs.readdirSync(dir, { withFileTypes: true });

  for (const entry of entries) {
    const fullPath = path.join(dir, entry.name);

    if (entry.isDirectory()) {
      // Recursively scan subdirectories
      serviceFiles.push(...findServiceFiles(fullPath));
    } else if (entry.isFile() && entry.name.endsWith('.service.ts')) {
      serviceFiles.push(fullPath);
    }
  }

  return serviceFiles;
}

// Main execution
console.log('🔍 Scanning for service files with file upload methods...\n');

const serviceFiles = findServiceFiles();
let fixedCount = 0;

for (const filePath of serviceFiles) {
  const relativePath = path.relative(PROXY_DIR, filePath);
  if (fixServiceFile(filePath)) {
    console.log(`✅ Fixed: ${relativePath}`);
    fixedCount++;
  }
}

if (fixedCount === 0) {
  console.log('✨ No service files need fixing.');
} else {
  console.log(`\n✨ Fixed ${fixedCount} file(s).`);
}

