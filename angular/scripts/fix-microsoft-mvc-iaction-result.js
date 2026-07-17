const fs = require('fs');
const path = require('path');

/**
 * ABP generate-proxy may import either IActionResult or ActionResult from
 * microsoft/asp-net-core/mvc/models depending on the module/template. Proxy services stay
 * generated; this patches the shared models file so both names are always exported.
 *
 * Invoked from generate-proxy-all.js after proxy generation.
 *
 * Usage: node scripts/fix-microsoft-mvc-iaction-result.js
 */

const CANDIDATE_MS_MODELS = [
  path.join(__dirname, '../src/app/proxy/microsoft/asp-net-core/mvc/models.ts'),
  path.join(__dirname, '../src/app/microsoft/asp-net-core/mvc/models.ts'),
];

function resolveModelsPath() {
  return CANDIDATE_MS_MODELS.find(p => fs.existsSync(p)) || null;
}

function main() {
  console.log('🔧 Ensuring MVC ActionResult/IActionResult aliases in microsoft ASP.NET Core MVC models...\n');

  const modelsPath = resolveModelsPath();
  if (!modelsPath) {
    console.log(
      '⚠️  File not found (skip): src/app/proxy/microsoft/asp-net-core/mvc/models.ts'
    );
    return;
  }

  let content = fs.readFileSync(modelsPath, 'utf8');

  const hasIActionResult = /\bexport\s+(type|interface)\s+IActionResult\b/.test(content);
  const hasActionResult = /\bexport\s+(type|interface)\s+ActionResult\b/.test(content);
  if (hasIActionResult && hasActionResult) {
    console.log('✨ ActionResult and IActionResult already exported; nothing to do.');
    return;
  }

  let block = '\n\n/** Compatibility: generated proxy services may expect either MVC result name. */\n';
  if (!hasIActionResult && hasActionResult) {
    block += 'export type IActionResult = ActionResult;\n';
  } else if (hasIActionResult && !hasActionResult) {
    block += 'export type ActionResult<T = unknown> = IActionResult;\n';
  } else {
    block += 'export interface IActionResult {}\n';
    block += 'export type ActionResult<T = unknown> = IActionResult;\n';
  }

  fs.writeFileSync(modelsPath, content.replace(/\s*$/, '') + block, 'utf8');
  console.log(`✅ Patched MVC result aliases in ${path.relative(process.cwd(), modelsPath)}`);
}

main();
