const { execSync } = require('child_process');
const path = require('path');

/**
 * Script để generate proxy cho một module cụ thể
 * 
 * Usage:
 *   npm run generate-proxy:module -- abp
 *   npm run generate-proxy:module -- audit-logging
 *   npm run generate-proxy:module -- <module-name>
 */

const ABP_HOST = process.env.ABP_HOST || 'https://localhost:44360';

// Get module name from command line arguments
const args = process.argv.slice(2);
const moduleName = args[0];

if (!moduleName) {
  console.error('❌ Vui lòng chỉ định tên module!');
  console.log('\nUsage:');
  console.log('  npm run generate-proxy:module -- <module-name>');
  console.log('\nExamples:');
  console.log('  npm run generate-proxy:module -- abp');
  console.log('  npm run generate-proxy:module -- audit-logging');
  console.log('\nHoặc set biến môi trường ABP_HOST:');
  console.log('  ABP_HOST=https://your-api.com npm run generate-proxy:module -- <module-name>');
  process.exit(1);
}

console.log(`🚀 Generating proxy for module: ${moduleName}`);
console.log(`   Host: ${ABP_HOST}\n`);

try {
  const command = `abp generate-proxy -t ng -u ${ABP_HOST} -m ${moduleName}`;
  console.log(`Running: ${command}\n`);
  
  execSync(command, { stdio: 'inherit' });
  
  console.log(`\n✅ Proxy generated successfully for module: ${moduleName}`);
  console.log(`\n💡 Tip: Run 'npm run fix-proxy-dtos' to fix any missing DTOs`);
  
} catch (error) {
  console.error(`\n❌ Error generating proxy for module: ${moduleName}`);
  console.error(error.message);
  process.exit(1);
}
