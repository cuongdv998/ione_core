const fs = require('fs');
const path = require('path');

/**
 * Tạo các file shim để import đúng đường dẫn mà app đang dùng,
 * không sửa file proxy do abp generate-proxy sinh ra.
 *
 * - proxy/claims/work-task-status.enum.ts: re-export từ work-tasks (app import @/proxy/claims/work-task-status.enum)
 * - proxy/claim/claims/claim-task-models.ts: re-export ClaimTaskDto, GetClaimTasksInput từ models (app import @/proxy/claim/claims/claim-task-models)
 *
 * Usage: node scripts/fix-claim-task-proxy-paths.js
 */

const PROXY_DIR = path.join(__dirname, '../src/app/proxy');
const CLAIMS_DIR = path.join(PROXY_DIR, 'claims');
const CLAIM_CLAIMS_DIR = path.join(PROXY_DIR, 'claim/claims');

const WORK_TASK_STATUS_SHIM = path.join(CLAIMS_DIR, 'work-task-status.enum.ts');
const CLAIM_TASK_MODELS_SHIM = path.join(CLAIM_CLAIMS_DIR, 'claim-task-models.ts');

function ensureDir(dir) {
  if (!fs.existsSync(dir)) {
    fs.mkdirSync(dir, { recursive: true });
  }
}

function createWorkTaskStatusShim() {
  ensureDir(CLAIMS_DIR);
  const content = `/** Re-export for app import @/proxy/claims/work-task-status.enum (generated enum lives in work-tasks) */
export { WorkTaskStatus, workTaskStatusOptions } from '../work-tasks/work-task-status.enum';
`;
  fs.writeFileSync(WORK_TASK_STATUS_SHIM, content, 'utf8');
  return true;
}

function createClaimTaskModelsShim() {
  if (!fs.existsSync(path.join(CLAIM_CLAIMS_DIR, 'models.ts'))) {
    return false;
  }
  ensureDir(CLAIM_CLAIMS_DIR);
  const content = `/** Re-export for app import @/proxy/claim/claims/claim-task-models (types live in models.ts) */
export type { ClaimTaskDto, GetClaimTasksInput } from './models';
`;
  fs.writeFileSync(CLAIM_TASK_MODELS_SHIM, content, 'utf8');
  return true;
}

function main() {
  console.log('🔧 Fixing claim task proxy paths (shim files)...\n');

  let count = 0;
  if (createWorkTaskStatusShim()) {
    console.log('✅ Created: proxy/claims/work-task-status.enum.ts');
    count++;
  }
  if (createClaimTaskModelsShim()) {
    console.log('✅ Created: proxy/claim/claims/claim-task-models.ts');
    count++;
  }
  if (count === 0) {
    console.log('⚠️  Skipped (claim proxy not generated yet). Run after generate-proxy:all.');
  } else {
    console.log(`\n✨ Created ${count} shim file(s).`);
  }
}

main();
