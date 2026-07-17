export interface TableColumn {
  field: string;                 // field trong DTO
  header: string;                // label hiển thị
  sortable?: boolean;            // co sap xep khong
  type?: 'text' | 'date' | 'number' | 'boolean';
  /** Date format for type 'date' (e.g. 'dd/MM/yyyy' or 'dd/MM/yyyy HH:mm'). Default 'dd/MM/yyyy'. */
  dateFormat?: string;
  width?: string;                // vi du: '150px'
  freeze?: 'left' | 'right'; //Co dinh mot column
  align?: 'left' | 'center' | 'right';
  formatter?: (value: any, row?: any) => string; // Custom formatter function
  cellClass?: (value: any, row?: any) => string; // Custom cell class function
  wrap?: boolean; // allow wrapping long text in this column
  /** When set, cell is rendered as a link opening in new tab (href from this function). */
  linkUrl?: (row: any) => string | null;
  /** Optional CSS classes for the link when linkUrl is set (e.g. 'text-black font-semibold'). */
  linkClass?: string;
}