const fs = require('fs');
const path = require('path');

/**
 * Script để fix các vấn đề sau khi generate proxy cho claim module
 * Chạy sau khi generate proxy để fix các vấn đề có thể xảy ra
 * 
 * Usage: node scripts/fix-claim-proxy.js
 */

const PROXY_DIR = path.join(__dirname, '../src/app/proxy');
const CLAIM_MODELS_PATH = path.join(PROXY_DIR, 'claim/claims/models.ts');
const CLAIM_SERVICE_PATH = path.join(PROXY_DIR, 'claim/controllers/claim.service.ts');
const CLAIM_TASK_SERVICE_PATH = path.join(PROXY_DIR, 'claim/controllers/claim-task.service.ts');
const CLAIM_CONTROLLERS_INDEX_PATH = path.join(PROXY_DIR, 'claim/controllers/index.ts');
const DETAILED_EVALUATION_SERVICE_PATH = path.join(
  PROXY_DIR,
  'claim/controllers/claim-detailed-assessment-evaluation.service.ts'
);

/**
 * @returns {{ innerStart: number; innerEnd: number } | null} inner is content.slice(innerStart, innerEnd) inside `{` `}` of export interface Name
 */
function getExportInterfaceInnerRange(content, interfaceName) {
  const marker = `export interface ${interfaceName}`;
  const idx = content.indexOf(marker);
  if (idx === -1) {
    return null;
  }
  const braceOpen = content.indexOf('{', idx);
  if (braceOpen === -1) {
    return null;
  }
  let depth = 0;
  for (let i = braceOpen; i < content.length; i++) {
    const c = content[i];
    if (c === '{') {
      depth++;
    } else if (c === '}') {
      depth--;
      if (depth === 0) {
        return { innerStart: braceOpen + 1, innerEnd: i };
      }
    }
  }
  return null;
}

/**
 * Append a property line inside export interface Name if prop name not already declared.
 */
function appendOptionalPropertyToInterface(content, interfaceName, propertyLine) {
  const range = getExportInterfaceInnerRange(content, interfaceName);
  if (!range) {
    return content;
  }
  const inner = content.slice(range.innerStart, range.innerEnd);
  const propMatch = propertyLine.match(/\b([a-zA-Z0-9_]+)\??\s*:/);
  const propName = propMatch ? propMatch[1] : null;
  if (propName && new RegExp(`\\b${propName}\\??\\s*:`).test(inner)) {
    return content;
  }
  const line = propertyLine.endsWith('\n') ? propertyLine : `${propertyLine}\n`;
  const trimmedInner = inner.replace(/\s*$/, '');
  const join = trimmedInner.length === 0 ? '' : trimmedInner.endsWith('\n') ? '' : '\n';
  return (
    content.slice(0, range.innerStart) +
    trimmedInner +
    join +
    line +
    content.slice(range.innerEnd)
  );
}

/**
 * OpenAPI thường không sinh DTO cho body PUT cancel — thêm interface để claim-detail modal type-safe.
 */
function ensureCancelClaimInput(content) {
  if (/\bexport\s+interface\s+CancelClaimInput\b/.test(content)) {
    return content;
  }

  const marker = 'export interface AssignOnsiteAssessmentInput';
  const idx = content.indexOf(marker);
  if (idx === -1) {
    return content;
  }

  const braceOpen = content.indexOf('{', idx);
  let depth = 0;
  let i = braceOpen;
  for (; i < content.length; i++) {
    const c = content[i];
    if (c === '{') {
      depth++;
    } else if (c === '}') {
      depth--;
      if (depth === 0) {
        i++;
        break;
      }
    }
  }

  const snippet = `

/** Body cho PUT /api/claims/{id}/cancel (claim-detail: reasonId + reasonDescription). */
export interface CancelClaimInput {
  reasonId?: string;
  reasonDescription?: string;
}
`;
  return content.slice(0, i) + snippet + content.slice(i);
}

/**
 * Backend có các field này nhưng OpenAPI/proxy đôi khi thiếu — bổ sung để `npm start` / CI không lỗi TS.
 */
function ensureClaimFolderAndTaskModels(content) {
  let out = content;

  for (const line of [
    '  incidentObjectIds?: string[];',
    '  assigneeOrganizationId?: string;',
    '  assessmentStartDate?: string;',
  ]) {
    out = appendOptionalPropertyToInterface(out, 'CreateClaimFolderDto', line);
  }

  for (const line of [
    '  isReporter?: boolean;',
    '  isAssignee?: boolean;',
    '  canReassign?: boolean;',
    '  hasFinishedOnsiteAssessment?: boolean;',
  ]) {
    out = appendOptionalPropertyToInterface(out, 'ClaimTaskDto', line);
  }

  // ClaimDetailDto: backend trả về openEmployeeId nhưng OpenAPI spec đôi khi thiếu.
  out = appendOptionalPropertyToInterface(out, 'ClaimDetailDto', '  openEmployeeId?: string;');

  if (!/\bexport\s+interface\s+ReassignDetailedAssessmentInput\b/.test(out)) {
    out =
      out.trimEnd() +
      `

export interface ReassignDetailedAssessmentInput {
  assigneeOrganizationId: string;
  assigneeId: string;
  startDate: string;
}
`;
  }

  return out;
}

/**
 * ClaimPolicyDto must align with PolicyClaimLookupDto (ProductId vs product names in Products).
 * Find the interface by brace depth so this works regardless of field order or line endings.
 */
function ensureClaimPolicyDtoProductId(content) {
  const marker = 'export interface ClaimPolicyDto';
  const idx = content.indexOf(marker);
  if (idx === -1) {
    return content;
  }
  const braceOpen = content.indexOf('{', idx);
  if (braceOpen === -1) {
    return content;
  }
  let depth = 0;
  let i = braceOpen;
  for (; i < content.length; i++) {
    const c = content[i];
    if (c === '{') {
      depth++;
    } else if (c === '}') {
      depth--;
      if (depth === 0) {
        i++;
        break;
      }
    }
  }
  const block = content.slice(idx, i);
  if (!block.includes('products: string[]') || /\bproductId\?:\s*string\b/.test(block)) {
    return content;
  }
  const updated = block.replace(/(\n\s*products:\s*string\[];)(\r?\n)/, '$1$2  productId?: string;$2');
  if (updated === block) {
    return content;
  }
  return content.slice(0, idx) + updated + content.slice(i);
}

/**
 * Fix enum imports in claim models.ts
 */
function fixClaimModels() {
  if (!fs.existsSync(CLAIM_MODELS_PATH)) {
    return false;
  }

  let content = fs.readFileSync(CLAIM_MODELS_PATH, 'utf8');
  const originalContent = content;

  // Fix enum imports - ensure they use correct relative paths
  // From: claim/claims/models.ts
  // To: claims/process-claim-type.enum.ts and claims/claim-status.enum.ts
  // Relative path: ../../claims/
  
  // Check if imports are correct
  const hasProcessClaimTypeImport = /import.*ProcessClaimType.*from/.test(content);
  const hasClaimStatusImport = /import.*ClaimStatus.*from/.test(content);
  
  // Fix ProcessClaimType import if needed
  if (hasProcessClaimTypeImport) {
    // Replace any incorrect import paths
    content = content.replace(
      /import\s+type\s+\{\s*ProcessClaimType\s*\}\s+from\s+['"]([^'"]+)['"]/g,
      (match, importPath) => {
        // If path doesn't match expected, fix it
        if (!importPath.includes('claims/process-claim-type.enum')) {
          return `import type { ProcessClaimType } from '../../claims/process-claim-type.enum'`;
        }
        return match;
      }
    );
  } else {
    // Add missing import
    const importLine = `import type { ProcessClaimType } from '../../claims/process-claim-type.enum';\n`;
    // Insert after first import line
    const firstImportIndex = content.indexOf('import');
    if (firstImportIndex !== -1) {
      const nextLineIndex = content.indexOf('\n', firstImportIndex);
      content = content.slice(0, nextLineIndex + 1) + importLine + content.slice(nextLineIndex + 1);
    }
  }

  // Fix ClaimStatus import if needed
  if (hasClaimStatusImport) {
    // Replace any incorrect import paths
    content = content.replace(
      /import\s+type\s+\{\s*ClaimStatus\s*\}\s+from\s+['"]([^'"]+)['"]/g,
      (match, importPath) => {
        // If path doesn't match expected, fix it
        if (!importPath.includes('claims/claim-status.enum')) {
          return `import type { ClaimStatus } from '../../claims/claim-status.enum'`;
        }
        return match;
      }
    );
  } else {
    // Add missing import
    const importLine = `import type { ClaimStatus } from '../../claims/claim-status.enum';\n`;
    // Insert after ProcessClaimType import or first import
    const processClaimTypeIndex = content.indexOf('ProcessClaimType');
    if (processClaimTypeIndex !== -1) {
      const nextLineIndex = content.indexOf('\n', processClaimTypeIndex);
      content = content.slice(0, nextLineIndex + 1) + importLine + content.slice(nextLineIndex + 1);
    } else {
      const firstImportIndex = content.indexOf('import');
      if (firstImportIndex !== -1) {
        const nextLineIndex = content.indexOf('\n', firstImportIndex);
        content = content.slice(0, nextLineIndex + 1) + importLine + content.slice(nextLineIndex + 1);
      }
    }
  }

  // Ensure PolicyDto has correct type for products array
  if (content.includes('products: string[]')) {
    // This is correct, no change needed
  }

  // Ensure CreateClaimResponseDto has correct type for policies array
  // Fix PolicyDto -> ClaimPolicyDto to avoid conflict with app/models.ts PolicyDto
  if (content.includes('policies: PolicyDto[]')) {
    content = content.replace(/policies:\s*PolicyDto\[\]/g, 'policies: ClaimPolicyDto[]');
  }
  
  // Rename PolicyDto to ClaimPolicyDto to avoid conflict with app/models.ts PolicyDto
  if (content.includes('export interface PolicyDto')) {
    content = content.replace(/export interface PolicyDto/g, 'export interface ClaimPolicyDto');
  }

  content = ensureClaimPolicyDtoProductId(content);

  // Ensure ClaimPolicyDto has paymentStatus (backend PolicyDto/PolicyClaimLookupDto có PaymentStatus;
  // nếu generate proxy từ spec cũ thì có thể thiếu, nên patch vào để mapPolicyLookupToClaimPolicy không lỗi)
  if (content.includes('export interface ClaimPolicyDto') && !content.includes('paymentStatus?: string')) {
    content = content.replace(
      /(\s*status\?\s*:\s*string;\s*)(\r?\n)(\s*certificateUrl)/,
      '$1$2  paymentStatus?: string;$2$3'
    );
  }

  // Ensure ClaimPolicyDto has insurerId and insurerName (backend PolicyDto has InsurerId, InsurerName;
  // nếu generate proxy từ spec cũ thì có thể thiếu, nên patch vào để npm start không lỗi)
  if (content.includes('export interface ClaimPolicyDto') && !content.includes('insurerId?: string')) {
    const newBlock = '  certificateUrl?: string;\n  insurerId?: string;\n  insurerName?: string;\n}';
    if (content.includes('  certificateUrl?: string;\n}')) {
      content = content.replace('  certificateUrl?: string;\n}', newBlock);
    } else if (content.includes('  certificateUrl?: string;\r\n}')) {
      content = content.replace('  certificateUrl?: string;\r\n}', newBlock);
    } else {
      content = content.replace(
        /(\s*certificateUrl\?\s*:\s*string;\s*)(\r?\n)(\s*})/,
        '$1$2  insurerId?: string;$2  insurerName?: string;$2$3'
      );
    }
  }

  // Also fix any references to PolicyDto in CreateClaimResponseDto
  if (content.includes('policies?: PolicyDto[]')) {
    content = content.replace(/policies\?:\s*PolicyDto\[\]/g, 'policies?: ClaimPolicyDto[]');
  }

  content = ensureClaimFolderAndTaskModels(content);

  content = ensureCancelClaimInput(content);

  if (content !== originalContent) {
    fs.writeFileSync(CLAIM_MODELS_PATH, content, 'utf8');
    return true;
  }

  return false;
}

/**
 * Optional incidentDate matches API DateTime?; avoids TS errors when passing undefined.
 */
function fixClaimTaskService() {
  if (!fs.existsSync(CLAIM_TASK_SERVICE_PATH)) {
    return false;
  }

  let content = fs.readFileSync(CLAIM_TASK_SERVICE_PATH, 'utf8');
  const originalContent = content;

  content = content.replace(
    /getObjectTypesByPolicyAndProduct = \(policyId: string, productId: string, incidentDate: string,/,
    'getObjectTypesByPolicyAndProduct = (policyId: string, productId: string, incidentDate?: string,'
  );

  if (content !== originalContent) {
    fs.writeFileSync(CLAIM_TASK_SERVICE_PATH, content, 'utf8');
    return true;
  }

  return false;
}

/**
 * Thêm CancelClaimInput vào import từ ../claims/models nếu thiếu.
 */
function ensureClaimServiceImportsCancelClaimInput(content) {
  const re = /import type \{([^}]+)\} from '\.\.\/claims\/models'/;
  const m = content.match(re);
  if (!m) {
    return content;
  }
  if (/\bCancelClaimInput\b/.test(m[1])) {
    return content;
  }
  const types = m[1]
    .split(',')
    .map((t) => t.trim())
    .filter(Boolean);
  types.push('CancelClaimInput');
  types.sort((a, b) => a.localeCompare(b));
  return content.replace(re, `import type { ${types.join(', ')} } from '../claims/models'`);
}

/**
 * PUT /api/claims/{id}/cancel — proxy chỉ có id; bổ sung body CancelClaimInput (optional cho claim-list).
 */
function patchClaimServiceCancelMethod(content) {
  if (/cancel\s*=\s*\([^)]*CancelClaimInput/.test(content)) {
    return content;
  }

  const replacedSignature = content.replace(
    /cancel\s*=\s*\(id:\s*string,\s*config\?\s*:\s*Partial<Rest\.Config>\)/,
    'cancel = (id: string, input?: CancelClaimInput, config?: Partial<Rest.Config>)',
  );

  let next = replacedSignature;
  next = next.replace(
    /(url:\s*`\/api\/claims\/\$\{id\}\/cancel`,)\s*\r?\n(\s*)\},/,
    '$1\n$2  body: input ?? {},\n$2},',
  );

  return next;
}

/**
 * Fix claim service file
 */
function fixClaimService() {
  if (!fs.existsSync(CLAIM_SERVICE_PATH)) {
    return false;
  }

  let content = fs.readFileSync(CLAIM_SERVICE_PATH, 'utf8');
  const originalContent = content;

  content = ensureClaimServiceImportsCancelClaimInput(content);
  content = patchClaimServiceCancelMethod(content);

  // Ensure imports are correct
  // Check if CreateClaimResponseDto is imported
  const hasCreateClaimResponseDtoImport = /import.*CreateClaimResponseDto.*from/.test(content);
  
  if (!hasCreateClaimResponseDtoImport && content.includes('CreateClaimResponseDto')) {
    // Add missing import
    const importMatch = content.match(/import type \{ ([^}]+) \} from '\.\.\/claims\/models'/);
    if (importMatch) {
      const existingImports = importMatch[1].split(',').map(i => i.trim());
      if (!existingImports.includes('CreateClaimResponseDto')) {
        existingImports.push('CreateClaimResponseDto');
        const newImport = `import type { ${existingImports.join(', ')} } from '../claims/models';`;
        content = content.replace(/import type \{ [^}]+ \} from '\.\.\/claims\/models'/, newImport);
      }
    }
  }

  // Ensure URL template strings are correct (if needed)
  // Check if URL contains template strings
  const urlPattern = /url:\s*'([^']*\$\{[^}]+\}[^']*)',/g;
  if (urlPattern.test(content)) {
    // Fix URL template strings
    content = content.replace(urlPattern, (match) => {
      const urlMatch = match.match(/url:\s*'([^']+)',/);
      if (urlMatch && urlMatch[1]) {
        const urlPath = urlMatch[1];
        return `url: \`${urlPath}\`,`;
      }
      return match;
    });
  }

  if (content !== originalContent) {
    fs.writeFileSync(CLAIM_SERVICE_PATH, content, 'utf8');
    return true;
  }

  return false;
}

/**
 * Remove generated detailed assessment evaluation proxy service if it is inconsistent
 * with the generated claim models. The app uses a local service wrapper instead.
 */
function removeBrokenDetailedAssessmentEvaluationProxy() {
  let changed = false;

  if (fs.existsSync(DETAILED_EVALUATION_SERVICE_PATH)) {
    fs.unlinkSync(DETAILED_EVALUATION_SERVICE_PATH);
    changed = true;
  }

  if (fs.existsSync(CLAIM_CONTROLLERS_INDEX_PATH)) {
    const content = fs.readFileSync(CLAIM_CONTROLLERS_INDEX_PATH, 'utf8');
    const updated = content
      .split(/\r?\n/)
      .filter(line => !line.includes('claim-detailed-assessment-evaluation.service'))
      .join('\n');

    if (updated !== content) {
      fs.writeFileSync(CLAIM_CONTROLLERS_INDEX_PATH, updated, 'utf8');
      changed = true;
    }
  }

  return changed;
}

/**
 * Main function
 */
function main() {
  console.log('🔍 Fixing claim proxy files...\n');

  let fixedCount = 0;

  if (fixClaimModels()) {
    console.log('✅ Fixed: claim/claims/models.ts');
    fixedCount++;
  }

  if (fixClaimService()) {
    console.log('✅ Fixed: claim/controllers/claim.service.ts');
    fixedCount++;
  }

  if (fixClaimTaskService()) {
    console.log('✅ Fixed: claim/controllers/claim-task.service.ts');
    fixedCount++;
  }

  if (removeBrokenDetailedAssessmentEvaluationProxy()) {
    console.log('✅ Fixed: claim/controllers/claim-detailed-assessment-evaluation.service.ts');
    fixedCount++;
  }

  if (fixedCount === 0) {
    console.log('✨ No claim proxy files need fixing.');
  } else {
    console.log(`\n✨ Fixed ${fixedCount} file(s).`);
  }
}

main();
