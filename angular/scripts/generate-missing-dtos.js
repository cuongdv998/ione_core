const fs = require('fs');
const path = require('path');

/**
 * Script để tự động generate các DTO còn thiếu từ generate-proxy.json
 * Chạy sau khi chạy lệnh abp generate-proxy
 * 
 * Usage: node scripts/generate-missing-dtos.js
 */

const PROXY_JSON_PATH = path.join(__dirname, '../src/app/proxy/generate-proxy.json');
const PROXY_DIR = path.join(__dirname, '../src/app/proxy');

// Extract DTO name from full type name
function extractDtoName(fullTypeName) {
  const parts = fullTypeName.split('.');
  return parts[parts.length - 1];
}

// Get module path and subfolder from type name using generate-proxy.json modules data
function getModulePathFromType(typeName, modules) {
  // First, check namespace patterns (more specific patterns first)
  // These patterns must be checked BEFORE checking controllers to avoid wrong module matching
  
  if (typeName.includes('Volo.Abp.AuditLogging')) {
    // Check if audit-logging module exists in modules with custom rootPath
    if (modules['audit-logging'] && modules['audit-logging'].rootPath) {
      const rootPath = modules['audit-logging'].rootPath;
      // If rootPath is "audit-logging", check if volo/abp/audit-logging exists
      if (rootPath === 'audit-logging') {
        const voloPath = path.join(PROXY_DIR, 'volo/abp/audit-logging');
        if (fs.existsSync(voloPath)) {
          return voloPath;
        }
      }
      return path.join(PROXY_DIR, rootPath);
    }
    return path.join(PROXY_DIR, 'volo/abp/audit-logging');
  }
  
  if (typeName.includes('Volo.Abp.Identity')) {
    // Check if identity module exists in modules
    if (modules.identity && modules.identity.rootPath) {
      return path.join(PROXY_DIR, modules.identity.rootPath);
    }
    return path.join(PROXY_DIR, 'volo/abp/identity');
  }
  
  if (typeName.includes('Volo.Abp.PermissionManagement')) {
    // Check if permissionManagement module exists in modules
    if (modules.permissionManagement && modules.permissionManagement.rootPath) {
      return path.join(PROXY_DIR, modules.permissionManagement.rootPath);
    }
    return path.join(PROXY_DIR, 'volo/abp/permission-management');
  }
  
  if (typeName.includes('Volo.Abp.Ui.Navigation') || typeName.includes('Volo.Abp.UI.Navigation')) {
    return path.join(PROXY_DIR, 'volo/abp/ui/navigation');
  }
  
  if (typeName.includes('Volo.Abp.Auditing')) {
    return path.join(PROXY_DIR, 'volo/abp/auditing');
  }
  
  // Then, try to find module by checking controllers in modules
  for (const [moduleName, moduleData] of Object.entries(modules)) {
    if (!moduleData || !moduleData.controllers) continue;
    
    // Check if any controller in this module uses this type
    for (const [controllerName, controller] of Object.entries(moduleData.controllers)) {
      // Check controller type
      if (controller.type && typeName.includes(controller.type.split('.')[0])) {
        const rootPath = moduleData.rootPath || moduleName;
        const basePath = path.join(PROXY_DIR, rootPath);
        // Try to find subfolder from namespace (e.g., iOne.Hr.DepartmentTypes -> department-types)
        const subfolder = getSubfolderFromNamespace(typeName, rootPath);
        return subfolder ? path.join(basePath, subfolder) : basePath;
      }
      
      // Check interfaces
      if (controller.interfaces) {
        for (const iface of controller.interfaces) {
          if (iface.type && typeName.includes(iface.type.split('.')[0])) {
            const rootPath = moduleData.rootPath || moduleName;
            const basePath = path.join(PROXY_DIR, rootPath);
            const subfolder = getSubfolderFromNamespace(typeName, rootPath);
            return subfolder ? path.join(basePath, subfolder) : basePath;
          }
        }
      }
    }
  }
  
  // Fallback: Check other namespace patterns (only if not matched above)
  if (typeName.includes('Volo.Abp.AspNetCore.Mvc.MultiTenancy')) {
    return path.join(PROXY_DIR, 'volo/abp/asp-net-core/mvc/multi-tenancy');
  }
  if (typeName.includes('Volo.Abp.AspNetCore.Mvc.ApplicationConfigurations')) {
    return path.join(PROXY_DIR, 'volo/abp/asp-net-core/mvc/application-configurations');
  }
  if (typeName.includes('Volo.Abp.Http.Modeling')) {
    return path.join(PROXY_DIR, 'volo/abp/http/modeling');
  }
  
  // Check for custom modules (e.g., iOne.Hr.* -> hr module)
  if (typeName.includes('iOne.Hr')) {
    const rootPath = modules.hr?.rootPath || 'hr';
    const basePath = path.join(PROXY_DIR, rootPath);
    const subfolder = getSubfolderFromNamespace(typeName, rootPath);
    return subfolder ? path.join(basePath, subfolder) : basePath;
  }
  
  // Check for Master module (e.g., iOne.Master.* -> master module)
  if (typeName.includes('iOne.Master')) {
    const rootPath = modules.master?.rootPath || 'master';
    const basePath = path.join(PROXY_DIR, rootPath);
    const subfolder = getSubfolderFromNamespace(typeName, rootPath);
    return subfolder ? path.join(basePath, subfolder) : basePath;
  }
  
  // Try to infer from namespace pattern: Extract first part of namespace
  const namespaceParts = typeName.split('.');
  if (namespaceParts.length > 0) {
    const firstPart = namespaceParts[0];
    // Check if there's a module matching this namespace
    for (const [moduleName, moduleData] of Object.entries(modules)) {
      if (moduleData && moduleData.rootPath) {
        // Try to match namespace with module
        if (typeName.toLowerCase().includes(moduleName.toLowerCase()) || 
            moduleName.toLowerCase().includes(firstPart.toLowerCase())) {
          const basePath = path.join(PROXY_DIR, moduleData.rootPath);
          const subfolder = getSubfolderFromNamespace(typeName, moduleData.rootPath);
          return subfolder ? path.join(basePath, subfolder) : basePath;
        }
      }
    }
  }
  
  return null;
}

// Get module namespace from module name and data
function getModuleNamespace(moduleName, moduleData) {
  // Try to infer namespace from module name
  // audit-logging -> Volo.Abp.AuditLogging
  // identity -> Volo.Abp.Identity
  // permissionManagement -> Volo.Abp.PermissionManagement
  // hr -> iOne.Hr
  
  if (moduleName === 'audit-logging') return 'Volo.Abp.AuditLogging';
  if (moduleName === 'identity') return 'Volo.Abp.Identity';
  if (moduleName === 'permissionManagement') return 'Volo.Abp.PermissionManagement';
  if (moduleName === 'hr') return 'iOne.Hr';
  if (moduleName === 'abp') return 'Volo.Abp';
  
  // Try to get from first controller if available
  if (moduleData.controllers) {
    for (const [controllerName, controller] of Object.entries(moduleData.controllers)) {
      if (controller.type) {
        const parts = controller.type.split('.');
        if (parts.length >= 3) {
          return parts.slice(0, 3).join('.'); // e.g., Volo.Abp.AuditLogging
        }
      }
    }
  }
  
  return null;
}

// Get subfolder from namespace (e.g., iOne.Hr.DepartmentTypes -> department-types)
function getSubfolderFromNamespace(typeName, rootPath) {
  const parts = typeName.split('.');
  if (parts.length < 3) return null;
  
  // For iOne.Hr.DepartmentTypes, extract "DepartmentTypes" and convert to kebab-case
  const lastPart = parts[parts.length - 2]; // Get second to last (e.g., "DepartmentTypes")
  if (!lastPart) return null;
  
  // Convert PascalCase to kebab-case
  const kebabCase = lastPart.replace(/([A-Z])/g, '-$1').toLowerCase().replace(/^-/, '');
  
  // Check if this subfolder exists in the module
  const subfolderPath = path.join(PROXY_DIR, rootPath, kebabCase);
  if (fs.existsSync(subfolderPath)) {
    return kebabCase;
  }
  
  return null;
}

// Get import path for enum type
function getEnumImportPath(enumType, modulePath) {
  const enumName = extractDtoName(enumType);
  
  // Known enum mappings
  if (enumType.includes('Auditing.EntityChangeType') || enumName === 'EntityChangeType') {
    // Calculate relative path from modulePath to volo/abp/auditing
    const auditingPath = path.join(PROXY_DIR, 'volo/abp/auditing');
    const relativePath = path.relative(modulePath, auditingPath);
    // Normalize path separators and ensure it starts with ./
    let normalizedPath = relativePath.replace(/\\/g, '/');
    if (!normalizedPath.startsWith('.')) {
      normalizedPath = './' + normalizedPath;
    }
    return path.join(normalizedPath, 'entity-change-type.enum').replace(/\\/g, '/');
  }
  
  // Handle other known enums
  if (enumName === 'LoginResultType' || enumName === 'HttpStatusCode') {
    // These might be in @abp/ng.core or need to be created
    // For now, return null - they should be handled separately
    return null;
  }
  
  // For other enums, try to find in common locations
  // Check if enum file exists in volo/abp/auditing
  const enumFileName = enumName.toLowerCase().replace(/([A-Z])/g, '-$1').replace(/^-/, '');
  const auditingEnumPath = path.join(PROXY_DIR, 'volo/abp/auditing', `${enumFileName}.enum.ts`);
  if (fs.existsSync(auditingEnumPath)) {
    const relativePath = path.relative(modulePath, path.join(PROXY_DIR, 'volo/abp/auditing'));
    let normalizedPath = relativePath.replace(/\\/g, '/');
    if (!normalizedPath.startsWith('.')) {
      normalizedPath = './' + normalizedPath;
    }
    return path.join(normalizedPath, `${enumFileName}.enum`).replace(/\\/g, '/');
  }
  
  // Check in same module directory
  const sameDirEnumPath = path.join(modulePath, `${enumFileName}.enum.ts`);
  if (fs.existsSync(sameDirEnumPath)) {
    return `./${enumFileName}.enum`;
  }
  
  // Check in parent directories (for nested modules)
  const parentDir = path.dirname(modulePath);
  const parentEnumPath = path.join(parentDir, `${enumFileName}.enum.ts`);
  if (fs.existsSync(parentEnumPath)) {
    const relativePath = path.relative(modulePath, parentDir);
    let normalizedPath = relativePath.replace(/\\/g, '/');
    if (!normalizedPath.startsWith('.')) {
      normalizedPath = './' + normalizedPath;
    }
    return path.join(normalizedPath, `${enumFileName}.enum`).replace(/\\/g, '/');
  }
  
  // Check in sibling directories (e.g., hr-department-types from app folder)
  // This handles cases where enum is in a different folder at the same level
  const siblingDirs = fs.readdirSync(parentDir, { withFileTypes: true })
    .filter(dirent => dirent.isDirectory())
    .map(dirent => dirent.name);
  
  for (const siblingDir of siblingDirs) {
    const siblingEnumPath = path.join(parentDir, siblingDir, `${enumFileName}.enum.ts`);
    if (fs.existsSync(siblingEnumPath)) {
      const relativePath = path.relative(modulePath, path.join(parentDir, siblingDir));
      let normalizedPath = relativePath.replace(/\\/g, '/');
      if (!normalizedPath.startsWith('.')) {
        normalizedPath = './' + normalizedPath;
      }
      return path.join(normalizedPath, `${enumFileName}.enum`).replace(/\\/g, '/');
    }
  }
  
  // Check for HrDepartmentTypeStatus specifically in hr-department-types folder
  if (enumName === 'HrDepartmentTypeStatus' || enumType.includes('HrDepartmentTypeStatus')) {
    const hrEnumPath = path.join(PROXY_DIR, 'hr-department-types', 'hr-department-type-status.enum.ts');
    if (fs.existsSync(hrEnumPath)) {
      const relativePath = path.relative(modulePath, path.join(PROXY_DIR, 'hr-department-types'));
      let normalizedPath = relativePath.replace(/\\/g, '/');
      if (!normalizedPath.startsWith('.')) {
        normalizedPath = './' + normalizedPath;
      }
      return path.join(normalizedPath, 'hr-department-type-status.enum').replace(/\\/g, '/');
    }
  }
  
  // Default: return null - enum might need to be created or imported from elsewhere
  return null;
}

// Map C# type to TypeScript type
function mapTypeToTS(prop, allTypes) {
  const { type, typeSimple, isRequired } = prop;
  
  // Handle nullable types - optional goes on property name, not type
  const isOptional = isRequired === false;
  
  // System types
  if (typeSimple === 'string' || typeSimple === 'string?') {
    return 'string';
  }
  if (typeSimple === 'number' || typeSimple === 'number?') {
    return 'number';
  }
  if (typeSimple === 'boolean' || typeSimple === 'boolean?') {
    return 'boolean';
  }
  
  // Enum types - handle nullable enums correctly
  if (typeSimple === 'enum' || typeSimple === 'enum?') {
    const enumName = extractDtoName(type);
    
    // Handle System.Net.HttpStatusCode as number
    if (type.includes('System.Net.HttpStatusCode') || enumName === 'HttpStatusCode') {
      return 'number';
    }
    
    // Handle LoginResultType - could be number or enum, check if enum file exists
    if (enumName === 'LoginResultType') {
      // Check if enum file exists, otherwise use number
      const enumPath = path.join(PROXY_DIR, 'volo/abp/account/web/areas/account/controllers/models/login-result-type.enum.ts');
      if (fs.existsSync(enumPath)) {
        return enumName;
      }
      return 'number'; // Fallback to number if enum file doesn't exist
    }
    
    // Don't add ? to enum type, nullable is handled by property optional marker
    return enumName;
  }
  
  // Array types
  if (typeSimple && typeSimple.startsWith('[') && typeSimple.endsWith(']')) {
    const innerType = typeSimple.slice(1, -1);
    const innerTypeNonNullable = innerType.endsWith('?') ? innerType.slice(0, -1) : innerType;

    // Arrays of enums are represented as typeSimple: "[enum]" in generate-proxy.json.
    // In that case, use the real enum type from `type` (e.g. "[iOne.PolicyContracts.PolicyContractStatus]").
    if (innerTypeNonNullable === 'enum') {
      const fullInnerType = typeof type === 'string' && type.startsWith('[') && type.endsWith(']')
        ? type.slice(1, -1)
        : type;

      const enumName = extractDtoName(fullInnerType);

      // Keep the same special-cases as single enums
      if (fullInnerType.includes('System.Net.HttpStatusCode') || enumName === 'HttpStatusCode') {
        return 'number[]';
      }

      if (enumName === 'LoginResultType') {
        const enumPath = path.join(
          PROXY_DIR,
          'volo/abp/account/web/areas/account/controllers/models/login-result-type.enum.ts'
        );
        return fs.existsSync(enumPath) ? `${enumName}[]` : 'number[]';
      }

      return `${enumName}[]`;
    }

    if (innerType.startsWith('System.')) {
      const tsType = innerType.includes('String') ? 'string' : 
                     innerType.includes('Int') || innerType.includes('Double') || innerType.includes('Decimal') ? 'number' :
                     innerType.includes('Boolean') ? 'boolean' : 'any';
      return `${tsType}[]`;
    } else {
      const dtoName = extractDtoName(innerType);
      return `${dtoName}[]`;
    }
  }
  
  // Dictionary types
  if (typeSimple && typeSimple.startsWith('{') && typeSimple.includes(':')) {
    return 'Record<string, any>';
  }
  
  // Complex DTO types
  if (!type.startsWith('System.')) {
    const dtoName = extractDtoName(type);
    return dtoName;
  }
  
  return 'any';
}

// Convert PascalCase to camelCase
function toCamelCase(str) {
  if (!str) return str;
  // If already starts with lowercase, return as is
  if (str[0] === str[0].toLowerCase()) return str;
  // Convert first letter to lowercase
  return str[0].toLowerCase() + str.slice(1);
}

// Generate TypeScript interface from DTO definition
function generateInterface(dtoName, dtoDef, allTypes, modulePath) {
  // Check if extends from base type
  const extendsFrom = [];
  const importsNeeded = new Set();
  
  if (dtoDef.baseType) {
    // Handle ExtensibleObject (no generic, no import needed - it's from @abp/ng.core but not commonly used)
    if (dtoDef.baseType.includes('ExtensibleObject')) {
      // ExtensibleObject is typically not extended in TypeScript interfaces
      // It's handled via extraProperties in the interface itself
      // So we don't add it to extends, but we could add ExtensibleObject import if needed
      // For now, skip it as it's usually not needed in TypeScript
    }
    // Handle EntityDto types (check Extensible variants first, then regular ones)
    else if (dtoDef.baseType.includes('EntityDto')) {
      // Extract the generic type if any
      const entityDtoMatch = dtoDef.baseType.match(/EntityDto<([^>]+)>/);
      const idType = entityDtoMatch ? entityDtoMatch[1].includes('Guid') ? 'string' : 'string' : 'string';
      
      // Check Extensible variants first (they extend from regular ones)
      if (dtoDef.baseType.includes('ExtensibleFullAuditedEntityDto')) {
        extendsFrom.push(`FullAuditedEntityDto<${idType}>`);
        importsNeeded.add('FullAuditedEntityDto');
      } else if (dtoDef.baseType.includes('ExtensibleAuditedEntityDto')) {
        extendsFrom.push(`AuditedEntityDto<${idType}>`);
        importsNeeded.add('AuditedEntityDto');
      } else if (dtoDef.baseType.includes('ExtensibleCreationAuditedEntityDto')) {
        extendsFrom.push(`CreationAuditedEntityDto<${idType}>`);
        importsNeeded.add('CreationAuditedEntityDto');
      } else if (dtoDef.baseType.includes('ExtensibleEntityDto')) {
        extendsFrom.push(`EntityDto<${idType}>`);
        importsNeeded.add('EntityDto');
      }
      // Then check regular variants
      else if (dtoDef.baseType.includes('FullAuditedEntityDto')) {
        extendsFrom.push(`FullAuditedEntityDto<${idType}>`);
        importsNeeded.add('FullAuditedEntityDto');
      } else if (dtoDef.baseType.includes('AuditedEntityDto')) {
        extendsFrom.push(`AuditedEntityDto<${idType}>`);
        importsNeeded.add('AuditedEntityDto');
      } else if (dtoDef.baseType.includes('CreationAuditedEntityDto')) {
        extendsFrom.push(`CreationAuditedEntityDto<${idType}>`);
        importsNeeded.add('CreationAuditedEntityDto');
      } else {
        extendsFrom.push(`EntityDto<${idType}>`);
        importsNeeded.add('EntityDto');
      }
    }
    // Handle Request DTO types
    else if (dtoDef.baseType.includes('ExtensiblePagedAndSortedResultRequestDto') || 
             dtoDef.baseType.includes('PagedAndSortedResultRequestDto')) {
      extendsFrom.push('PagedAndSortedResultRequestDto');
      importsNeeded.add('PagedAndSortedResultRequestDto');
    } else if (dtoDef.baseType.includes('ExtensiblePagedResultRequestDto') ||
               dtoDef.baseType.includes('PagedResultRequestDto')) {
      extendsFrom.push('PagedResultRequestDto');
      importsNeeded.add('PagedResultRequestDto');
    } else if (dtoDef.baseType.includes('ExtensibleSortedResultRequestDto') ||
               dtoDef.baseType.includes('SortedResultRequestDto')) {
      extendsFrom.push('SortedResultRequestDto');
      importsNeeded.add('SortedResultRequestDto');
    } else if (dtoDef.baseType.includes('ExtensibleLimitedResultRequestDto') ||
               dtoDef.baseType.includes('LimitedResultRequestDto')) {
      extendsFrom.push('LimitedResultRequestDto');
      importsNeeded.add('LimitedResultRequestDto');
    }
  }
  
  const extendsClause = extendsFrom.length > 0 ? ` extends ${extendsFrom.join(', ')}` : '';
  let code = `export interface ${dtoName}${extendsClause} {\n`;
  
  // Generate properties
  if (dtoDef.properties && dtoDef.properties.length > 0) {
    for (const prop of dtoDef.properties) {
      // Use jsonName if available (camelCase), otherwise convert name to camelCase
      const propName = prop.jsonName || toCamelCase(prop.name);
      let propType = mapTypeToTS(prop, allTypes);
      
      // Fix nullable syntax - remove ? from end of type (nullable is handled by property optional marker)
      if (propType.endsWith('?')) {
        propType = propType.slice(0, -1);
      }
      
      const optional = prop.isRequired === false ? '?' : '';
      code += `  ${propName}${optional}: ${propType};\n`;
    }
  }
  
  // Add extraProperties for ExtensibleObject-based DTOs
  // ExtensibleObject and ExtensibleEntityDto variants support extraProperties
  // But we only add it if it's not already in properties and baseType indicates extensibility
  if (dtoDef.baseType && (
      dtoDef.baseType.includes('ExtensibleObject') || 
      dtoDef.baseType.includes('ExtensibleEntityDto') ||
      dtoDef.baseType.includes('ExtensibleFullAuditedEntityDto') ||
      dtoDef.baseType.includes('ExtensibleAuditedEntityDto') ||
      dtoDef.baseType.includes('ExtensibleCreationAuditedEntityDto')
    )) {
    // Check if extraProperties already exists in properties
    const hasExtraProperties = dtoDef.properties && dtoDef.properties.some(p => {
      const propName = p.jsonName || toCamelCase(p.name);
      return propName === 'extraProperties' || propName === 'ExtraProperties';
    });
    if (!hasExtraProperties) {
      code += `  extraProperties?: Record<string, any>;\n`;
    }
  }
  
  code += '}\n';
  
  // Return both code and imports needed
  return { code, imports: Array.from(importsNeeded) };
}

// Check if DTO already exists in models file or is imported
function dtoExistsInFile(filePath, dtoName) {
  if (!fs.existsSync(filePath)) return false;
  const content = fs.readFileSync(filePath, 'utf8');
  
  // Check if exported in this file
  if (content.includes(`export interface ${dtoName}`) || 
      content.includes(`export type ${dtoName}`)) {
    return true;
  }
  
  // Check if imported from another file (to avoid conflicts)
  const importRegex = new RegExp(`import[^}]*{([^}]*)}[^;]*from[^;]+;`, 'g');
  let match;
  while ((match = importRegex.exec(content)) !== null) {
    const imports = match[1].split(',').map(imp => imp.trim());
    if (imports.some(imp => imp === dtoName || imp.includes(dtoName))) {
      return true; // Already imported, don't generate
    }
  }
  
  return false;
}

// Get all DTOs used in return values and parameters
function getAllNeededDtos(modules, types) {
  const neededDtos = new Set();
  
  // Helper to extract DTO from type string
  function extractDtosFromType(typeStr) {
    if (!typeStr || typeStr.startsWith('System.') || typeStr === 'System.Void') {
      return [];
    }
    
    const dtos = new Set();
    
    // Handle generic types like PagedResultDto<AuditLogDto> or ListResultDto<IdentityRoleDto>
    const genericMatch = typeStr.match(/<([^>]+)>/);
    if (genericMatch) {
      const innerType = genericMatch[1].trim();
      if (!innerType.startsWith('System.')) {
        dtos.add(innerType);
        // Recursively extract from nested generics
        const nested = extractDtosFromType(innerType);
        nested.forEach(dto => dtos.add(dto));
      }
    }
    
    // Add the main type if it's not a System type and not already added
    // But skip generic wrapper types like PagedResultDto, ListResultDto
    if (!typeStr.startsWith('System.') && !dtos.has(typeStr)) {
      // Only add if it's not a generic wrapper (contains <)
      if (!typeStr.includes('<')) {
        dtos.add(typeStr);
      }
    }
    
    return Array.from(dtos);
  }
  
  // Process modules
  for (const [moduleName, moduleData] of Object.entries(modules)) {
    if (!moduleData.controllers) continue;
    
    for (const [controllerName, controller] of Object.entries(moduleData.controllers)) {
      // Check interfaces
      if (controller.interfaces) {
        for (const iface of controller.interfaces) {
          if (iface.methods) {
            for (const method of iface.methods) {
              // Return types
              if (method.returnValue && method.returnValue.type) {
                extractDtosFromType(method.returnValue.type).forEach(dto => neededDtos.add(dto));
              }
              // Parameter types
              if (method.parametersOnMethod) {
                method.parametersOnMethod.forEach(param => {
                  extractDtosFromType(param.type).forEach(dto => neededDtos.add(dto));
                });
              }
            }
          }
        }
      }
      
      // Check actions
      if (controller.actions) {
        for (const [actionName, action] of Object.entries(controller.actions)) {
          if (action.returnValue && action.returnValue.type) {
            extractDtosFromType(action.returnValue.type).forEach(dto => neededDtos.add(dto));
          }
          if (action.parametersOnMethod) {
            action.parametersOnMethod.forEach(param => {
              extractDtosFromType(param.type).forEach(dto => neededDtos.add(dto));
            });
          }
        }
      }
    }
  }
  
  // Process types to find referenced DTOs
  const processed = new Set();
  function processType(typeName) {
    if (processed.has(typeName) || typeName.startsWith('System.')) return;
    processed.add(typeName);
    
    const typeDef = types[typeName];
    if (!typeDef || !typeDef.properties) return;
    
    for (const prop of typeDef.properties) {
      if (prop.type && !prop.type.startsWith('System.')) {
        // Handle array types
        if (prop.typeSimple && prop.typeSimple.startsWith('[') && prop.typeSimple.endsWith(']')) {
          const innerType = prop.typeSimple.slice(1, -1);
          if (!innerType.startsWith('System.')) {
            neededDtos.add(innerType);
            processType(innerType);
          }
        } else {
          neededDtos.add(prop.type);
          processType(prop.type);
        }
      }
    }
  }
  
  // Process all needed DTOs recursively
  Array.from(neededDtos).forEach(dto => processType(dto));
  
  // Filter out System types, Dictionary types, and primitive types
  const filteredDtos = Array.from(neededDtos).filter(dto => {
    // Skip System types
    if (dto.startsWith('System.')) return false;
    // Skip Dictionary types (format: {System.String:System.String})
    if (dto.startsWith('{') && dto.includes(':')) return false;
    // Skip primitive types
    if (['string', 'number', 'boolean', 'any'].includes(dto)) return false;
    // Skip generic types without proper format
    if (dto.includes('<') && !dto.match(/^[^<]+<[^>]+>$/)) return false;
    return true;
  });
  
  return filteredDtos;
}

// Main function
function main() {
  console.log('🔍 Analyzing generate-proxy.json...\n');
  
  if (!fs.existsSync(PROXY_JSON_PATH)) {
    console.error('❌ generate-proxy.json not found!');
    process.exit(1);
  }
  
  const proxyData = JSON.parse(fs.readFileSync(PROXY_JSON_PATH, 'utf8'));
  const { modules, types } = proxyData;
  
  if (!modules || !types) {
    console.error('❌ Invalid generate-proxy.json structure!');
    process.exit(1);
  }
  
  console.log('📋 Finding all needed DTOs...');
  const allNeededDtos = getAllNeededDtos(modules, types);
  console.log(`   Found ${allNeededDtos.length} DTOs\n`);
  
  // Group DTOs by module
  const dtosByModule = {};
  const unmatchedDtos = [];
  
  for (const dtoType of allNeededDtos) {
    const modulePath = getModulePathFromType(dtoType, proxyData.modules);
    if (modulePath) {
      if (!dtosByModule[modulePath]) dtosByModule[modulePath] = [];
      dtosByModule[modulePath].push(dtoType);
    } else {
      unmatchedDtos.push(dtoType);
    }
  }
  
  if (unmatchedDtos.length > 0) {
    // Filter out System types, Dictionary types, and primitive types (these are not real DTOs)
    const realUnmatched = unmatchedDtos.filter(dto => {
      if (dto.startsWith('System.')) return false;
      if (dto.startsWith('{') && dto.includes(':')) return false;
      if (['string', 'number', 'boolean', 'any'].includes(dto)) return false;
      return true;
    });
    
    if (realUnmatched.length > 0) {
      console.log(`⚠️  Could not determine module path for ${realUnmatched.length} DTO(s):`);
      realUnmatched.slice(0, 10).forEach(dto => console.log(`   - ${dto}`));
      if (realUnmatched.length > 10) {
        console.log(`   ... and ${realUnmatched.length - 10} more`);
      }
      console.log('');
    }
  }
  
  let totalAdded = 0;
  
  // Process each module
  for (const [modulePath, dtoTypes] of Object.entries(dtosByModule)) {
    const modelsPath = path.join(modulePath, 'models.ts');
    
    // Create models.ts file if it doesn't exist
    if (!fs.existsSync(modelsPath)) {
      // Ensure directory exists
      if (!fs.existsSync(modulePath)) {
        fs.mkdirSync(modulePath, { recursive: true });
      }
      // Create empty models.ts file
      fs.writeFileSync(modelsPath, '', 'utf8');
      console.log(`📄 Created models file: ${path.relative(PROXY_DIR, modelsPath)}`);
    }
    
    console.log(`📝 Processing: ${path.relative(PROXY_DIR, modulePath)}`);
    
    let modelsContent = fs.readFileSync(modelsPath, 'utf8');
    const newInterfaces = [];
    const importsToAdd = new Set();
    
    // First, scan file content for enum usages and add missing imports
    // Find all enum types used in the file (e.g., HrDepartmentTypeStatus)
    const enumUsageRegex = /\b([A-Z][a-zA-Z]*Status|EntityChangeType|LoginResultType)\b/g;
    const usedEnums = new Set();
    let enumMatch;
    while ((enumMatch = enumUsageRegex.exec(modelsContent)) !== null) {
      const enumName = enumMatch[1];
      // Check if this is actually an enum (not just a word that matches the pattern)
      // Look for enum definition in types
      const enumTypeDef = Object.entries(types).find(([typeName, typeDef]) => {
        const extractedName = extractDtoName(typeName);
        return extractedName === enumName && typeDef.isEnum;
      });
      
      if (enumTypeDef) {
        const [fullTypeName, typeDef] = enumTypeDef;
        usedEnums.add({ name: enumName, fullType: fullTypeName });
      }
    }
    
    // For each used enum, check if import exists and add if missing
    for (const enumInfo of usedEnums) {
      const enumName = enumInfo.name;
      const enumType = enumInfo.fullType;
      
      // Calculate relative path to enum file
      let enumImportPath = getEnumImportPath(enumType, modulePath);
      if (enumImportPath) {
        // Check if this enum is already imported from this path
        const enumImportRegex = new RegExp(`import\\s+type\\s+{[^}]*\\b${enumName}\\b[^}]*}\\s+from\\s+['"]${enumImportPath.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')}['"];?`, 'g');
        const alreadyImported = enumImportRegex.test(modelsContent);
        
        if (!alreadyImported) {
          // Check if there's already an import from this path that we can merge with
          const existingImportFromPath = Array.from(importsToAdd).find(imp => imp.includes(enumImportPath));
          if (existingImportFromPath) {
            // Extract existing imports and merge
            const match = existingImportFromPath.match(/import\s+type\s+{\s*([^}]+)\s*}\s+from/);
            if (match) {
              const existingImports = match[1].split(',').map(i => i.trim());
              if (!existingImports.includes(enumName)) {
                const allImports = [...existingImports, enumName].join(', ');
                importsToAdd.delete(existingImportFromPath);
                importsToAdd.add(`import type { ${allImports} } from '${enumImportPath}';`);
              }
            }
          } else {
            // Add new import
            const importStatement = `import type { ${enumName} } from '${enumImportPath}';`;
            importsToAdd.add(importStatement);
          }
        }
      }
    }
    
    for (const dtoType of dtoTypes) {
      const dtoName = extractDtoName(dtoType);
      const typeDef = types[dtoType];
      
      if (!typeDef) {
        continue; // Skip if type definition not found
      }
      
      // Skip enums
      if (typeDef.isEnum) {
        continue;
      }
      
      // Check if already exists
      if (dtoExistsInFile(modelsPath, dtoName)) {
        // Even if interface exists, check for missing enum imports
        if (typeDef.properties) {
          for (const prop of typeDef.properties) {
            if (prop.typeSimple === 'enum' || prop.typeSimple === 'enum?') {
              const enumType = prop.type;
              const enumName = extractDtoName(enumType);
              
              // Calculate relative path to enum file
              let enumImportPath = getEnumImportPath(enumType, modulePath);
              if (enumImportPath) {
                // Check if this enum is already imported from this path
                const enumImportRegex = new RegExp(`import\\s+type\\s+{[^}]*\\b${enumName}\\b[^}]*}\\s+from\\s+['"]${enumImportPath.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')}['"];?`, 'g');
                const alreadyImported = enumImportRegex.test(modelsContent);
                
                if (!alreadyImported) {
                  // Check if there's already an import from this path that we can merge with
                  const existingImportFromPath = Array.from(importsToAdd).find(imp => imp.includes(enumImportPath));
                  if (existingImportFromPath) {
                    // Extract existing imports and merge
                    const match = existingImportFromPath.match(/import\s+type\s+{\s*([^}]+)\s*}\s+from/);
                    if (match) {
                      const existingImports = match[1].split(',').map(i => i.trim());
                      if (!existingImports.includes(enumName)) {
                        const allImports = [...existingImports, enumName].join(', ');
                        importsToAdd.delete(existingImportFromPath);
                        importsToAdd.add(`import type { ${allImports} } from '${enumImportPath}';`);
                      }
                    }
                  } else {
                    // Add new import
                    const importStatement = `import type { ${enumName} } from '${enumImportPath}';`;
                    importsToAdd.add(importStatement);
                  }
                }
              }
            }
          }
        }
        continue;
      }
      
      console.log(`   ➕ Generating ${dtoName}...`);
      
      // Generate interface
      const interfaceResult = generateInterface(dtoName, typeDef, types, modulePath);
      const interfaceCode = typeof interfaceResult === 'string' ? interfaceResult : interfaceResult.code;
      const baseImports = typeof interfaceResult === 'object' && interfaceResult.imports ? interfaceResult.imports : [];
      
      newInterfaces.push({ name: dtoName, code: interfaceCode, type: dtoType });
      
      // Add base type imports (EntityDto, PagedAndSortedResultRequestDto, etc.)
      if (baseImports.length > 0) {
        // Check if @abp/ng.core import already exists in file content
        const existingAbpImportMatch = modelsContent.match(/import type { ([^}]+) } from '@abp\/ng.core';/);
        let existingImports = [];
        
        if (existingAbpImportMatch) {
          existingImports = existingAbpImportMatch[1].split(',').map(i => i.trim());
        }
        
        // Check if @abp/ng.core import already exists in importsToAdd
        const existingAbpImportInAdd = Array.from(importsToAdd).find(imp => imp.includes('@abp/ng.core'));
        if (existingAbpImportInAdd) {
          const match = existingAbpImportInAdd.match(/import type { ([^}]+) } from '@abp\/ng.core';/);
          if (match) {
            const importsInAdd = match[1].split(',').map(i => i.trim());
            existingImports = [...new Set([...existingImports, ...importsInAdd])];
            importsToAdd.delete(existingAbpImportInAdd);
          }
        }
        
        // Merge all imports
        const allImports = [...new Set([...existingImports, ...baseImports])].sort().join(', ');
        const importStatement = `import type { ${allImports} } from '@abp/ng.core';`;
        
        // Only add if not already in file content
        if (!modelsContent.includes(importStatement)) {
          importsToAdd.add(importStatement);
        }
      }
      
      // Check for enum imports needed
      if (typeDef.properties) {
        for (const prop of typeDef.properties) {
          if (prop.typeSimple === 'enum' || prop.typeSimple === 'enum?') {
            const enumType = prop.type;
            const enumName = extractDtoName(enumType);
            
            // Calculate relative path to enum file
            let enumImportPath = getEnumImportPath(enumType, modulePath);
            if (enumImportPath) {
              // Check if this enum is already imported from this path
              const enumImportRegex = new RegExp(`import\\s+type\\s+{[^}]*\\b${enumName}\\b[^}]*}\\s+from\\s+['"]${enumImportPath.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')}['"];?`, 'g');
              const alreadyImported = enumImportRegex.test(modelsContent);
              
              if (!alreadyImported) {
                // Check if there's already an import from this path that we can merge with
                const existingImportFromPath = Array.from(importsToAdd).find(imp => imp.includes(enumImportPath));
                if (existingImportFromPath) {
                  // Extract existing imports and merge
                  const match = existingImportFromPath.match(/import\s+type\s+{\s*([^}]+)\s*}\s+from/);
                  if (match) {
                    const existingImports = match[1].split(',').map(i => i.trim());
                    if (!existingImports.includes(enumName)) {
                      const allImports = [...existingImports, enumName].join(', ');
                      importsToAdd.delete(existingImportFromPath);
                      importsToAdd.add(`import type { ${allImports} } from '${enumImportPath}';`);
                    }
                  }
                } else {
                  // Add new import
                  const importStatement = `import type { ${enumName} } from '${enumImportPath}';`;
                  importsToAdd.add(importStatement);
                }
              }
            }
          }
        }
      }
    }
    
    if (newInterfaces.length > 0) {
      // Sort interfaces by dependencies (simple approach: put simpler ones first)
      newInterfaces.sort((a, b) => {
        const aDef = types[a.type];
        const bDef = types[b.type];
        const aProps = aDef?.properties?.length || 0;
        const bProps = bDef?.properties?.length || 0;
        return aProps - bProps;
      });
      
      // Find where to insert (after imports, before existing interfaces)
      const importRegex = /^import[^;]+;$/gm;
      const imports = modelsContent.match(importRegex) || [];
      let insertPosition = 0;
      
      if (imports.length > 0) {
        // Find the end of the last import
        const lastImport = imports[imports.length - 1];
        const lastImportIndex = modelsContent.lastIndexOf(lastImport);
        insertPosition = lastImportIndex + lastImport.length;
        
        // Find the next newline after the last import
        const nextNewline = modelsContent.indexOf('\n', insertPosition);
        if (nextNewline > insertPosition) {
          insertPosition = nextNewline + 1;
        }
      } else {
        // No imports, insert at the beginning
        insertPosition = 0;
      }
      
      // Build new content
      const beforeInsert = modelsContent.substring(0, insertPosition);
      const afterInsert = modelsContent.substring(insertPosition);
      
      // Extract all existing imports from @abp/ng.core
      const abpCoreImportRegex = /import\s+type\s+{\s*([^}]+)\s*}\s+from\s+['"]@abp\/ng\.core['"];?/g;
      const existingAbpImports = new Set();
      let match;
      while ((match = abpCoreImportRegex.exec(beforeInsert)) !== null) {
        const imports = match[1].split(',').map(i => i.trim());
        imports.forEach(imp => existingAbpImports.add(imp));
      }
      
      // Collect all imports to add, separating @abp/ng.core from others
      const abpCoreImportsToAdd = new Set();
      const otherImportsByPath = new Map(); // Map<path, Set<importNames>>
      
      for (const imp of importsToAdd) {
        if (imp.includes('@abp/ng.core')) {
          const importMatch = imp.match(/import\s+type\s+{\s*([^}]+)\s*}\s+from/);
          if (importMatch) {
            const imports = importMatch[1].split(',').map(i => i.trim().replace(/\?$/, '')); // Remove trailing ?
            imports.forEach(imp => abpCoreImportsToAdd.add(imp));
          }
        } else {
          // Extract path and imports
          const importMatch = imp.match(/import\s+type\s+{\s*([^}]+)\s*}\s+from\s+['"]([^'"]+)['"];?/);
          if (importMatch) {
            const importNames = importMatch[1].split(',').map(i => i.trim().replace(/\?$/, '')); // Remove trailing ?
            const importPath = importMatch[2];
            
            // Check if any of these imports already exist in file
            const alreadyExists = importNames.some(name => {
              const regex = new RegExp(`import\\s+type\\s+{[^}]*\\b${name}\\b[^}]*}\\s+from\\s+['"]${importPath.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')}['"]`, 'g');
              return regex.test(beforeInsert) || regex.test(afterInsert);
            });
            
            if (!alreadyExists) {
              if (!otherImportsByPath.has(importPath)) {
                otherImportsByPath.set(importPath, new Set());
              }
              importNames.forEach(name => otherImportsByPath.get(importPath).add(name));
            }
          } else if (!beforeInsert.includes(imp) && !afterInsert.includes(imp)) {
            // Fallback for imports that don't match the pattern
            if (!otherImportsByPath.has('')) {
              otherImportsByPath.set('', new Set());
            }
            otherImportsByPath.get('').add(imp);
          }
        }
      }
      
      // Merge all @abp/ng.core imports
      const allAbpImports = [...new Set([...existingAbpImports, ...abpCoreImportsToAdd])].sort();
      
      // Remove existing @abp/ng.core imports from beforeInsert
      let cleanedBeforeInsert = beforeInsert.replace(abpCoreImportRegex, '');
      
      // Build new content
      let newContent = cleanedBeforeInsert;
      
      // Add merged @abp/ng.core import if there are any
      if (allAbpImports.length > 0) {
        newContent += `import type { ${allAbpImports.join(', ')} } from '@abp/ng.core';\n`;
      }
      
      // Add other imports, grouped by path
      for (const [importPath, importNames] of otherImportsByPath.entries()) {
        if (importPath === '') {
          // Fallback imports (don't match pattern)
          importNames.forEach(imp => {
            newContent += imp + '\n';
          });
        } else {
          const sortedImports = Array.from(importNames).sort();
          newContent += `import type { ${sortedImports.join(', ')} } from '${importPath}';\n`;
        }
      }
      
      // Add blank line if needed
      if (newContent.trim().length > 0 && !newContent.endsWith('\n\n')) {
        if (!newContent.endsWith('\n')) {
          newContent += '\n';
        }
        newContent += '\n';
      }
      
      // Add new interfaces
      newContent += newInterfaces.map(i => i.code).join('\n\n');
      
      // Add remaining content with proper spacing
      if (afterInsert.trim().length > 0) {
        if (!newContent.endsWith('\n\n')) {
          newContent += '\n\n';
        }
        newContent += afterInsert;
      }
      
      fs.writeFileSync(modelsPath, newContent, 'utf8');
      console.log(`   ✅ Added ${newInterfaces.length} new interface(s)`);
      totalAdded += newInterfaces.length;
    } else {
      console.log(`   ✓ All DTOs already exist`);
    }
    console.log('');
  }
  
  if (totalAdded > 0) {
    console.log(`✨ Done! Added ${totalAdded} interface(s) in total.`);
  } else {
    console.log('✨ Done! No missing DTOs found.');
  }
  
  // Fix enum imports after generating DTOs
  console.log('\n🔍 Fixing enum imports...');
  try {
    require('./fix-enum-imports.js');
  } catch (error) {
    console.warn('⚠️  Could not run fix-enum-imports.js:', error.message);
  }
}

// Run the script
main();
