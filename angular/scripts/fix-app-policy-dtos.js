const fs = require('fs');
const path = require('path');

/**
 * Script fix các DTO Policy trong app/models.ts sau khi generate proxy.
 * Mục tiêu:
 * - Đồng bộ kiểu của PolicyDto (bản rút gọn) với PolicyDto đầy đủ ở cuối file,
 *   để tránh lỗi TS2717 khi interface được merge.
 *
 * Usage: node scripts/fix-app-policy-dtos.js
 */

const APP_MODELS_PATH = path.join(__dirname, '../src/app/proxy/app/models.ts');

function fixPolicyDtos() {
  if (!fs.existsSync(APP_MODELS_PATH)) {
    return false;
  }

  let content = fs.readFileSync(APP_MODELS_PATH, 'utf8');
  const originalContent = content;

  // Chúng ta chỉ sửa PolicyDto rút gọn ở phía trên file, có structure:
  // export interface PolicyDto {
  //   policyId?: string;
  //   lobName?: string;
  //   contractId?: string;
  //   certificateNo?: string;
  //   products?: string[];
  //   ...
  //   status?: string;
  // }
  //
  // Đảm bảo nó dùng chung kiểu với PolicyDto đầy đủ:
  //   products?: PolicyProductDetailDto[];
  //   status?: PolicyStatus;

  // PolicyDto rút gọn: "export interface PolicyDto {" (không có "extends").
  // PolicyDto đầy đủ: "export interface PolicyDto extends FullAuditedEntityDto<string> {".
  // Chỉ sửa block rút gọn; block kết thúc bằng "}\n\n\nexport interface " (bất kỳ interface tiếp theo).
  const startMarker = 'export interface PolicyDto {';
  const startIndex = content.indexOf(startMarker);
  if (startIndex === -1) {
    return false;
  }

  // Tránh trùng block PolicyDto extends (đầy đủ) — chỉ lấy block đầu tiên (rút gọn).
  const endMarker = '}\n\n\nexport interface ';
  const endMarkerAlt = '}\n\nexport interface ';
  let endIndex = content.indexOf(endMarker, startIndex);
  if (endIndex === -1) {
    endIndex = content.indexOf(endMarkerAlt, startIndex);
  }
  if (endIndex === -1 || endIndex <= startIndex) {
    return false;
  }

  // Block từ startIndex đến hết dấu "}" (bao gồm "}").
  const blockEnd = endIndex + 1;
  const before = content.slice(0, startIndex);
  const block = content.slice(startIndex, blockEnd);
  const after = content.slice(blockEnd);

  let fixedBlock = block;

  // Đổi products?: string[]; -> products?: PolicyProductDetailDto[];
  fixedBlock = fixedBlock.replace(
    /products\?:\s*string\[\];/,
    'products?: PolicyProductDetailDto[];'
  );

  // Đổi status?: string; -> status?: PolicyStatus;
  fixedBlock = fixedBlock.replace(
    /status\?:\s*string;/,
    'status?: PolicyStatus;'
  );

  // Ghép lại nội dung file
  content = before + fixedBlock + after;

  if (content !== originalContent) {
    fs.writeFileSync(APP_MODELS_PATH, content, 'utf8');
    return true;
  }

  return false;
}

function main() {
  console.log('🔍 Fixing app Policy DTOs in app/models.ts...\n');

  const fixed = fixPolicyDtos();

  if (fixed) {
    console.log('✅ Fixed: app/proxy/app/models.ts (PolicyDto types synchronized).');
  } else {
    console.log('✨ No changes needed for app/proxy/app/models.ts.');
  }
}

main();

