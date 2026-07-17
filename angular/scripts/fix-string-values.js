const fs = require('fs');
const path = require('path');

/**
 * Script để tự động fix StringValues type trong các file models.ts
 * Chạy sau khi generate proxy để fix lỗi StringValues không được định nghĩa
 * 
 * Usage: node scripts/fix-string-values.js
 */

const PROXY_DIR = path.join(__dirname, '../src/app/proxy');
const TARGET_FILE = path.join(PROXY_DIR, 'microsoft/asp-net-core/http/models.ts');
const STRING_VALUES_TYPE = 'export type StringValues = string | string[];\n';

/**
 * Fix StringValues type trong file models.ts
 */
function fixStringValues(filePath) {
  if (!fs.existsSync(filePath)) {
    return false;
  }

  let content = fs.readFileSync(filePath, 'utf8');
  const originalContent = content;

  // Check if StringValues is used but not defined
  const usesStringValues = /StringValues/.test(content);
  const hasStringValuesType = /export\s+type\s+StringValues/.test(content);

  if (usesStringValues && !hasStringValuesType) {
    // Add StringValues type definition at the beginning of the file
    // If file starts with newline, add after it, otherwise add at the start
    if (content.trim().length === 0) {
      content = STRING_VALUES_TYPE;
    } else if (content.startsWith('\n')) {
      content = '\n' + STRING_VALUES_TYPE + content.substring(1);
    } else {
      content = STRING_VALUES_TYPE + '\n' + content;
    }

    fs.writeFileSync(filePath, content, 'utf8');
    return content !== originalContent;
  }

  return false;
}

/**
 * Main function
 */
function main() {
  console.log('🔍 Fixing StringValues type definition...\n');

  let fixedCount = 0;

  if (fixStringValues(TARGET_FILE)) {
    const relativePath = path.relative(PROXY_DIR, TARGET_FILE);
    console.log(`✅ Fixed: ${relativePath}`);
    fixedCount++;
  }

  if (fixedCount === 0) {
    console.log('✨ No StringValues issues to fix.');
  } else {
    console.log(`\n✨ Fixed ${fixedCount} file(s).`);
  }
}

main();


