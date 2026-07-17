/**
 * Script để fix URL template strings trong các file service proxy
 * Sửa các URL từ single quote string sang template string (backtick) khi có biến
 * 
 * Usage: node scripts/fix-proxy-service-urls.js
 */

const fs = require('fs');
const path = require('path');

const PROXY_DIR = path.join(__dirname, '../src/app/proxy');

/**
 * Fix URL template strings in a service file
 */
function fixServiceFile(filePath) {
  if (!fs.existsSync(filePath)) {
    return false;
  }

  let content = fs.readFileSync(filePath, 'utf8');
  const originalContent = content;

  // Pattern to match: url: '/api/path/${variable}',
  // Replace with: url: `/api/path/${variable}`,
  // Match any URL that contains ${...} inside single quotes
  // $ needs to be escaped in regex, { and } don't need escaping
  const urlPattern = /url:\s*'([^']*\$\{[^}]+\}[^']*)',/g;
  
  let hasChanges = false;
  content = content.replace(urlPattern, (match) => {
    hasChanges = true;
    // Extract the URL path (everything between quotes)
    const urlMatch = match.match(/url:\s*'([^']+)',/);
    if (urlMatch && urlMatch[1]) {
      const urlPath = urlMatch[1];
      // Replace single quote with backtick for template string
      return `url: \`${urlPath}\`,`;
    }
    return match;
  });

  // Only write if content changed
  if (content !== originalContent) {
    fs.writeFileSync(filePath, content, 'utf8');
    return true;
  }

  return false;
}

/**
 * Find all service files in proxy directory
 */
function findServiceFiles(dir = PROXY_DIR) {
  const serviceFiles = [];

  if (!fs.existsSync(dir)) {
    return serviceFiles;
  }

  const entries = fs.readdirSync(dir, { withFileTypes: true });

  for (const entry of entries) {
    const fullPath = path.join(dir, entry.name);

    if (entry.isDirectory()) {
      // Recursively scan subdirectories
      serviceFiles.push(...findServiceFiles(fullPath));
    } else if (entry.isFile() && entry.name.endsWith('.service.ts')) {
      serviceFiles.push(fullPath);
    }
  }

  return serviceFiles;
}

// Main execution
console.log('🔍 Scanning for service files...\n');

const serviceFiles = findServiceFiles();
let fixedCount = 0;

for (const filePath of serviceFiles) {
  const relativePath = path.relative(PROXY_DIR, filePath);
  if (fixServiceFile(filePath)) {
    console.log(`✅ Fixed: ${relativePath}`);
    fixedCount++;
  }
}

if (fixedCount === 0) {
  console.log('✨ No service files need fixing.');
} else {
  console.log(`\n✨ Fixed ${fixedCount} file(s).`);
}

