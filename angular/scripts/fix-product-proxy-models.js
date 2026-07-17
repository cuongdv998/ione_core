const fs = require('fs');
const path = require('path');

/**
 * Bổ sung field backend đã có nhưng proxy TypeScript đôi khi thiếu sau generate-proxy.
 * Không sửa tay file trong proxy; chạy từ generate-proxy-all.js.
 *
 * Usage: node scripts/fix-product-proxy-models.js
 */

const PROXY_DIR = path.join(__dirname, '../src/app/proxy');
const PRODUCT_MODELS_PATH = path.join(PROXY_DIR, 'product/pro-products/models.ts');

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

function main() {
  console.log('🔧 Patching product/pro-products/models.ts (certificateTemplateDocumentId)...\n');

  if (!fs.existsSync(PRODUCT_MODELS_PATH)) {
    console.log('⚠️  File not found (skip): product/pro-products/models.ts');
    return;
  }

  let content = fs.readFileSync(PRODUCT_MODELS_PATH, 'utf8');
  const original = content;
  const line = '  certificateTemplateDocumentId?: string;';

  for (const iface of ['ProProductDto', 'CreateProProductDto', 'UpdateProProductDto']) {
    content = appendOptionalPropertyToInterface(content, iface, line);
  }

  if (content !== original) {
    fs.writeFileSync(PRODUCT_MODELS_PATH, content, 'utf8');
    console.log('✅ Updated product/pro-products/models.ts');
  } else {
    console.log('✨ No changes needed (fields already present or interfaces missing).');
  }
}

main();
