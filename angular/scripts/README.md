# Proxy Generation Scripts

## generate-missing-dtos.js

Script tự động bổ sung các DTO còn thiếu sau khi chạy `abp generate-proxy`.

### Vấn đề

ABP Angular proxy generator đôi khi không generate đầy đủ các Output DTOs (DTOs được sử dụng trong return type của các methods), chỉ generate Input DTOs (DTOs được sử dụng trong parameters).

### Giải pháp

Script này sẽ:
1. Đọc `generate-proxy.json` để tìm tất cả DTOs được sử dụng
2. Kiểm tra các file `models.ts` để xem DTO nào còn thiếu
3. Tự động generate các DTO còn thiếu vào đúng file `models.ts` tương ứng

### Cách sử dụng

```bash
# Chạy sau khi generate proxy
npm run fix-proxy-dtos

# Hoặc chạy đầy đủ: generate proxy + fix DTOs
npm run generate-proxy:complete
```

### Scripts có sẵn

- `npm run generate-proxy` - Generate proxy cho module abp
- `npm run generate-proxy:audit-logging` - Generate proxy cho module audit-logging
- `npm run generate-proxy:complete` - Generate proxy cho tất cả modules và fix DTOs
- `npm run fix-proxy-dtos` - Chỉ fix các DTO còn thiếu
