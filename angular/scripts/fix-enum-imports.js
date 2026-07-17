const fs = require('fs');
const path = require('path');

/**
 * Script để tự động thêm enum imports vào các file models.ts
 * Chạy sau khi generate proxy để fix các enum imports còn thiếu
 * 
 * Cơ chế tổng quát:
 * 1. Tự động scan tất cả enum files trong proxy directory
 * 2. Tạo map từ enum name -> enum file path
 * 3. Tự động detect enum usage trong models.ts files
 * 4. Tự động thêm imports với relative path chính xác
 * 
 * Usage: node scripts/fix-enum-imports.js
 */

const PROXY_DIR = path.join(__dirname, '../src/app/proxy');

// Cache: Map enum name -> { filePath, enumName }
const enumMap = new Map();

/**
 * Scan tất cả enum files và tạo map
 */
function buildEnumMap(dir = PROXY_DIR) {
  if (!fs.existsSync(dir)) {
    return;
  }

  const entries = fs.readdirSync(dir, { withFileTypes: true });
  
  for (const entry of entries) {
    const fullPath = path.join(dir, entry.name);
    
    if (entry.isDirectory()) {
      // Recursively scan subdirectories
      buildEnumMap(fullPath);
    } else if (entry.isFile() && entry.name.endsWith('.enum.ts')) {
      // Read enum file to extract enum name
      try {
        const content = fs.readFileSync(fullPath, 'utf8');
        // Match: export enum EnumName {
        const enumMatch = content.match(/export\s+enum\s+(\w+)\s*\{/);
        if (enumMatch) {
          const enumName = enumMatch[1];
          enumMap.set(enumName, {
            filePath: fullPath,
            enumName: enumName
          });
        }
      } catch (error) {
        // Skip if can't read file
        console.warn(`⚠️  Could not read enum file: ${fullPath}`);
      }
    }
  }
}

/**
 * Get import path for enum type (tổng quát, không hardcode)
 */
function getEnumImportPath(enumName, fromFilePath) {
  const enumInfo = enumMap.get(enumName);
  if (!enumInfo) {
    return null; // Enum not found
  }

  const enumFilePath = enumInfo.filePath;
  const fromDir = path.dirname(fromFilePath);
  
  // Calculate relative path
  const relativePath = path.relative(fromDir, enumFilePath);
  let normalizedPath = relativePath.replace(/\\/g, '/');
  
  // Remove .ts extension
  normalizedPath = normalizedPath.replace(/\.ts$/, '');
  
  // Ensure path starts with ./
  if (!normalizedPath.startsWith('.')) {
    normalizedPath = './' + normalizedPath;
  }
  
  return normalizedPath;
}

/**
 * Find all enum usages in a file (tổng quát)
 */
function findEnumUsages(content, filePath) {
  const enums = new Set();
  const fileDir = path.dirname(filePath);
  
  // Get all existing imports to check what's already imported
  const importLines = content.split('\n').filter(line => line.trim().startsWith('import'));
  const importedEnums = new Set();
  
  for (const importLine of importLines) {
    // Extract enum names from import statements
    // Match: import type { Enum1, Enum2 } from '...'
    const importMatch = importLine.match(/import\s+type\s+\{([^}]+)\}/);
    if (importMatch) {
      const importedNames = importMatch[1].split(',').map(name => name.trim());
      importedNames.forEach(name => importedEnums.add(name));
    }
  }
  
  // Pattern to find type annotations: : EnumName or : EnumName?
  // Also match in interface properties: property: EnumName;
  const typePattern = /:\s*([A-Z][a-zA-Z0-9]+)\s*[;,\?}\[\]]/g;
  
  // Known primitives and common types to skip
  const knownPrimitives = new Set([
    'string', 'number', 'boolean', 'any', 'void', 'undefined', 'null',
    'Date', 'Record', 'Array', 'Promise', 'Observable', 'Subject'
  ]);
  
  // Known DTO types that are not enums
  const knownDtoTypes = new Set([
    'FullAuditedEntityDto', 'PagedAndSortedResultRequestDto',
    'ApplicationMenuItemDto', 'ApplicationMenuDto'
  ]);
  
  let match;
  while ((match = typePattern.exec(content)) !== null) {
    const typeName = match[1];
    
    // Skip if already imported
    if (importedEnums.has(typeName)) {
      continue;
    }
    
    // Skip if it's a known primitive or common type
    if (knownPrimitives.has(typeName) || knownDtoTypes.has(typeName)) {
      continue;
    }
    
    // Skip if it's part of a generic (e.g., EntityDto<string>)
    const beforeMatch = content.substring(Math.max(0, match.index - 50), match.index);
    if (beforeMatch.includes('<')) {
      continue;
    }
    
    // Check if this type exists in our enum map
    if (enumMap.has(typeName)) {
      // Check if enum file exists
      const enumInfo = enumMap.get(typeName);
      if (fs.existsSync(enumInfo.filePath)) {
        enums.add(typeName);
      }
    }
  }
  
  return Array.from(enums);
}

/**
 * Fix enum imports in a file
 */
function fixEnumImports(filePath) {
  if (!fs.existsSync(filePath)) {
    return false;
  }

  let content = fs.readFileSync(filePath, 'utf8');
  const originalContent = content;

  // Find all enum usages that need imports
  const enumUsages = findEnumUsages(content, filePath);

  if (enumUsages.length === 0) {
    return false;
  }

  // Group enums by import path
  const importsToAdd = new Map();

  for (const enumName of enumUsages) {
    // Get import path
    const importPath = getEnumImportPath(enumName, filePath);
    if (importPath) {
      if (!importsToAdd.has(importPath)) {
        importsToAdd.set(importPath, new Set());
      }
      importsToAdd.get(importPath).add(enumName);
    }
  }

  if (importsToAdd.size === 0) {
    return false;
  }

  // Find insert position (after existing imports)
  const importRegex = /^import[^;]+;$/gm;
  const imports = content.match(importRegex) || [];
  let insertPosition = 0;

  if (imports.length > 0) {
    const lastImport = imports[imports.length - 1];
    const lastImportIndex = content.lastIndexOf(lastImport);
    insertPosition = lastImportIndex + lastImport.length;

    // Find the next newline after the last import
    const nextNewline = content.indexOf('\n', insertPosition);
    if (nextNewline > insertPosition) {
      insertPosition = nextNewline + 1;
    }
  }

  // Build import statements
  let newImports = '';
  for (const [importPath, enumNames] of importsToAdd.entries()) {
    const sortedEnums = Array.from(enumNames).sort().join(', ');
    newImports += `import type { ${sortedEnums} } from '${importPath}';\n`;
  }

  // Insert imports
  const beforeInsert = content.substring(0, insertPosition);
  const afterInsert = content.substring(insertPosition);

  // Add blank line if needed
  if (beforeInsert.trim().length > 0 && !beforeInsert.endsWith('\n\n')) {
    if (!beforeInsert.endsWith('\n')) {
      newImports = '\n' + newImports;
    } else {
      newImports = '\n' + newImports;
    }
  }

  content = beforeInsert + newImports + afterInsert;

  // Only write if content changed
  if (content !== originalContent) {
    fs.writeFileSync(filePath, content, 'utf8');
    return true;
  }

  return false;
}

/**
 * Main function
 */
function main() {
  console.log('🔍 Building enum map...');
  buildEnumMap();
  console.log(`📦 Found ${enumMap.size} enum(s) in proxy directory.\n`);

  console.log('🔍 Scanning for missing enum imports...\n');

  // Find all models.ts files
  const modelsFiles = [];

  function findModelsFiles(dir) {
    if (!fs.existsSync(dir)) {
      return;
    }
    
    const entries = fs.readdirSync(dir, { withFileTypes: true });
    for (const entry of entries) {
      const filePath = path.join(dir, entry.name);
      
      if (entry.isDirectory()) {
        findModelsFiles(filePath);
      } else if (entry.isFile() && entry.name === 'models.ts') {
        modelsFiles.push(filePath);
      }
    }
  }

  findModelsFiles(PROXY_DIR);

  let fixedCount = 0;

  for (const filePath of modelsFiles) {
    const relativePath = path.relative(PROXY_DIR, filePath);
    if (fixEnumImports(filePath)) {
      console.log(`✅ Fixed: ${relativePath}`);
      fixedCount++;
    }
  }

  if (fixedCount === 0) {
    console.log('✨ No enum imports to fix.');
  } else {
    console.log(`\n✨ Fixed ${fixedCount} file(s).`);
  }
}

main();
