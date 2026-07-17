const fs = require('fs');
const path = require('path');

/**
 * Fix lỗi TS2304: Cannot find name 'enum' trong proxy models.
 * ABP generate-proxy đôi khi sinh ra type "enum" hoặc "enum[]" (reserved keyword trong TypeScript).
 *
 * Cách xử lý:
 * 1. Trong mỗi interface, nếu có property dạng "statuses?: enum[]" và cùng interface có "status?: X"
 *    thì thay "statuses?: enum[]" bằng "statuses?: X[]".
 * 2. Mọi chỗ còn lại dùng "enum" hoặc "enum[]" làm type thì thay bằng "any" / "any[]".
 *
 * Chạy sau generate-proxy (được gọi từ generate-proxy-all.js).
 *
 * Usage: node scripts/fix-enum-type-keyword.js
 */

const PROXY_DIR = path.join(__dirname, '../src/app/proxy');

/**
 * Tìm tất cả file models.ts trong proxy
 */
function findModelsFiles(dir, list = []) {
  if (!fs.existsSync(dir)) return list;
  const entries = fs.readdirSync(dir, { withFileTypes: true });
  for (const entry of entries) {
    const fullPath = path.join(dir, entry.name);
    if (entry.isDirectory()) {
      findModelsFiles(fullPath, list);
    } else if (entry.isFile() && entry.name === 'models.ts') {
      list.push(fullPath);
    }
  }
  return list;
}

/**
 * Trong đoạn nội dung của một interface, tìm type của property "status" (hoặc tương tự)
 * để suy ra type cho "statuses" (số nhiều). Ví dụ: status?: PolicyContractStatus -> PolicyContractStatus
 */
function inferEnumTypeFromInterfaceContent(interfaceContent) {
  // status?: SomeEnum; hoặc status?: SomeEnum }
  const statusMatch = interfaceContent.match(/status\s*\?\s*:\s*([A-Za-z][A-Za-z0-9]*)\s*[;}\[\]]/);
  if (statusMatch) return statusMatch[1];
  return null;
}

/**
 * Fix nội dung file: thay type "enum" / "enum[]" (reserved keyword) bằng type đúng hoặc any/any[]
 */
function fixContent(content) {
  let out = content;

  // 1) Fix theo từng interface: nếu có "statuses?: enum[]" và "status?: X" thì dùng X[]
  const interfaceRegex = /export\s+interface\s+(\w+)[^{]*\{([^}]*(?:\{[^}]*\}[^}]*)*)\}/g;
  let match;
  while ((match = interfaceRegex.exec(content)) !== null) {
    const interfaceName = match[1];
    const body = match[2];
    if (!body.includes('enum[]') && !body.includes('?: enum;') && !body.includes(': enum;')) continue;
    const inferredType = inferEnumTypeFromInterfaceContent(body);
    if (inferredType && body.includes('statuses?: enum[]')) {
      const wrong = 'statuses?: enum[]';
      const right = `statuses?: ${inferredType}[]`;
      out = out.replace(wrong, right);
    }
  }

  // 2) Thay mọi type "enum[]" còn lại bằng "any[]"
  out = out.replace(/\?\s*:\s*enum\[\]/g, '?: any[]');
  out = out.replace(/\:\s*enum\[\]/g, ': any[]');

  // 3) Thay type "enum" (không phải "export enum") bằng "any"
  // Tránh sửa "export enum X" hoặc "enum X {"
  out = out.replace(/(\?\s*:\s*)enum(\s*[;,\]}\s])/g, '$1any$2');
  out = out.replace(/(:\s*)enum(\s*[;,\]}\s])/g, '$1any$2');

  return out;
}

function main() {
  console.log('🔧 Fixing reserved keyword "enum" as type in proxy models...\n');

  const modelsFiles = findModelsFiles(PROXY_DIR);
  let fixedCount = 0;

  for (const filePath of modelsFiles) {
    const content = fs.readFileSync(filePath, 'utf8');
    if (!content.includes('enum[]') && !content.match(/\?\s*:\s*enum\s*[;,\]}]/) && !content.match(/\:\s*enum\s*[;,\]}]/)) {
      continue;
    }
    const newContent = fixContent(content);
    if (newContent !== content) {
      fs.writeFileSync(filePath, newContent, 'utf8');
      const relativePath = path.relative(process.cwd(), filePath);
      console.log(`✅ Fixed: ${relativePath}`);
      fixedCount++;
    }
  }

  if (fixedCount === 0) {
    console.log('✨ No "enum" type usage to fix.');
  } else {
    console.log(`\n✨ Fixed ${fixedCount} file(s).`);
  }
}

main();
