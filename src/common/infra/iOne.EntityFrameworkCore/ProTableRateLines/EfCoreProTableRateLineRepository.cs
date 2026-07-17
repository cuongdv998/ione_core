using System;
using System.Globalization;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using iOne.ProTableRateLines;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace iOne.EntityFrameworkCore.ProTableRateLines;

public class EfCoreProTableRateLineRepository : EfCoreRepository<iOneDbContext, ProTableRateLine, Guid>,
    IProTableRateLineRepository
{
    private readonly ILogger<EfCoreProTableRateLineRepository> _logger;

    public EfCoreProTableRateLineRepository(
        IDbContextProvider<iOneDbContext> dbContextProvider,
        ILogger<EfCoreProTableRateLineRepository> logger)
        : base(dbContextProvider)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ProTableRateLine?> FindMatchingRateLineAsync(
        Guid tableRateId,
        Guid? coverageId,
        Dictionary<string, (string Operator, object Value)> conditions,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        
        var sqlBuilder = new StringBuilder();
        var parameters = new List<NpgsqlParameter>();
        var paramIndex = 0;

        // Start building the SQL query
        sqlBuilder.Append(@"
            SELECT * FROM pro_table_rate_line
            WHERE table_rate_id = @p0
              AND is_deleted = false
              AND effect_date <= CURRENT_DATE
              AND (expire_date IS NULL OR expire_date >= CURRENT_DATE)");
        
        parameters.Add(new NpgsqlParameter($"@p{paramIndex++}", tableRateId));

        // Add coverage filter if provided
        if (coverageId.HasValue)
        {
            sqlBuilder.Append($" AND coverage_id = @p{paramIndex}");
            parameters.Add(new NpgsqlParameter($"@p{paramIndex++}", coverageId.Value));
        }

        // Build condition clauses based on the operators
        foreach (var (attributeCode, (op, value)) in conditions)
        {
            var conditionSql = BuildConditionSql(attributeCode, op, value, ref paramIndex, parameters);
            if (!string.IsNullOrEmpty(conditionSql))
            {
                sqlBuilder.Append($" AND {conditionSql}");
            }
        }

        // Khi có nhiều dòng match: chọn dòng có nhiều condition attribute hơn (condition là jsonb, đếm số key top-level)
        sqlBuilder.Append(" ORDER BY COALESCE((SELECT count(*) FROM jsonb_object_keys(condition) AS k), 0) DESC, id LIMIT 1");

        var sql = sqlBuilder.ToString();

        // Log SQL and parameters for maintenance and debugging
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug(
                "[FindMatchingRateLine] SQL: {Sql}",
                sql);
            foreach (var p in parameters)
            {
                _logger.LogDebug(
                    "[FindMatchingRateLine] Parameter {Name}: DbType={DbType}, Value={Value}",
                    p.ParameterName,
                    p.NpgsqlDbType,
                    p.Value ?? DBNull.Value);
            }
        }

        // Execute the raw SQL query
        var result = await dbContext.Set<ProTableRateLine>()
            .FromSqlRaw(sql, parameters.ToArray())
            .FirstOrDefaultAsync(cancellationToken);

        return result;
    }

    /// <summary>
    /// Builds the SQL condition clause for a single attribute based on its operator.
    /// </summary>
    private string BuildConditionSql(
        string attributeCode, 
        string op, 
        object value, 
        ref int paramIndex, 
        List<NpgsqlParameter> parameters)
    {
        // Normalize operator to uppercase
        var normalizedOp = op?.ToUpperInvariant() ?? "EQUAL";

        var innerSql = normalizedOp switch
        {
            "EQUAL" => BuildEqualCondition(attributeCode, value, ref paramIndex, parameters),
            "GREATER_OR_EQUAL" => BuildComparisonCondition(attributeCode, ">=", value, ref paramIndex, parameters),
            "LESS_OR_EQUAL" => BuildComparisonCondition(attributeCode, "<=", value, ref paramIndex, parameters),
            "CONTAINS" => BuildContainsCondition(attributeCode, value, ref paramIndex, parameters),
            "NOT_CONTAINS" => BuildNotContainsCondition(attributeCode, value, ref paramIndex, parameters),
            "IN_RANGE" => BuildInRangeCondition(attributeCode, value, ref paramIndex, parameters),
            "IN" => BuildInCondition(attributeCode, value, ref paramIndex, parameters),
            "NOT_IN" => BuildNotInCondition(attributeCode, value, ref paramIndex, parameters),
            _ => BuildEqualCondition(attributeCode, value, ref paramIndex, parameters) // Default to EQUAL
        };

        if (string.IsNullOrEmpty(innerSql))
        {
            return innerSql;
        }

        // If attribute is missing in condition JSONB, pass through (do not filter out)
        return $"((condition->'{EscapeJsonKey(attributeCode)}') IS NULL OR ({innerSql}))";
    }

    /// <summary>
    /// Builds EQUAL condition: attribute == value
    /// JSON condition format: {"attributeCode": value}
    /// </summary>
    private string BuildEqualCondition(string attributeCode, object value, ref int paramIndex, List<NpgsqlParameter> parameters)
    {
        var valueStr = ConvertToString(value);
        parameters.Add(new NpgsqlParameter($"@p{paramIndex}", valueStr));
        return $"(condition->>'{EscapeJsonKey(attributeCode)}') = @p{paramIndex++}";
    }

    /// <summary>
    /// Builds comparison condition (>=, <=) for numeric values.
    /// </summary>
    private string BuildComparisonCondition(string attributeCode, string comparisonOp, object value, ref int paramIndex, List<NpgsqlParameter> parameters)
    {
        var numericValue = ConvertToDecimal(value);
        parameters.Add(new NpgsqlParameter($"@p{paramIndex}", numericValue));
        return $"(condition->>'{EscapeJsonKey(attributeCode)}')::numeric {comparisonOp} @p{paramIndex++}";
    }

    /// <summary>
    /// Builds CONTAINS condition: attribute LIKE '%value%'
    /// The condition value in JSON is the pattern to search for in the input value.
    /// </summary>
    private string BuildContainsCondition(string attributeCode, object value, ref int paramIndex, List<NpgsqlParameter> parameters)
    {
        var valueStr = ConvertToString(value);
        parameters.Add(new NpgsqlParameter($"@p{paramIndex}", valueStr));
        // Input value should contain the condition pattern
        return $"@p{paramIndex++} LIKE '%' || (condition->>'{EscapeJsonKey(attributeCode)}') || '%'";
    }

    /// <summary>
    /// Builds NOT_CONTAINS condition: attribute NOT LIKE '%value%'
    /// </summary>
    private string BuildNotContainsCondition(string attributeCode, object value, ref int paramIndex, List<NpgsqlParameter> parameters)
    {
        var valueStr = ConvertToString(value);
        parameters.Add(new NpgsqlParameter($"@p{paramIndex}", valueStr));
        return $"@p{paramIndex++} NOT LIKE '%' || (condition->>'{EscapeJsonKey(attributeCode)}') || '%'";
    }

    /// <summary>
    /// Builds IN_RANGE condition: value >= min AND value <= max
    /// JSON condition format: {"attributeCode": [min, max]}
    /// </summary>
    private string BuildInRangeCondition(string attributeCode, object value, ref int paramIndex, List<NpgsqlParameter> parameters)
    {
        var numericValue = ConvertToDecimal(value);
        parameters.Add(new NpgsqlParameter($"@p{paramIndex}", numericValue));
        var paramName = $"@p{paramIndex++}";
        
        // The condition stores [min, max] array, check if input value is within range
        return $"({paramName} >= ((condition->'{EscapeJsonKey(attributeCode)}'->>0)::numeric) AND {paramName} <= ((condition->'{EscapeJsonKey(attributeCode)}'->>1)::numeric))";
    }

    /// <summary>
    /// Builds IN condition: attribute IN (values)
    /// JSON condition format: {"attributeCode": [value1, value2, ...]}
    /// </summary>
    private string BuildInCondition(string attributeCode, object value, ref int paramIndex, List<NpgsqlParameter> parameters)
    {
        var valueStr = ConvertToString(value);
        parameters.Add(new NpgsqlParameter($"@p{paramIndex}", valueStr));
        // Check if the input value exists in the JSON array
        return $"(condition->'{EscapeJsonKey(attributeCode)}') ? @p{paramIndex++}";
    }

    /// <summary>
    /// Builds NOT_IN condition: attribute NOT IN (values)
    /// JSON condition format: {"attributeCode": [value1, value2, ...]}
    /// </summary>
    private string BuildNotInCondition(string attributeCode, object value, ref int paramIndex, List<NpgsqlParameter> parameters)
    {
        var valueStr = ConvertToString(value);
        parameters.Add(new NpgsqlParameter($"@p{paramIndex}", valueStr));
        // Check if the input value does NOT exist in the JSON array
        return $"NOT ((condition->'{EscapeJsonKey(attributeCode)}') ? @p{paramIndex++})";
    }

    /// <summary>
    /// Escapes a JSON key to prevent SQL injection.
    /// </summary>
    private static string EscapeJsonKey(string key)
    {
        // Replace single quotes with escaped single quotes
        return key.Replace("'", "''");
    }

    /// <summary>
    /// Converts a value to string for SQL parameter.
    /// Uses invariant culture for numeric values so DB comparison matches (e.g. "3.3" not "3,3" in de-DE).
    /// </summary>
    private static string ConvertToString(object value)
    {
        if (value is JsonElement jsonElement)
        {
            return jsonElement.ValueKind switch
            {
                JsonValueKind.String => jsonElement.GetString() ?? string.Empty,
                JsonValueKind.Number => jsonElement.GetRawText(),
                _ => jsonElement.GetRawText()
            };
        }

        if (value is decimal d) return d.ToString(CultureInfo.InvariantCulture);
        if (value is double dbl) return dbl.ToString(CultureInfo.InvariantCulture);
        if (value is float f) return f.ToString(CultureInfo.InvariantCulture);
        if (value is int i) return i.ToString(CultureInfo.InvariantCulture);
        if (value is long l) return l.ToString(CultureInfo.InvariantCulture);
        
        return value?.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Converts a value to decimal for numeric comparisons.
    /// Uses invariant culture so that "." is always the decimal separator (avoids "3.3" parsing as 33 in vi-VN/de-DE).
    /// </summary>
    private static decimal ConvertToDecimal(object value)
    {
        if (value is JsonElement jsonElement)
        {
            if (jsonElement.ValueKind == JsonValueKind.Number)
            {
                return jsonElement.GetDecimal();
            }

            if (jsonElement.ValueKind == JsonValueKind.String)
            {
                var s = jsonElement.GetString();
                if (decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
                {
                    return parsed;
                }
            }
            else if (decimal.TryParse(jsonElement.GetRawText(), NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
            {
                return parsed;
            }
        }
        
        if (value is decimal d) return d;
        if (value is double dbl) return (decimal)dbl;
        if (value is float f) return (decimal)f;
        if (value is int i) return i;
        if (value is long l) return l;
        
        if (decimal.TryParse(value?.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }
        
        return 0;
    }
}
