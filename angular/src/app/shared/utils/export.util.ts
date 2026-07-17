import * as XLSX from 'xlsx-js-style';
import { saveAs } from 'file-saver';

export interface ExportColumn {
  field: string;
  header: string;
  formatter?: (value: any, row: any) => string | number;
}

/**
 * Export data to Excel file
 * @param data Array of data objects to export
 * @param columns Column definitions with headers and optional formatters
 * @param filename Name of the file (without extension)
 */
export function exportToExcel(
  data: any[],
  columns: ExportColumn[],
  filename: string
): void {
  if (!data || data.length === 0) {
    throw new Error('No data to export');
  }

  // Prepare worksheet data
  const worksheetData: any[][] = [];

  // Add header row
  const headers = columns.map(col => col.header);
  worksheetData.push(headers);

  // Add data rows
  data.forEach(row => {
    const rowData = columns.map(col => {
      const value = row[col.field];
      
      // Use formatter if available
      if (col.formatter) {
        return col.formatter(value, row);
      }
      
      // Handle null/undefined
      if (value === null || value === undefined) {
        return '';
      }
      
      // Handle dates
      if (value instanceof Date) {
        return value.toLocaleDateString('vi-VN');
      }
      
      // Return as-is for other types
      return value;
    });
    worksheetData.push(rowData);
  });

  // Create workbook and worksheet
  const workbook = XLSX.utils.book_new();
  const worksheet = XLSX.utils.aoa_to_sheet(worksheetData);

  // Get worksheet range
  const range = XLSX.utils.decode_range(worksheet['!ref'] || 'A1');
  const numRows = range.e.r + 1;
  const numCols = range.e.c + 1;

  // Apply formatting to all cells
  for (let row = 0; row < numRows; row++) {
    for (let col = 0; col < numCols; col++) {
      const cellAddress = XLSX.utils.encode_cell({ r: row, c: col });
      if (!worksheet[cellAddress]) continue;

      const isHeader = row === 0;
      
      // Header row formatting - professional blue theme with white text
      if (isHeader) {
        worksheet[cellAddress].s = {
          font: { 
            bold: true, 
            sz: 11,
            color: { rgb: 'FFFFFFFF' } // White text (ARGB format)
          },
          fill: { 
            fgColor: { rgb: 'FF4472C4' }, // Professional blue (ARGB format)
            patternType: 'solid'
          },
          alignment: { 
            horizontal: 'center', 
            vertical: 'center',
            wrapText: true
          },
          border: {
            top: { style: 'thin', color: { rgb: 'FF000000' } },
            bottom: { style: 'thin', color: { rgb: 'FF000000' } },
            left: { style: 'thin', color: { rgb: 'FF000000' } },
            right: { style: 'thin', color: { rgb: 'FF000000' } }
          }
        };
      } else {
        // Data row formatting - alternating row colors for better readability
        const isEvenRow = row % 2 === 0;
        worksheet[cellAddress].s = {
          font: { 
            sz: 10,
            color: { rgb: 'FF000000' } // Black text
          },
          fill: { 
            fgColor: { rgb: isEvenRow ? 'FFF8F9FA' : 'FFFFFFFF' }, // Very light gray for even rows, white for odd
            patternType: 'solid'
          },
          alignment: { 
            horizontal: 'left', 
            vertical: 'center',
            wrapText: true
          },
          border: {
            top: { style: 'thin', color: { rgb: 'FFE0E0E0' } },
            bottom: { style: 'thin', color: { rgb: 'FFE0E0E0' } },
            left: { style: 'thin', color: { rgb: 'FFE0E0E0' } },
            right: { style: 'thin', color: { rgb: 'FFE0E0E0' } }
          }
        };
      }
    }
  }

  // Set row height for header row (better visibility)
  if (!worksheet['!rows']) worksheet['!rows'] = [];
  worksheet['!rows'][0] = { hpt: 30 }; // Header row height (in points)

  // Auto-width columns with improved calculation
  const colWidths = columns.map((col) => {
    let maxLength = col.header.length;
    
    // Check all data values for this column
    data.forEach(row => {
      const value = col.formatter 
        ? String(col.formatter(row[col.field], row))
        : String(row[col.field] || '');
      const valueLength = value.length;
      if (valueLength > maxLength) {
        maxLength = valueLength;
      }
    });
    
    // Calculate width: add padding, enforce min/max bounds
    // Min 12 chars, max 60 chars, add 4 for padding and borders
    const calculatedWidth = Math.min(Math.max(maxLength + 4, 12), 60);
    return { wch: calculatedWidth };
  });
  worksheet['!cols'] = colWidths;

  // Freeze header row for better navigation
  // Note: Standard xlsx library has limited freeze pane support
  // This sets the view to freeze the first row
  if (!worksheet['!freeze']) {
    worksheet['!freeze'] = {};
  }
  worksheet['!freeze'] = { 
    xSplit: 0, 
    ySplit: 1, 
    topLeftCell: 'A2', 
    activePane: 'bottomLeft', 
    state: 'frozen' 
  };

  // Add worksheet to workbook
  XLSX.utils.book_append_sheet(workbook, worksheet, 'Sheet1');

  // Generate Excel file with styles
  const excelBuffer = XLSX.write(workbook, { 
    bookType: 'xlsx', 
    type: 'array',
    cellStyles: true // Preserve cell styles
  });
  const blob = new Blob([excelBuffer], { 
    type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' 
  });

  // Generate filename with date
  const dateStr = new Date().toISOString().slice(0, 10).replace(/-/g, '');
  const fullFilename = `${filename}_${dateStr}.xlsx`;

  // Download file
  saveAs(blob, fullFilename);
}
