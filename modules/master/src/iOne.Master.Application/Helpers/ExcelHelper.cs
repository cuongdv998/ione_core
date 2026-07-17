using ClosedXML.Excel;

namespace iOne.Master.Helpers;

/// <summary>
/// Helper methods for working with Excel files using ClosedXML
/// </summary>
public static class ExcelHelper
{
    /// <summary>
    /// Gets the cell value as a string, handling different data types appropriately.
    /// DateTime values are formatted as "yyyy-MM-dd", Number values are converted to string,
    /// and all other values are returned as strings.
    /// </summary>
    /// <param name="row">The Excel row</param>
    /// <param name="column">The column number (1-based)</param>
    /// <returns>The cell value as a string</returns>
    public static string GetCellValue(this IXLRow row, int column)
    {
        var cell = row.Cell(column);
        
        if (cell.DataType == XLDataType.DateTime)
        {
            return cell.GetDateTime().ToString("yyyy-MM-dd");
        }
        
        if (cell.DataType == XLDataType.Number)
        {
            return cell.GetDouble().ToString();
        }
        
        return cell.GetString();
    }
}

