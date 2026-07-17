using ClosedXML.Excel;
using ClosedXML.Report;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using iOne.HrDepartments;
using iOne.HrEmployees;
using iOne.Master.ResDocuments;
using iOne.Report.Localization;
using iOne.Report.ReportTemplateParameters;
using iOne.Report.ReportTemplateSqlParameters;
using iOne.Report.ReportTemplateSqls;
using iOne.Reports;
using iOne.ResDocuments;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;
using Volo.Abp.Domain.Repositories;


namespace iOne.Report.ReportTemplates
{
    public class ReportTemplateAppService : CrudAppService<ReportTemplate, ReportTemplateDto, Guid, FilterDto, CreateReportTemplateDto, UpdateReportTemplateDto>, IReportTemplateAppService
    {
        /// <summary>Mã mẫu báo cáo tìm kiếm đơn — server tự gắn tham số phạm vi theo <see cref="HrEmployee"/> đăng nhập.</summary>
        public const string PolicySearchReportTemplateCode = "POLICY_SEARCH";

        // Set localization resource for this service in constructor

        private readonly IRepository<ReportTemplateParameter, Guid> _paramRepo;
        private readonly IRepository<ReportTemplateSql, Guid> _sqlRepo;
        private readonly IRepository<ReportTemplateSqlParameter> _sqlParamRepo;
        private readonly IReportTemplateSqlParameterAppService _reportTemplateSqlParameterAppService;
        private readonly IReportTemplateParameterRepository _parameterRepository;
        private readonly IReportTemplateRepository _reportTemplateRepository;
        private readonly IReportTemplateSqlRepository _sqlRepository;
        private readonly IReportTemplateSqlParameterRepository _sqlParamRepository;
        private readonly IConfiguration _configuration;
        private readonly IResDocumentAppService _resDocumentAppService;
        private readonly ILogger<ReportTemplateAppService> _logger;
        private readonly IRepository<HrEmployee, Guid> _hrEmployeeRepository;
        private readonly IRepository<HrDepartment, Guid> _hrDepartmentRepository;

        public ReportTemplateAppService(
            IRepository<ReportTemplate, Guid> repository,
            IRepository<ReportTemplateParameter, Guid> paramRepo,
            IRepository<ReportTemplateSql, Guid> sqlRepo,
            IRepository<ReportTemplateSqlParameter> sqlParamRepo,
            IReportTemplateSqlParameterAppService reportTemplateSqlParameterAppService,
            IReportTemplateParameterRepository parameterRepository,
            IReportTemplateRepository reportTemplateRepository,
            IReportTemplateSqlRepository sqlRepository,
            IReportTemplateSqlParameterRepository sqlParamRepository,
            IConfiguration configuration,
            IResDocumentAppService resDocumentAppService,
            ILogger<ReportTemplateAppService> logger,
            IRepository<HrEmployee, Guid> hrEmployeeRepository,
            IRepository<HrDepartment, Guid> hrDepartmentRepository
             ) : base(repository)
        {
            LocalizationResource = typeof(ReportResource);
            _paramRepo = paramRepo;
            _sqlRepo = sqlRepo;
            _sqlParamRepo = sqlParamRepo;
            _reportTemplateSqlParameterAppService = reportTemplateSqlParameterAppService;
            _parameterRepository = parameterRepository;
            _sqlParamRepository = sqlParamRepository;
            _sqlRepository = sqlRepository;
            _reportTemplateRepository = reportTemplateRepository;
            _configuration = configuration;
            _resDocumentAppService = resDocumentAppService;
            _logger = logger;
            _hrEmployeeRepository = hrEmployeeRepository;
            _hrDepartmentRepository = hrDepartmentRepository;
        }

        public override async Task<PagedResultDto<ReportTemplateDto>> GetListAsync(FilterDto input)
        {
            var query = await CreateFilteredQueryAsync(input);

            query = ApplySorting(query, input);
            query = ApplyPaging(query, input);

            var totalCount = await AsyncExecuter.CountAsync(query);
            var entities = await AsyncExecuter.ToListAsync(query);

            var items = ObjectMapper.Map<List<ReportTemplate>, List<ReportTemplateDto>>(entities);

            return new PagedResultDto<ReportTemplateDto>(totalCount, items);
        }

        public async Task<List<ReportTemplateDDL>> GetTemplateDdlsAsync()
        {
            var query = await Repository.GetQueryableAsync();
            var list = await AsyncExecuter.ToListAsync(query.Where(
                x => x.Status == "active" &&
                x.EffectDate <= DateTime.Now &&
                x.ExpireDate >= DateTime.Now));
            var ddls = list.Select(t => new ReportTemplateDDL
            {
                Name = t.Name,
                Code = t.Code,
                Id = t.Id
            }).ToList();
            return ddls;
        }

        public async Task<List<ReportTemplateParameterDto>> GetParameterAsync(Guid reportTemplateId)
        {
            var parameter = await _parameterRepository.GetQueryableAsync();
            var list = await AsyncExecuter.ToListAsync(parameter.Where(
                x => x.Status == "active" && x.ReportTemplateId == reportTemplateId));

            return ObjectMapper.Map<List<ReportTemplateParameter>, List<ReportTemplateParameterDto>>(list);
        }

        public async Task<ReportTemplateDetailDto> GetDetailAsync(Guid id)
        {
            // Get template (will throw if not found)
            var template = await Repository.GetAsync(id);

            // Get parameters for this template

            var parameters = await this._parameterRepository.GetListByTemplateIdAsync(id);
            var parameterDtos = ObjectMapper.Map<List<ReportTemplateParameter>, List<ReportTemplateParameterDto>>(parameters);

            // Get SQLs for this template
            var sqls = await this._sqlRepository.GetListByTemplateIdAsync(id);

            // Get all sql-parameter mappings for these sqls
            var sqlIds = sqls.Select(s => s.Id).ToList();
            List<ReportTemplateSqlParameter> sqlParams = new();
            if (sqlIds.Any())
            {
                sqlParams = await this._sqlParamRepository.GetListBySqlIdsAsync(sqlIds);
            }

            // Build parameter id -> code dictionary
            var paramCodeDict = parameters.ToDictionary(p => p.Id, p => p.Code);

            // Map sqls to detail dtos including parameter codes
            var sqlDetailDtos = new List<ReportTemplateSqlDetailDto>();
            foreach (var s in sqls)
            {
                var paramCodes = sqlParams
                    .Where(sp => sp.SqlId == s.Id)
                    .Select(sp => paramCodeDict.TryGetValue(sp.ParameterId, out var code) ? code : null)
                    .Where(c => c != null)
                    .Select(c => c!)
                    .ToList();

                var dto = new ReportTemplateSqlDetailDto
                {
                    Id = s.Id,
                    ReportTemplateId = s.ReportTemplateId,
                    SqlText = s.SqlText,
                    VarName = s.VarName,
                    Status = s.Status,
                    IsSingle = s.IsSingle,
                    ParameterCodes = paramCodes
                };

                sqlDetailDtos.Add(dto);
            }

            // Assemble detail dto
            var detail = new ReportTemplateDetailDto
            {
                Id = template.Id,
                Name = template.Name,
                Code = template.Code,
                DocumentId = template.DocumentId,
                Description = template.Description,
                Status = template.Status,
                EffectDate = template.EffectDate,
                ExpireDate = template.ExpireDate,
                Parameters = parameterDtos,
                Sqls = sqlDetailDtos
            };

            // Populate file detail if DocumentId is set
            try
            {
                if (template.DocumentId != Guid.Empty)
                {
                    var file = await _resDocumentAppService.GetSingleFileAsync(template.DocumentId);
                    detail.Document = file;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not get attached document for report template {TemplateId}", id);
                throw new UserFriendlyException(
                    "Không tìm thấy tài liệu đính kèm của mẫu báo cáo."
                );
            }

            return detail;
        }

        public override async Task<ReportTemplateDto> CreateAsync(CreateReportTemplateDto input)
        {
            // Map and insert template
            var template = ObjectMapper.Map<CreateReportTemplateDto, ReportTemplate>(input);
            template = await Repository.InsertAsync(template);

            var paramDict = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);

            // Insert parameters
            if (input.Parameters != null)
            {
                foreach (var p in input.Parameters)
                {
                    // ensure ReportTemplateId is set for DTO mapping
                    p.ReportTemplateId = template.Id;
                    var paramEntity = ObjectMapper.Map<CreateReportTemplateParameterDto, ReportTemplateParameter>(p);
                    await _paramRepo.InsertAsync(paramEntity);
                    paramDict[p.Code] = paramEntity.Id;
                }
            }

            // Insert SQLs and mappings
            if (input.Sqls != null)
            {
                foreach (var s in input.Sqls)
                {
                    s.ReportTemplateId = template.Id;
                    var sqlEntity = ObjectMapper.Map<CreateReportTemplateSqlDto, ReportTemplateSql>(s);
                    await _sqlRepo.InsertAsync(sqlEntity);

                    if (s.ParameterCodes != null)
                    {
                        foreach (var code in s.ParameterCodes)
                        {
                            if (!paramDict.TryGetValue(code, out var paramId))
                            {
                                throw new UserFriendlyException(L["ReportTemplate:ParameterNotFound", code]);
                            }

                            CreateUpdateReportTemplateSqlParameterDto dto = new()
                            {
                                SqlId = sqlEntity.Id,
                                ParameterId = paramId
                            };

                            await _reportTemplateSqlParameterAppService.CreateAsync(dto);
                        }
                    }
                }
            }

            // UnitOfWork will save changes automatically; return created dto
            return ObjectMapper.Map<ReportTemplate, ReportTemplateDto>(template);
        }

        [Volo.Abp.Uow.UnitOfWork]
        public override async Task<ReportTemplateDto> UpdateAsync(Guid id, UpdateReportTemplateDto input)
        {
            // Update main template fields
            var template = await Repository.GetAsync(id);
            ObjectMapper.Map(input, template);
            template = await Repository.UpdateAsync(template);

            // Load existing parameters and sqls
            var existingParams = await _parameterRepository.GetListByTemplateIdAsync(id);
            var existingParamsById = existingParams.ToDictionary(x => x.Id);

            var existingSqls = await _sqlRepository.GetListByTemplateIdAsync(id);
            var existingSqlsById = existingSqls.ToDictionary(x => x.Id);

            // --- Parameters: create or update ---
            var processedParamIds = new HashSet<Guid>();
            if (input.Parameters != null)
            {
                foreach (var p in input.Parameters)
                {
                    if (p.Id.HasValue && p.Id.Value != Guid.Empty && existingParamsById.ContainsKey(p.Id.Value))
                    {
                        // update existing
                        var entity = await _paramRepo.GetAsync(p.Id.Value);
                        ObjectMapper.Map(p, entity);
                        await _paramRepo.UpdateAsync(entity, autoSave: true);
                        processedParamIds.Add(entity.Id);
                    }
                    else
                    {
                        // create new
                        var createDto = new CreateReportTemplateParameterDto
                        {
                            ReportTemplateId = id,
                            Code = p.Code,
                            Name = p.Name,
                            DataType = p.DataType,
                            Status = p.Status,
                            Description = p.Description
                        };

                        var entity = ObjectMapper.Map<CreateReportTemplateParameterDto, ReportTemplateParameter>(createDto);
                        await _paramRepo.InsertAsync(entity, autoSave: true);
                        processedParamIds.Add(entity.Id);
                    }
                }
            }

            // --- Parameters: delete removed and their mappings ---
            var toDeleteParams = existingParams.Where(ep => !processedParamIds.Contains(ep.Id)).ToList();
            foreach (var del in toDeleteParams)
            {
                // delete sql-parameter mappings that reference this parameter
                var mappings = await _sqlParamRepository.GetListByParameterIdAsync(del.Id);
                foreach (var m in mappings)
                {
                    await _sqlParamRepo.DeleteAsync(m, autoSave: true);
                }

                await _paramRepo.DeleteAsync(del, autoSave: true);
            }

            // Ensure parameter changes persisted so paramCodeDict is correct
            await CurrentUnitOfWork.SaveChangesAsync();

            // Rebuild parameter code -> id dictionary
            var currentParams = await _parameterRepository.GetListByTemplateIdAsync(id);
            var paramCodeDict = currentParams.ToDictionary(x => x.Code, x => x.Id, StringComparer.OrdinalIgnoreCase);

            // --- SQLs: create or update, and rebuild mappings per sql ---
            var processedSqlIds = new HashSet<Guid>();
            if (input.Sqls != null)
            {
                foreach (var s in input.Sqls)
                {
                    ReportTemplateSql sqlEntity;

                    if (s.Id.HasValue && s.Id.Value != Guid.Empty && existingSqlsById.ContainsKey(s.Id.Value))
                    {
                        // update existing
                        sqlEntity = await _sqlRepo.GetAsync(s.Id.Value);
                        var updateDto = new UpdateReportTemplateSqlDto
                        {
                            SqlText = s.SqlText,
                            VarName = s.VarName,
                            Status = s.Status,
                            IsSingle = s.IsSingle,
                            ParameterCodes = s.ParameterCodes ?? new List<string>()
                        };
                        ObjectMapper.Map(updateDto, sqlEntity);
                        await _sqlRepo.UpdateAsync(sqlEntity, autoSave: true);
                        processedSqlIds.Add(sqlEntity.Id);
                    }
                    else
                    {
                        // create new
                        var createDto = new CreateReportTemplateSqlDto
                        {
                            ReportTemplateId = id,
                            SqlText = s.SqlText,
                            VarName = s.VarName,
                            Status = s.Status,
                            IsSingle = s.IsSingle,
                            ParameterCodes = s.ParameterCodes ?? new List<string>()
                        };
                        sqlEntity = ObjectMapper.Map<CreateReportTemplateSqlDto, ReportTemplateSql>(createDto);
                        await _sqlRepo.InsertAsync(sqlEntity, autoSave: true);
                        processedSqlIds.Add(sqlEntity.Id);
                    }

                    // remove existing mappings for this sql
                    var existingMappings = await _sqlParamRepository.GetListBySqlIdAsync(sqlEntity.Id);
                    foreach (var m in existingMappings)
                    {
                        await _sqlParamRepo.DeleteAsync(m, autoSave: true);
                    }

                    // create mappings based on parameter codes
                    if (s.ParameterCodes != null)
                    {
                        foreach (var code in s.ParameterCodes)
                        {
                            if (!paramCodeDict.TryGetValue(code, out var paramId))
                            {
                                throw new UserFriendlyException(L["ReportTemplate:ParameterNotFound", code]);
                            }

                            var mapDto = new CreateUpdateReportTemplateSqlParameterDto
                            {
                                SqlId = sqlEntity.Id,
                                ParameterId = paramId
                            };

                            await _reportTemplateSqlParameterAppService.CreateAsync(mapDto);
                        }
                    }
                }
            }

            // Ensure SQL inserts/updates and mapping changes are saved before deleting removed SQLs
            await CurrentUnitOfWork.SaveChangesAsync();

            // --- SQLs: delete removed and their mappings ---
            var toDeleteSqls = existingSqls.Where(es => !processedSqlIds.Contains(es.Id)).ToList();
            foreach (var delSql in toDeleteSqls)
            {
                var mappings = await _sqlParamRepository.GetListBySqlIdAsync(delSql.Id);
                foreach (var m in mappings)
                {
                    await _sqlParamRepo.DeleteAsync(m, autoSave: true);
                }

                await _sqlRepo.DeleteAsync(delSql, autoSave: true);
            }

            // Final save within unit of work
            await CurrentUnitOfWork.SaveChangesAsync();

            // return updated template dto
            return ObjectMapper.Map<ReportTemplate, ReportTemplateDto>(template);
        }

        protected override async Task<IQueryable<ReportTemplate>> CreateFilteredQueryAsync(FilterDto input)
        {
            var query = await base.CreateFilteredQueryAsync(input);

            if (!input.name.IsNullOrWhiteSpace())
            {
                query = query.Where(x => EF.Functions.ILike(x.Name, $"%{input.name}%"));
            }
            if (!input.code.IsNullOrWhiteSpace())
            {
                query = query.Where(x => EF.Functions.ILike(x.Code, $"%{input.code}%"));
            }
            if (!input.status.IsNullOrWhiteSpace())
            {
                query = query.Where(x =>
                    x.Status.Equals(input.status)
                );
            }

            return query;
        }

        public async Task<IRemoteStreamContent> ExportAsync(FilterDto input)
        {
            var list = await GetListAsync(input);

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("ReportTemplates");

            var headers = new[]
            {
                "Id", "Name", "Code", "DocumentId", "Description",
                "Status", "EffectDate", "ExpireDate", "ParameterCount", "SqlCount"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(1, i + 1).Value = headers[i];
                ws.Cell(1, i + 1).Style.Font.Bold = true;
            }

            int row = 2;
            foreach (var t in list.Items)
            {
                int paramCount = 0;
                int sqlCount = 0;

                try
                {
                    var detail = await GetDetailAsync(t.Id);
                    paramCount = detail.Parameters?.Count ?? 0;
                    sqlCount = detail.Sqls?.Count ?? 0;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error getting detail for template {TemplateId}", t.Id);
                    // ignore and continue
                }

                ws.Cell(row, 1).Value = t.Id.ToString();
                ws.Cell(row, 2).Value = t.Name ?? string.Empty;
                ws.Cell(row, 3).Value = t.Code ?? string.Empty;
                ws.Cell(row, 4).Value = t.DocumentId.ToString();
                ws.Cell(row, 5).Value = t.Description ?? string.Empty;
                ws.Cell(row, 6).Value = t.Status ?? string.Empty;
                ws.Cell(row, 7).Value = t.EffectDate?.ToString("yyyy-MM-dd") ?? string.Empty;
                ws.Cell(row, 8).Value = t.ExpireDate?.ToString("yyyy-MM-dd") ?? string.Empty;
                ws.Cell(row, 9).Value = paramCount;
                ws.Cell(row, 10).Value = sqlCount;

                row++;
            }

            ws.Columns().AdjustToContents();

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            var fileName = $"report-templates_{DateTime.Now:yyyyMMdd}.xlsx";

            return new RemoteStreamContent(
                stream,
                fileName,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            );
        }

        public async Task<IRemoteStreamContent> ExportDynamicFileByCode(string code, Dictionary<string, object> parameters)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new UserFriendlyException(L["ReportTemplate:NotFoundByCode", code ?? ""]);
            }

            var query = await Repository.GetQueryableAsync();
            var template = await AsyncExecuter.FirstOrDefaultAsync(
                query.Where(x => x.Code != null && x.Code.Trim().ToLower() == code.Trim().ToLower()));

            if (template == null)
            {
                throw new UserFriendlyException(L["ReportTemplate:NotFoundByCode", code]);
            }

            // Debug help: confirm which templateId is used for this export-by-code call.
            try
            {
                var paramKeys = parameters?.Keys?.Take(20) ?? Enumerable.Empty<string>();
                _logger.LogInformation(
                    "ExportDynamicFileByCode called. Code={Code}, TemplateId={TemplateId}, ParamKeys=[{ParamKeys}]",
                    code,
                    template.Id,
                    string.Join(", ", paramKeys)
                );
            }
            catch
            {
                // Ignore debug logging failures
            }

            var parameterDict = parameters == null
                ? new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, object>(parameters, StringComparer.OrdinalIgnoreCase);

            if (string.Equals(code.Trim(), PolicySearchReportTemplateCode, StringComparison.OrdinalIgnoreCase))
            {
                await MergePolicySearchViewerScopeAsync(parameterDict);
            }

            return await ExportDynamicFile(template.Id, parameterDict);
        }

        /// <summary>
        /// Tham số đồng bộ với <c>PolicySearchQueryScope</c> (Policy module): ViewerEmployeeId, ViewerPartnerId,
        /// ViewerDepartmentId, ViewerIsManager, ManagedDeptIds. SQL mẫu báo cáo cần khai báo và map các mã này.
        /// </summary>
        private async Task MergePolicySearchViewerScopeAsync(Dictionary<string, object> parameters)
        {
            if (CurrentUser.Id == null)
            {
                throw new UserFriendlyException(L["ReportTemplate:PolicySearchNotLoggedIn"]);
            }

            var employee = await _hrEmployeeRepository.FirstOrDefaultAsync(e => e.UserId == CurrentUser.Id.Value);
            if (employee == null)
            {
                throw new UserFriendlyException(L["ReportTemplate:PolicySearchNoEmployee"]);
            }

            Guid[] managedDeptIds;
            if (employee.PartnerId.HasValue || employee.IsManager != true)
            {
                managedDeptIds = Array.Empty<Guid>();
            }
            else
            {
                var root = employee.OrgId ?? employee.DepartmentId;
                var subtree = await BuildReportManagedDepartmentSubtreeAsync(root);
                managedDeptIds = subtree.Count > 0 ? subtree.ToArray() : Array.Empty<Guid>();
            }

            parameters["ViewerEmployeeId"] = employee.Id;
            parameters["ViewerPartnerId"] = employee.PartnerId.HasValue ? employee.PartnerId.Value : DBNull.Value;
            parameters["ViewerDepartmentId"] = employee.DepartmentId;
            parameters["ViewerIsManager"] = employee.IsManager == true;
            parameters["ManagedDeptIds"] = managedDeptIds;
        }

        private async Task<List<Guid>> BuildReportManagedDepartmentSubtreeAsync(Guid rootDepartmentId)
        {
            if (rootDepartmentId == Guid.Empty)
            {
                return new List<Guid>();
            }

            var q = await _hrDepartmentRepository.GetQueryableAsync();
            var rows = await AsyncExecuter.ToListAsync(
                q.Where(d => !d.IsDeleted).Select(d => new { d.Id, d.ParentId }));

            if (!rows.Any(r => r.Id == rootDepartmentId))
            {
                return new List<Guid>();
            }

            var childrenByParent = rows
                .Where(r => r.ParentId.HasValue)
                .GroupBy(r => r.ParentId!.Value)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Id).ToList());

            var result = new List<Guid>();
            var queue = new Queue<Guid>();
            queue.Enqueue(rootDepartmentId);
            while (queue.Count > 0)
            {
                var id = queue.Dequeue();
                result.Add(id);
                if (childrenByParent.TryGetValue(id, out var children))
                {
                    foreach (var c in children)
                    {
                        queue.Enqueue(c);
                    }
                }
            }

            return result;
        }

        public async Task<IRemoteStreamContent> ExportDynamicFile(Guid templateId, Dictionary<string, object> parameters)
        {
            var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

            var sqls = await this._sqlRepository.GetListByTemplateIdAsync(templateId);
            if (sqls == null || !sqls.Any())
            {
                throw new UserFriendlyException(L["ReportTemplate:NoSqlsFound"]);
            }

            var connStr = _configuration.GetConnectionString("Default") ?? _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connStr))
            {
                throw new UserFriendlyException(L["ReportTemplate:DbConnectionNotConfigured"]);
            }

            try
            {
                await using var conn = new NpgsqlConnection(connStr);
                await conn.OpenAsync();

                foreach (var sql in sqls)
                {
                    try
                    {
                        var mappings = await _sqlParamRepository.GetListBySqlIdAsync(sql.Id);

                        var cmdText = sql.SqlText ?? string.Empty;
                        await using var cmd = conn.CreateCommand();
                        cmd.CommandType = CommandType.Text;

                        var paramNameByCode = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        int paramIndex = 0;

                        if (mappings != null && mappings.Any())
                        {
                            foreach (var m in mappings)
                            {
                                var paramEntity = await _parameterRepository.GetAsync(m.ParameterId);
                                if (paramEntity == null) continue;

                                var code = paramEntity.Code ?? string.Empty;

                                // find value in provided parameters (case-insensitive)
                                object? value = null;
                                if (!parameters.TryGetValue(code, out value))
                                {
                                    var kv = parameters.FirstOrDefault(k => string.Equals(k.Key, code, StringComparison.OrdinalIgnoreCase));
                                    if (!kv.Equals(default(KeyValuePair<string, object>)))
                                    {
                                        value = kv.Value;
                                    }
                                }

                                // create a safe parameter name and add parameter once per code
                                if (!paramNameByCode.ContainsKey(code))
                                {
                                    var safeCode = Regex.Replace(code, "[^a-zA-Z0-9_]", "_");
                                    var paramName = $"p{paramIndex++}_{safeCode}";
                                    paramNameByCode[code] = paramName;

                                    // replace common placeholder patterns with @paramName (safe)
                                    if (!string.IsNullOrWhiteSpace(code))
                                    {
                                        cmdText = cmdText.Replace("{" + code + "}", "@" + paramName, StringComparison.OrdinalIgnoreCase);
                                        cmdText = cmdText.Replace(":" + code, "@" + paramName, StringComparison.OrdinalIgnoreCase);
                                        // replace @code occurrences (case-insensitive)
                                        cmdText = Regex.Replace(cmdText, "@" + Regex.Escape(code), "@" + paramName, RegexOptions.IgnoreCase);
                                    }

                                    // Empty string as null so UUID/date params get NULL and "IS NULL" conditions work (avoids uuid = text error)
                                    object? paramValue = value;
                                    if (value is string str && string.IsNullOrWhiteSpace(str))
                                        paramValue = DBNull.Value;
                                    // Set explicit NpgsqlDbType so PostgreSQL knows parameter type (fixes 42P08). Use UUID for *_id params to fix 42883: uuid = text.
                                    var dbType = GetNpgsqlDbTypeFromParameterDataType(paramEntity.DataType, code);
                                    var valueForDb = CoerceParameterValueToDbType(paramValue, dbType);
                                    var npgParam = new NpgsqlParameter(paramName, dbType) { Value = valueForDb };
                                    cmd.Parameters.Add(npgParam);
                                }
                            }
                        }

                        cmd.CommandText = cmdText;

                        var rows = new List<Dictionary<string, object?>>();
                        await using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    var name = reader.GetName(i);
                                    var val = reader.IsDBNull(i) ? null : reader.GetValue(i);
                                    row[name] = val;
                                }
                                rows.Add(row);
                            }
                        }

                        // Some XLSX templates expect a `stt` column for row numbering
                        // (e.g. {{item.stt}}). If SQL doesn't provide it, populate it
                        // based on the result order.
                        if (rows.Count > 0 && !rows[0].ContainsKey("stt"))
                        {
                            for (int i = 0; i < rows.Count; i++)
                            {
                                rows[i]["stt"] = i + 1;
                            }
                        }

                        var key = !string.IsNullOrWhiteSpace(sql.VarName) ? sql.VarName : sql.Id.ToString();

                        if (sql.IsSingle)
                        {
                            var obj = rows.FirstOrDefault();
                            if (obj != null)
                            {
                                foreach (var kv in obj)
                                {
                                    result[kv.Key] = kv.Value;
                                }
                            }
                        }
                        else
                        {
                            result[key] = rows?.ToList() ?? new List<Dictionary<string, object?>>();
                        }

                        // Debug help: track whether the SQL actually returned data.
                        // (Excel binding issues often look like "empty file" even when SQL returned rows.)
                        try
                        {
                            string firstRowColumns = string.Empty;
                            if (rows.Count > 0)
                            {
                                firstRowColumns = string.Join(", ", rows[0].Keys.Take(30));
                            }

                            _logger.LogInformation(
                                "ReportTemplate SQL executed. TemplateId={TemplateId}, SqlId={SqlId}, VarName={VarName}, IsSingle={IsSingle}, RowCount={RowCount}, FirstRowColumns={FirstRowColumns}",
                                templateId,
                                sql.Id,
                                sql.VarName,
                                sql.IsSingle,
                                rows.Count,
                                firstRowColumns
                            );
                        }
                        catch
                        {
                            // Ignore logging failures
                        }
                    }
                    catch (Exception ex)
                    {
                        var key = !string.IsNullOrWhiteSpace(sql.VarName) ? sql.VarName : sql.Id.ToString();
                        _logger.LogError(ex, "Error executing SQL {SqlId} (VarName={VarName}) for template {TemplateId}", sql.Id, sql.VarName, templateId);
                        // attach localized error message
                        result[key] = new { error = L["ReportTemplate:ErrorExecutingSqls", ex.Message].ToString() };
                    }
                }

                await conn.CloseAsync();

                // Debug summary: show what keys/structures the template can bind.
                try
                {
                    var keysPreview = result.Keys.Take(30).ToArray();
                    var keySummaries = new List<string>();

                    foreach (var k in keysPreview)
                    {
                        if (!result.TryGetValue(k, out var v))
                        {
                            keySummaries.Add($"{k}=<missing>");
                            continue;
                        }

                        if (v is string || v == null)
                        {
                            keySummaries.Add($"{k}=single");
                            continue;
                        }

                        if (v is System.Collections.ICollection col)
                        {
                            keySummaries.Add($"{k}=list({col.Count})");
                        }
                        else
                        {
                            keySummaries.Add($"{k}=list(?)");
                        }
                    }

                    _logger.LogInformation(
                        "ReportTemplate result summary. TemplateId={TemplateId}, KeysPreview=[{KeysPreview}], KeySummaries=[{KeySummaries}]",
                        templateId,
                        string.Join(", ", keysPreview),
                        string.Join("; ", keySummaries)
                    );
                }
                catch
                {
                    // Ignore debug logging failures
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while executing SQLs for template {TemplateId}", templateId);
                throw new UserFriendlyException(L["ReportTemplate:ErrorExecutingSqls", ex.Message]);
            }

            // Try to get template metadata (URL) and download template bytes if possible
            try
            {
                var template = await Repository.GetAsync(templateId);
                var templateFile = await _resDocumentAppService.GetSingleFileAsync(template.DocumentId);
                var ext = Path.GetExtension(templateFile.FileName)?.ToLower();

                if (ext == ".docx")
                {
                    if (!string.IsNullOrWhiteSpace(templateFile?.Url))
                    {
                        try
                        {
                            var httpClient = new HttpClient();
                            var bytes = await httpClient.GetByteArrayAsync(templateFile.Url!);

                            var memoryStream = new MemoryStream();
                            memoryStream.Write(bytes, 0, bytes.Length);
                            memoryStream.Position = 0;

                            using (var wordDoc = WordprocessingDocument.Open(memoryStream, true))
                            {
                                try
                                {
                                    ReplaceSingleFields(wordDoc, result);
                                }
                                catch (Exception ex)
                                {
                                    Logger.LogError(ex, "Error in ReplaceSingleFields");
                                    throw;
                                }

                                try
                                {
                                    FillMultipleTables(wordDoc, result);
                                }
                                catch (Exception ex)
                                {
                                    Logger.LogError(ex, "Error in FillMultipleTables");
                                    throw;
                                }

                                try
                                {
                                    wordDoc.MainDocumentPart.Document.Save();
                                }
                                catch (Exception ex)
                                {
                                    Logger.LogError(ex, "Error when saving document");
                                    throw;
                                }
                            }

                            memoryStream.Position = 0;

                            return new RemoteStreamContent(
                                memoryStream,
                                "Report.docx",
                                "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
                            );

                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error creating DOCX report for template {TemplateId} from URL {Url}", templateId, templateFile?.Url);
                            throw new UserFriendlyException("Something went wrong when create file");
                        }
                    }
                }
                else if (ext == ".xlsx")
                {
                    if (!string.IsNullOrWhiteSpace(templateFile?.Url))
                    {
                        try
                        {
                            using var httpClient = new HttpClient();
                            var bytes = await httpClient.GetByteArrayAsync(templateFile.Url!);

                            // If we got bytes, attempt to open as XLTemplate and bind variables
                            using var msIn = new MemoryStream(bytes);
                            var xlTemplate = new XLTemplate(msIn);

                            // ClosedXML templates can be sensitive to variable name casing depending on template authoring.
                            // Generate common casing variants before binding to avoid "blank" outputs.
                            var variablesForXlsx = BuildXlsxVariables(result);
                            xlTemplate.AddVariable(variablesForXlsx);
                            xlTemplate.Generate();

                            var outStream = new MemoryStream();
                            xlTemplate.SaveAs(outStream);
                            outStream.Position = 0;

                            return new RemoteStreamContent(
                                outStream,
                                "Report.xlsx",
                                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                            );
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to process XLSX template {TemplateId} from URL {Url}, falling back to JSON", templateId, templateFile?.Url);
                            // ignore download or template parse/binding errors and fall back to JSON
                        }
                    }
                }
                else
                {
                    _logger.LogError("Unsupported template extension '{Ext}' for template {TemplateId}", ext, templateId);
                    throw new UserFriendlyException("Something went wrong when create file");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating report file for template {TemplateId}", templateId);
                throw new UserFriendlyException("Something went wrong when create file");
            }


            // Fallback: return JSON result
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(result, options);

            var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            ms.Position = 0;
            var fileName = $"report_dynamic_{DateTime.Now:yyyyMMdd_HHmmss}.json";

            return new RemoteStreamContent(ms, fileName, "application/json");
        }

        private void ReplaceSingleFields(
            WordprocessingDocument wordDoc,
            Dictionary<string, object> data)
        {
            // Replace {{Key}} placeholders case-insensitively (templates often differ in casing).
            // Only replace "single" values (skip list/table variables).
            var placeholderRegex = new Regex(
                @"\{\{\s*(?<key>[^}]+?)\s*\}\}",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

            var paragraphs = wordDoc.MainDocumentPart!
                .Document
                .Descendants<Paragraph>();

            foreach (var paragraph in paragraphs)
            {
                foreach (var text in paragraph.Descendants<Text>())
                {
                    if (!text.Text.Contains("{{")) continue;

                    text.Text = placeholderRegex.Replace(text.Text, match =>
                    {
                        var key = match.Groups["key"].Value.Trim();

                        if (!data.TryGetValue(key, out var val))
                            return match.Value; // keep placeholder when not found

                        // Only replace single scalars; list/table variables remain as-is.
                        if (val is IEnumerable && val is not string)
                            return match.Value;

                        return val?.ToString() ?? string.Empty;
                    });
                }
            }
        }


        private void RemoveRowContainsMarker(
            WordprocessingDocument wordDoc,
            string marker)
        {
            var body = wordDoc.MainDocumentPart.Document.Body;

            var rows = body.Descendants<TableRow>()
                .Where(r => r.InnerText.IndexOf(marker, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();

            foreach (var row in rows)
            {
                row.Remove();
            }
        }

        void FillMultipleTables(
            WordprocessingDocument wordDoc,
            Dictionary<string, object> data)
        {
            var body = wordDoc.MainDocumentPart.Document.Body;

            foreach (var entry in data)
            {
                if (entry.Value is not IEnumerable list || entry.Value is string) continue;

                var tableKey = entry.Key; // Products, Products2
                var items = list.Cast<object>().ToList();

                FillTableByKey(wordDoc, tableKey, items);
            }
        }

        private void FillTableByKey(
            WordprocessingDocument wordDoc,
            string tableKey,
            List<object> items)
        {
            var body = wordDoc.MainDocumentPart.Document.Body;

            // tìm table có marker {{#Key}}
            var table = body.Descendants<Table>()
                .FirstOrDefault(t => t.InnerText.IndexOf($"{{{{#{tableKey}}}}}", StringComparison.OrdinalIgnoreCase) >= 0);

            if (table == null) return;

            // xóa marker
            RemoveRowContainsMarker(wordDoc, $"{{{{#{tableKey}}}}}");

            var templateRow = table.Descendants<TableRow>()
                .FirstOrDefault(r => r.InnerText.Contains("{{"));

            if (templateRow == null) return;

            templateRow.Remove();

            int index = 1;
            var dictItems = items.Cast<Dictionary<string, object>>().ToList();

            var placeholderRegex = new Regex(
                @"\{\{\s*(?<key>[^}]+?)\s*\}\}",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);
            foreach (var item in dictItems)
            {
                var newRow = (TableRow)templateRow.CloneNode(true);

                foreach (var text in newRow.Descendants<Text>())
                {
                    if (!text.Text.Contains("{{")) continue;

                    text.Text = placeholderRegex.Replace(text.Text, match =>
                    {
                        var key = match.Groups["key"].Value.Trim();

                        if (key.Equals("Index", StringComparison.OrdinalIgnoreCase))
                            return index.ToString();

                        if (!item.TryGetValue(key, out var value))
                            return match.Value; // keep when missing

                        return FormatValue(value);
                    });
                }

                table.AppendChild(newRow);
                index++;
            }
        }

        private static Dictionary<string, object?> BuildXlsxVariables(Dictionary<string, object?> source)
        {
            // Generate common key casing variants to tolerate template authors using {{Name}} instead of {{name}} (or vice versa).
            // This is intentionally conservative; it won't change values, only adds alias keys.
            // ClosedXML.Report treats variable aliases in a way that can become case-insensitive
            // internally; if we add multiple casing variants (policy/Policy/POLICY) it can throw
            // "An item with the same key has already been added".
            // So we keep only one entry per case-insensitive alias.
            var variables = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

            foreach (var (key, value) in source)
            {
                if (value is IEnumerable enumerable && value is not string)
                {
                    var normalizedList = new List<object?>();
                    foreach (var item in enumerable)
                    {
                        if (item is Dictionary<string, object?> row)
                        {
                            normalizedList.Add(NormalizeRowKeys(row));
                        }
                        else if (item is Dictionary<string, object> rowObj)
                        {
                            var converted = rowObj.ToDictionary(
                                kv => kv.Key,
                                kv => (object?)kv.Value,
                                StringComparer.OrdinalIgnoreCase
                            );
                            normalizedList.Add(NormalizeRowKeys(converted));
                        }
                        else
                        {
                            normalizedList.Add(item);
                        }
                    }

                    foreach (var variant in GetKeyVariants(key))
                        variables[variant] = normalizedList;
                }
                else
                {
                    foreach (var variant in GetKeyVariants(key))
                        variables[variant] = value;
                }
            }

            // Workaround: some XLSX templates reference `{{item.<field>}}` directly in cells
            // without configuring ClosedXML.Report "flat table"/list range. In that case the
            // template engine won't have an `item` identifier unless we provide it.
            // We set `item` to the first element of the first collection variable (if present).
            if (!variables.ContainsKey("item"))
            {
                object? firstElement = null;
                foreach (var (_, value) in source)
                {
                    if (value is IEnumerable enumerable && value is not string)
                    {
                        var enumerator = enumerable.GetEnumerator();
                        if (enumerator.MoveNext())
                        {
                            firstElement = enumerator.Current;
                        }
                        break;
                    }
                }

                if (firstElement is Dictionary<string, object?> rowDict)
                {
                    var normalized = NormalizeRowKeys(rowDict);
                    variables["item"] = normalized;
                    foreach (var variant in GetKeyVariants("item"))
                        variables[variant] = normalized;
                }
                else if (firstElement is Dictionary<string, object> rowObjDict)
                {
                    var converted = rowObjDict.ToDictionary(
                        kv => kv.Key,
                        kv => (object?)kv.Value,
                        StringComparer.OrdinalIgnoreCase
                    );
                    var normalized = NormalizeRowKeys(converted);
                    variables["item"] = normalized;
                    foreach (var variant in GetKeyVariants("item"))
                        variables[variant] = normalized;
                }
                else if (firstElement != null)
                {
                    variables["item"] = firstElement;
                    foreach (var variant in GetKeyVariants("item"))
                        variables[variant] = firstElement;
                }
            }

            return variables;
        }

        private static Dictionary<string, object?> NormalizeRowKeys(Dictionary<string, object?> row)
        {
            var normalized = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (var (k, v) in row)
            {
                foreach (var variant in GetKeyVariants(k))
                    normalized[variant] = v;
            }
            return normalized;
        }

        private static IEnumerable<string> GetKeyVariants(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                yield break;

            var trimmed = key.Trim();

            // Original
            yield return trimmed;

            // Lower/upper/camel/pascal first letter variations
            var upperFirst = char.ToUpperInvariant(trimmed[0]) + trimmed[1..];
            var lowerFirst = char.ToLowerInvariant(trimmed[0]) + trimmed[1..];

            yield return upperFirst;
            yield return lowerFirst;
            yield return trimmed.ToUpperInvariant();
            yield return trimmed.ToLowerInvariant();
        }


        /// <summary>
        /// Maps report parameter DataType (and parameter code) to NpgsqlDbType. Uses code convention *_id => UUID to avoid 42883: operator does not exist: uuid = text.
        /// </summary>
        private static NpgsqlDbType GetNpgsqlDbTypeFromParameterDataType(string? dataType, string? parameterCode)
        {
            if (!string.IsNullOrWhiteSpace(dataType))
            {
                switch (dataType.Trim().ToLowerInvariant())
                {
                    case "uuid[]":
                    case "uuid_array":
                        return NpgsqlDbType.Array | NpgsqlDbType.Uuid;
                    case "uuid":
                    case "guid":
                        return NpgsqlDbType.Uuid;
                    case "int":
                    case "integer":
                    case "int32":
                        return NpgsqlDbType.Integer;
                    case "bigint":
                    case "int64":
                        return NpgsqlDbType.Bigint;
                    case "bool":
                    case "boolean":
                        return NpgsqlDbType.Boolean;
                    case "date":
                        return NpgsqlDbType.Date;
                    case "timestamp":
                    case "datetime":
                        return NpgsqlDbType.Timestamp;
                    case "numeric":
                    case "decimal":
                        return NpgsqlDbType.Numeric;
                }
            }
            // Convention: parameters ending with *_id (snake_case) OR *Id (camel/pascal) are typically UUIDs.
            // This avoids "operator does not exist: uuid = text" when the template parameter data_type isn't set to uuid.
            if (!string.IsNullOrWhiteSpace(parameterCode))
            {
                var trimmed = parameterCode.Trim();
                if (string.Equals(trimmed, "ManagedDeptIds", StringComparison.OrdinalIgnoreCase))
                {
                    return NpgsqlDbType.Array | NpgsqlDbType.Uuid;
                }

                if (string.Equals(trimmed, "ViewerIsManager", StringComparison.OrdinalIgnoreCase))
                {
                    return NpgsqlDbType.Boolean;
                }

                if (trimmed.EndsWith("_id", StringComparison.OrdinalIgnoreCase) ||
                    trimmed.EndsWith("id", StringComparison.OrdinalIgnoreCase))
                {
                    return NpgsqlDbType.Uuid;
                }

                // Convention: date params are typically stored as YYYY-MM-DD and compared against timestamp/date columns.
                // If template doesn't provide data_type metadata, infer from name to avoid "timestamp >= text".
                // Examples: EffectiveDateFrom, EffectiveDateTo, ApprovalDate, CreatedDate, ExpiryDateFrom...
                if (trimmed.Contains("datetime", StringComparison.OrdinalIgnoreCase) ||
                    trimmed.Contains("timestamp", StringComparison.OrdinalIgnoreCase))
                {
                    return NpgsqlDbType.Timestamp;
                }

                if (trimmed.Contains("date", StringComparison.OrdinalIgnoreCase))
                {
                    return NpgsqlDbType.Date;
                }
            }
            return NpgsqlDbType.Text;
        }

        /// <summary>
        /// Coerces the parameter value to the correct type for PostgreSQL (e.g. Guid for UUID, not string).
        /// </summary>
        private static object? CoerceParameterValueToDbType(object? value, NpgsqlDbType dbType)
        {
            if (value == null || value is DBNull)
                return DBNull.Value;
            if (dbType == (NpgsqlDbType.Array | NpgsqlDbType.Uuid))
            {
                if (value is Guid[] arr)
                {
                    return arr;
                }

                if (value is List<Guid> list)
                {
                    return list.ToArray();
                }

                return Array.Empty<Guid>();
            }

            if (dbType == NpgsqlDbType.Boolean)
            {
                if (value is bool b)
                {
                    return b;
                }

                if (value is string s && bool.TryParse(s, out var parsed))
                {
                    return parsed;
                }
            }

            if (dbType == NpgsqlDbType.Uuid)
            {
                if (value is Guid g)
                    return g;
                if (value is string s)
                {
                    if (string.IsNullOrWhiteSpace(s))
                        return DBNull.Value;
                    return Guid.TryParse(s, out var guid) ? guid : DBNull.Value;
                }
            }
            if (dbType == NpgsqlDbType.Date || dbType == NpgsqlDbType.Timestamp)
            {
                if (value is DateTime dt)
                    return dt;
                if (value is string s && !string.IsNullOrWhiteSpace(s) && DateTime.TryParse(s, out var parsed))
                    return parsed;
            }
            return value;
        }

        private string FormatValue(object value)
        {
            if (value == null) return string.Empty;

            return value switch
            {
                DateTime dt => dt.ToString("dd/MM/yyyy"),
                decimal d => d.ToString("N0"),
                double d => d.ToString("N0"),
                _ => value.ToString()
            };
        }
    }
}
