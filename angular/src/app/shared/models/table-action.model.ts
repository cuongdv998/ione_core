export interface TableAction<T = any> {
  label?: string;
  icon?: string;

  command?: (row: T) => void;
  separator?: boolean;

  visible?: (row: T) => boolean;
  disabled?: (row: T) => boolean;
}
