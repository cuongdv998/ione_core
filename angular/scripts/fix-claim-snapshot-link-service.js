const fs = require('fs');
const path = require('path');

/**
 * Đảm bảo ClaimService trong proxy có method getSnapshotLink
 * sau mỗi lần generate-proxy:all.
 */

const SERVICE_PATH = path.join(__dirname, '../src/app/proxy/claim/controllers/claim.service.ts');

function ensureGetSnapshotLink() {
  if (!fs.existsSync(SERVICE_PATH)) {
    return false;
  }

  let content = fs.readFileSync(SERVICE_PATH, 'utf8');
  const original = content;

  if (content.includes('getSnapshotLink = (id: string')) {
    return false;
  }

  const getMarker = '  get = (id: string, config?: Partial<Rest.Config>) =>';
  const getListMarker = '  getList = (input: GetClaimsInput, config?: Partial<Rest.Config>) =>';

  const getIndex = content.indexOf(getMarker);
  const getListIndex = content.indexOf(getListMarker);

  if (getIndex === -1 || getListIndex === -1) {
    return false;
  }

  const before = content.slice(0, getListIndex);
  const after = content.slice(getListIndex);

  const snippet = `

  getSnapshotLink = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, string>({
      method: 'POST',
      url: \`/api/claims/\${id}/snapshot-link\`,
    },
    { apiName: this.apiName,...config });
`;

  content = before + snippet + after;

  if (content !== original) {
    fs.writeFileSync(SERVICE_PATH, content, 'utf8');
    return true;
  }

  return false;
}

function main() {
  console.log('🔍 Ensuring ClaimService has getSnapshotLink...\n');
  const fixed = ensureGetSnapshotLink();
  if (fixed) {
    console.log('✅ Fixed: claim/controllers/claim.service.ts (added getSnapshotLink).');
  } else {
    console.log('✨ No changes needed for claim/controllers/claim.service.ts.');
  }
}

main();

