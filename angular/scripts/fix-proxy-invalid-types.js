const fs = require('fs');
const path = require('path');

/**
 * Fix invalid TypeScript emitted by ABP generate-proxy:
 * - "interface X extends any" -> "type X = any" (interfaces cannot extend primitive any)
 * - "interface any" / "interface any extends any" -> "type AnyType = any" (interface name cannot be 'any')
 * - Generic "interface any extends any { key: TKey; value: TValue }" -> KeyValuePair<TKey, TValue>
 * - Missing SendResponse in hr/models.ts -> add a minimal interface
 *
 * Run after generate-proxy (invoked from generate-proxy-all.js).
 *
 * Usage: node scripts/fix-proxy-invalid-types.js
 */

const PROXY_DIR = path.join(__dirname, '../src/app/proxy');

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

function fixContent(filePath, content) {
  let out = content;

  // 1) "export interface any extends any { key: TKey; value: TValue; }" -> generic KeyValuePair
  out = out.replace(
    /export\s+interface\s+any\s+extends\s+any\s*\{\s*key:\s*TKey\s*;\s*value:\s*TValue\s*;\s*\}/g,
    'export interface KeyValuePair<TKey = any, TValue = any> {\n  key: TKey;\n  value: TValue;\n}'
  );

  // 2) "export interface any extends any { ... }" (multiline body) -> type AnyType = any
  out = out.replace(
    /export\s+interface\s+any\s+extends\s+any\s*\{[\s\S]*?\}/g,
    'export type AnyType = any'
  );
  out = out.replace(
    /export\s+interface\s+any\s+extends\s+any\s*;/g,
    'export type AnyType = any;'
  );

  // 3) "export interface any { ... }" (with or without body) -> type AnyType = any
  out = out.replace(
    /export\s+interface\s+any\s*\{[\s\S]*?\}/g,
    'export type AnyType = any'
  );

  // 4) "export interface X extends any" (e.g. FirebaseException) -> "export type X = any"
  out = out.replace(
    /export\s+interface\s+(\w+)\s+extends\s+any\s*\{[\s\S]*?\}/g,
    (match, name) => `export type ${name} = any`
  );
  out = out.replace(
    /export\s+interface\s+(\w+)\s+extends\s+any\s*;/g,
    (match, name) => `export type ${name} = any;`
  );

  // 5) Ensure hr/models.ts has SendResponse if it's referenced
  const relativePath = path.relative(PROXY_DIR, filePath).replace(/\\/g, '/');
  if (relativePath.includes('hr/') && /SendResponse/.test(out) && !/export\s+(interface|type)\s+SendResponse\b/.test(out)) {
    const sendResponseDef = 'export interface SendResponse { [key: string]: unknown }\n\n';
    if (out.trimStart() === out) {
      out = sendResponseDef + out;
    } else {
      out = out.replace(/^(\s*)/, (m) => m + sendResponseDef);
    }
  }

  return out;
}

function main() {
  console.log('🔧 Fixing invalid TypeScript types in proxy models (interface any, extends any, SendResponse)...\n');

  const modelsFiles = findModelsFiles(PROXY_DIR);
  let fixedCount = 0;

  for (const filePath of modelsFiles) {
    let content = fs.readFileSync(filePath, 'utf8');
    const newContent = fixContent(filePath, content);
    if (newContent !== content) {
      fs.writeFileSync(filePath, newContent, 'utf8');
      const relativePath = path.relative(process.cwd(), filePath);
      console.log(`✅ Fixed: ${relativePath}`);
      fixedCount++;
    }
  }

  if (fixedCount === 0) {
    console.log('✨ No invalid type declarations to fix.');
  } else {
    console.log(`\n✨ Fixed ${fixedCount} file(s).`);
  }
}

main();
