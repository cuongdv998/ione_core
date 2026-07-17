/**
 * Chuẩn hóa biển số xe: chỉ giữ số và chữ in hoa, loại bỏ ký tự đặc biệt (-, ., v.v.)
 * Chữ thường được chuyển thành in hoa.
 */
export function normalizeVehiclePlate(value: string | null | undefined): string {
  if (value == null) return '';
  return value
    .toUpperCase()
    .replace(/[^A-Z0-9]/g, '');
}
