const { execSync } = require('child_process');
const fs = require('fs');
const path = require('path');

/**
 * Script để generate proxy cho tất cả modules được định nghĩa trong generate-proxy.json
 * và tự động fix các DTO còn thiếu
 *
 * Claim module: sau bước `fix-claim-proxy.js`, ClaimPolicyDto được bổ sung `productId` (khớp
 * PolicyClaimLookupDto), claim-task.service được chỉnh `incidentDate` optional, và bổ sung
 * `CancelClaimInput` + `ClaimService.cancel(..., body)` cho PUT hủy YCBT — tránh lỗi TS khi map
 * policy lookup / mở hồ sơ bồi thường / modal hủy claim. Chỉ chạy `abp generate-proxy` đơn lẻ sẽ thiếu
 * các patch này; dùng `npm run generate-proxy:all`.
 *
 * Usage:
 *   npm run generate-proxy:all
 */

const PROXY_JSON_PATH = path.join(__dirname, '../src/app/proxy/generate-proxy.json');
const ABP_HOST = process.env.ABP_HOST || 'https://localhost:44360';

console.log('🚀 Generating proxies for all modules...');
console.log(`   Host: ${ABP_HOST}\n`);

// Read generate-proxy.json to get list of modules
let modules = [];
if (fs.existsSync(PROXY_JSON_PATH)) {
  try {
    const proxyData = JSON.parse(fs.readFileSync(PROXY_JSON_PATH, 'utf8'));
    // Always get all modules from 'modules' object (more reliable)
    if (proxyData.modules && typeof proxyData.modules === 'object') {
      modules = Object.keys(proxyData.modules);
      // Filter out modules that don't have rootPath (these are usually not standalone modules)
      modules = modules.filter(moduleName => {
        const moduleData = proxyData.modules[moduleName];
        return moduleData && moduleData.rootPath;
      });
      // Sort modules: abp and audit-logging first, then others
      modules.sort((a, b) => {
        if (a === 'abp') return -1;
        if (b === 'abp') return 1;
        if (a === 'audit-logging') return -1;
        if (b === 'audit-logging') return 1;
        return a.localeCompare(b);
      });
    }
    // Fallback to 'generated' field if modules object is not available
    else if (proxyData.generated && Array.isArray(proxyData.generated) && proxyData.generated.length > 0) {
      modules = proxyData.generated;
    }
  } catch (error) {
    console.warn('⚠️  Could not read generate-proxy.json, using default modules');
    modules = ['abp', 'audit-logging'];
  }
} else {
  console.warn('⚠️  generate-proxy.json not found, using default modules');
  modules = ['abp', 'audit-logging'];
}

if (modules.length === 0) {
  console.error('❌ No modules found to generate!');
  process.exit(1);
}

console.log(`📋 Found ${modules.length} module(s) to generate:`);
modules.forEach((m, i) => console.log(`   ${i + 1}. ${m}`));
console.log('');

// Generate proxy for each module
let successCount = 0;
let failCount = 0;

for (const moduleName of modules) {
  try {
    console.log(`\n📦 Generating proxy for module: ${moduleName}`);
    const command = `abp generate-proxy -t ng -u ${ABP_HOST} -m ${moduleName}`;
    console.log(`   Running: ${command}`);
    
    execSync(command, { stdio: 'inherit' });
    
    console.log(`   ✅ Success: ${moduleName}`);
    successCount++;
  } catch (error) {
    console.error(`   ❌ Failed: ${moduleName}`);
    console.error(`   Error: ${error.message}`);
    failCount++;
  }
}

console.log('\n' + '='.repeat(50));
console.log(`📊 Summary:`);
console.log(`   ✅ Success: ${successCount}`);
console.log(`   ❌ Failed: ${failCount}`);
console.log('='.repeat(50));

if (successCount > 0) {
  console.log('\n🔧 Fixing missing DTOs...\n');
  try {
    execSync('node scripts/generate-missing-dtos.js', { stdio: 'inherit' });
    console.log('\n🔧 Fixing res-documents DTOs...\n');
    execSync('node scripts/fix-res-documents-dtos.js', { stdio: 'inherit' });
    console.log('\n🔧 Generating missing service files...\n');
    execSync('node scripts/generate-missing-services.js', { stdio: 'inherit' });
    console.log('\n🔧 Fixing service URL template strings...\n');
    execSync('node scripts/fix-proxy-service-urls.js', { stdio: 'inherit' });
    console.log('\n🔧 Fixing enum imports...\n');
    execSync('node scripts/fix-enum-imports.js', { stdio: 'inherit' });
    console.log('\n🔧 Fixing reserved keyword "enum" as type...\n');
    execSync('node scripts/fix-enum-type-keyword.js', { stdio: 'inherit' });
    console.log('\n🔧 Fixing invalid types (interface any, extends any, SendResponse)...\n');
    execSync('node scripts/fix-proxy-invalid-types.js', { stdio: 'inherit' });
    console.log('\n🔧 Fixing microsoft MVC IActionResult alias (ABP proxy vs generated models)...\n');
    execSync('node scripts/fix-microsoft-mvc-iaction-result.js', { stdio: 'inherit' });
    console.log('\n🔧 Fixing StringValues type...\n');
    execSync('node scripts/fix-string-values.js', { stdio: 'inherit' });
    console.log('\n🔧 Fixing file upload methods...\n');
    execSync('node scripts/fix-proxy-file-uploads.js', { stdio: 'inherit' });
    console.log('\n🔧 Fixing claim proxy (ClaimPolicyDto.productId, CancelClaimInput + cancel body, claim-task params, ...)\n');
    execSync('node scripts/fix-claim-proxy.js', { stdio: 'inherit' });
    console.log('\n🔧 Fixing product proxy models (certificateTemplateDocumentId, ...)\n');
    execSync('node scripts/fix-product-proxy-models.js', { stdio: 'inherit' });
    console.log('\n🔧 Fixing claim task proxy paths (shim files)...\n');
    execSync('node scripts/fix-claim-task-proxy-paths.js', { stdio: 'inherit' });
    console.log('\n🔧 Fixing app Policy DTOs...\n');
    execSync('node scripts/fix-app-policy-dtos.js', { stdio: 'inherit' });
    console.log('\n🔧 Fixing claim snapshot link service...\n');
    execSync('node scripts/fix-claim-snapshot-link-service.js', { stdio: 'inherit' });
    console.log('\n✨ All done!');
  } catch (error) {
    console.error('\n⚠️  Error fixing DTOs/services/enums:', error.message);
  }
}

if (failCount > 0) {
  process.exit(1);
}
