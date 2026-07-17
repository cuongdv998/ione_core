# Plan Backend: Danh mục tham chiếu với bảo hiểm gốc (Insurer Dictionary)

**Module:** Master  
**Menu:** Danh mục → Danh mục Đối tượng bảo hiểm → Danh mục Mapping với bảo hiểm gốc  
**Tài liệu tham chiếu:** `dev_note/RULES_BACKEND_DEVELOPMENT.md`  
**Mục đích:** Document plan để review trước khi thực hiện.

---

## 1. Tổng quan chức năng

| Tính năng     | Mô tả |
|---------------|--------|
| **Tìm kiếm**  | Danh sách có phân trang, lọc theo BusinessName, OwnCode, InsurerCode, Status (và InsurerId nếu bổ sung). |
| **Thêm mới**  | Tạo bản ghi mới với validation overlap (BusinessName, InsurerId, OwnCode, EffectDate, ExpireDate). |
| **Sửa**       | Chỉ cho phép sửa: InsurerCode, ExtraData, Status, EffectDate, ExpireDate. **Không** cho phép sửa: BusinessName, InsurerId, OwnCode. |
| **Xóa**       | Soft delete trước (để ABP ghi audit log Deleted), sau đó cập nhật Status = Deactive. |

---

## 2. Ràng buộc nghiệp vụ

### 2.1. Khi sửa (Update)

- **Không được phép sửa:** `BusinessName`, `InsurerId`, `OwnCode`.
- **Được phép sửa:** `InsurerCode`, `ExtraData`, `Status`, `EffectDate`, `ExpireDate`.
- Cần đảm bảo **không overlap** (xem 2.3) với các bản ghi khác sau khi đổi EffectDate/ExpireDate.

### 2.2. Khi xóa (Delete)

- Thứ tự bắt buộc (để đảm bảo audit log ABP):
  1. **Bước 1:** Gọi soft delete (ví dụ `Repository.DeleteAsync(entity)`) → ABP ghi audit với `ChangeType = Deleted`.
  2. **Bước 2:** Sau khi save, cập nhật entity: `Status = Deactive` và `UpdateAsync` (entity vẫn còn trong DB do soft delete).
- **Lưu ý:** Hiện tại code đã implement đúng thứ tự này trong `InsurerDictionaryAppService.DeleteAsync`.

### 2.3. Không overlap (BusinessName, InsurerId, OwnCode, EffectDate, ExpireDate)

- Với cùng bộ **(BusinessName, InsurerId, OwnCode)**, khoảng **[EffectDate, ExpireDate]** không được giao với bất kỳ bản ghi nào khác (kể cả bản ghi đã soft delete hay status Deactive — tùy nghiệp vụ quyết định).
- **Định nghĩa overlap:** Hai khoảng `[A1, A2]` và `[B1, B2]` giao nhau khi `A1 <= B2` và `B1 <= A2`.
- **Cách implement đề xuất:**
  - **Create:** Trước khi Insert, kiểm tra không tồn tại bản ghi khác (cùng BusinessName, InsurerId, OwnCode) có khoảng [EffectDate, ExpireDate] giao với input.
  - **Update:** Khi đổi EffectDate/ExpireDate, kiểm tra tương tự, loại trừ chính bản ghi đang sửa (theo Id).
  - **Unique constraint DB (tùy chọn):** PostgreSQL không có constraint “no overlapping ranges” sẵn; có thể dùng CHECK + EXCLUDE USING gist (range type) hoặc chỉ validate ở Application/Domain. Đề xuất: **validation ở Application/Domain** là đủ, tránh phức tạp migration.

---

## 3. Cấu trúc bảng (PostgreSQL style)

Theo `RULES_BACKEND_DEVELOPMENT.md`, **table và column dùng snake_case** cho PostgreSQL.

| DB (snake_case)   | Kiểu / ràng buộc | Ghi chú |
|-------------------|-------------------|--------|
| **Table**         | `insurer_dictionary` | Nếu hiện tại đang là `insurerdictionary`, cân nhắc migration đổi tên cho đúng convention. |
| **id**            | uuid, PK           | |
| **business_name** | varchar(50), NOT NULL | Tên bảng dữ liệu cần mapping. |
| **insurer_id**    | uuid, NOT NULL    | Công ty BH (res_partner, partner_type = INSURER). **Hiện entity chưa có — cần bổ sung.** |
| **own_code**      | varchar(50), NOT NULL | Mã hệ thống. |
| **insurer_code**  | varchar(50), NOT NULL | Mã bảo hiểm. |
| **extra_data**    | text, NULL         | JSON key-value. |
| **status**        | varchar(15), NOT NULL | `active` / `deactive`. |
| **effect_date**   | date, NOT NULL     | Ngày hiệu lực. |
| **expire_date**   | date, NOT NULL     | Ngày hết hạn. |
| **creation_time** | timestamp, NOT NULL | Audit. |
| **creator_id**    | uuid, NULL         | Audit. |
| **last_modification_time** | timestamp, NULL | Audit. |
| **last_modifier_id** | uuid, NULL      | Audit. |
| **is_deleted**    | boolean            | Soft delete. |
| **deletion_time** | timestamp, NULL    | Audit. |
| **deleter_id**    | uuid, NULL         | Audit. |
| **concurrency_stamp** | varchar(40)   | ABP. |

- **Comment table:** `Bảng mapping các dữ liệu master data với công ty bảo hiểm gốc`.
- **Comment cột:** Giữ theo mô tả trong DDL bạn cung cấp (đã nêu trong yêu cầu).

---

## 4. Các thay đổi / bổ sung theo từng layer

### 4.1. Domain.Shared

- **File:** `InsurerDictionaryStatus.cs`  
- **Việc:** Giữ nguyên enum (Active, Deactive). Không đổi.

### 4.2. Domain (Entity, Manager, Repository)

| Hạng mục | Nội dung |
|----------|----------|
| **Entity `InsurerDictionary`** | 1) Thêm property `InsurerId` (Guid, private set). 2) Constructor + factory/setter cần truyền InsurerId. 3) **Không** thêm method public Update cho BusinessName, InsurerId, OwnCode (hoặc không gọi chúng từ AppService khi Update). 4) Table attribute: `[Table("insurer_dictionary")]` nếu đổi tên bảng; column có thể dùng `[Column("snake_case")]` hoặc cấu hình trong EF. |
| **InsurerDictionaryManager** | 1) Thêm method (hoặc tham số) kiểm tra overlap: ví dụ `HasOverlapAsync(businessName, insurerId, ownCode, effectDate, expireDate, excludeId?)`. 2) `CreateAsync`: gọi check overlap trước khi Insert (trong AppService hoặc trong Manager). 3) `UpdateAsync`: chỉ nhận và set các field được phép sửa (InsurerCode, ExtraData, Status, EffectDate, ExpireDate); **không** nhận BusinessName, InsurerId, OwnCode. 4) Khi Update, nếu đổi EffectDate/ExpireDate thì gọi HasOverlapAsync với excludeId = entity.Id. |
| **IInsurerDictionaryRepository** | Thêm method cần cho overlap nếu có (ví dụ `GetExistingRangesAsync(...)`) hoặc dùng query trong Manager. |
| **InsurerDictionaryManager.UpdateAsync** | Signature đổi thành không còn businessName, insurerId, ownCode; chỉ còn insurerCode, extraData, status, effectDate, expireDate. |

### 4.3. Application.Contracts (DTOs, Input, Interface)

| Hạng mục | Nội dung |
|----------|----------|
| **CreateInsurerDictionaryDto** | Thêm `InsurerId` (Guid, required). Giữ BusinessName, OwnCode, InsurerCode, ExtraData, Status, EffectDate, ExpireDate. |
| **UpdateInsurerDictionaryDto** | **Xóa** BusinessName, InsurerId, OwnCode. Chỉ giữ: InsurerCode, ExtraData, Status, EffectDate, ExpireDate. |
| **InsurerDictionaryDto** | Thêm InsurerId (để hiển thị). Giữ các field hiện có. |
| **GetInsurerDictionariesInput** | Có thể thêm filter `InsurerId?` (Guid?) nếu cần tìm theo công ty BH. |
| **IInsurerDictionaryAppService** | Không đổi signature; implementation sẽ map UpdateDto chỉ các field được phép sửa. |

### 4.4. Application (AppService)

| Hạng mục | Nội dung |
|----------|----------|
| **CreateAsync** | 1) Validate overlap (BusinessName, InsurerId, OwnCode, EffectDate, ExpireDate) qua Manager. 2) Tạo entity có InsurerId. 3) Gọi Manager.CreateAsync. |
| **UpdateAsync** | 1) Load entity. 2) **Không** map BusinessName, InsurerId, OwnCode từ input (dùng giá trị từ entity). 3) Nếu input đổi EffectDate/ExpireDate thì gọi check overlap (excludeId = entity.Id). 4) Gọi Manager.UpdateAsync chỉ với InsurerCode, ExtraData, Status, EffectDate, ExpireDate. |
| **DeleteAsync** | Giữ logic hiện tại: Delete (soft) trước → SaveChanges → Update Status = Deactive → UpdateAsync → SaveChanges. |
| **CreateFilteredQueryAsync** | Thêm filter theo InsurerId nếu GetInsurerDictionariesInput có InsurerId. |

### 4.5. EntityFrameworkCore (Configuration, Repository, Migration)

| Hạng mục | Nội dung |
|----------|----------|
| **InsurerDictionaryConfiguration** | 1) Table: `insurer_dictionary` (nếu đổi tên). 2) Column snake_case: `business_name`, `insurer_id`, `own_code`, `insurer_code`, `extra_data`, `status`, `effect_date`, `expire_date`, và các audit columns (creation_time, creator_id, ...). 3) Index phục vụ overlap: composite index (business_name, insurer_id, own_code, effect_date, expire_date) hoặc ít nhất (business_name, insurer_id, own_code) để tối ưu query kiểm tra overlap. |
| **EfCoreInsurerDictionaryRepository** | Chỉ cần thêm method custom nếu Manager cần (ví dụ query theo range). Có thể để logic overlap trong Manager dùng IQueryable từ DbSet. |
| **Migration** | 1) Nếu đổi tên bảng: `DROP TABLE IF EXISTS "insurerdictionary"`; tạo bảng mới `insurer_dictionary` với đủ cột (bao gồm insurer_id). 2) Nếu giữ tên bảng: migration thêm cột `insurer_id` (uuid, NOT NULL) — cần default hoặc backfill dữ liệu cũ trước khi NOT NULL. 3) Dùng IF EXISTS / IF NOT EXISTS theo RULES. 4) Comment table/column theo mô tả nghiệp vụ. |

### 4.6. Permissions, Localization, HttpApi, Menu

- **Permissions:** Đã có InsurerDictionaryPermissions (View, Create, Edit, Delete). Không đổi.
- **Localization:** Bổ sung key cho InsurerId (Display, Validation) trong vi-VN.json và en.json.
- **Controller:** Đã có InsurerDictionaryController; không cần đổi route/action.
- **Menu:** Đã nằm dưới Danh mục Đối tượng bảo hiểm. Không đổi.

---

## 5. Chi tiết Overlap (logic mẫu)

- **Điều kiện overlap:** Tồn tại bản ghi khác (cùng BusinessName, InsurerId, OwnCode) sao cho:
  - `existing.EffectDate <= input.ExpireDate` **và** `input.EffectDate <= existing.ExpireDate`
- **Create:** Kiểm tra không có bản ghi nào (kể cả IsDeleted = true nếu nghiệp vụ yêu cầu) thỏa điều kiện trên.
- **Update:** Giống trên, nhưng loại trừ `existing.Id != entity.Id`.
- Nếu có overlap → throw `UserFriendlyException` với message localization (ví dụ `InsurerDictionary:OverlapEffectExpire`).

---

## 6. Checklist triển khai (theo RULES)

- [ ] Entity: thêm InsurerId; không cho phép update BusinessName, InsurerId, OwnCode từ bên ngoài.
- [ ] UpdateDto: chỉ còn InsurerCode, ExtraData, Status, EffectDate, ExpireDate.
- [ ] UpdateAsync (Manager + AppService): không truyền/không set BusinessName, InsurerId, OwnCode.
- [ ] Delete: thứ tự Delete (soft) → SaveChanges → Update status Deactive → SaveChanges; đã đúng, chỉ cần giữ.
- [ ] Overlap: kiểm tra khi Create và khi Update (khi đổi EffectDate/ExpireDate).
- [ ] Table/column: snake_case (insurer_dictionary, business_name, insurer_id, ...); migration an toàn (IF EXISTS / IF NOT EXISTS).
- [ ] AutoMapper: map UpdateDto → Entity chỉ các field được phép (có thể dùng ForMember ignore cho BusinessName, InsurerId, OwnCode nếu vẫn có trong DTO vì lý do khác).
- [ ] Localization: InsurerId, message overlap (vi + en).
- [ ] Build + test API (GetList, Get, Create, Update, Delete) và test overlap (tạo hai bản ghi trùng khoảng → bản thứ hai hoặc update bị từ chối).

---

## 7. Tóm tắt rủi ro / lưu ý

1. **InsurerId:** Hiện entity chưa có; bảng hiện tại có thể chưa có cột. Migration cần xử lý dữ liệu cũ (default hoặc backfill) trước khi đặt NOT NULL.
2. **Đổi tên bảng/column:** Nếu chuyển từ `insurerdictionary` → `insurer_dictionary` và các cột sang snake_case đầy đủ, cần migration đổi tên hoặc tạo bảng mới và migrate dữ liệu; cần thống nhất với team.
3. **Overlap với bản ghi đã xóa:** Cần quyết định: có cấm overlap với cả bản ghi IsDeleted = true hay chỉ bản ghi chưa xóa. Đề xuất: kiểm tra trên toàn bộ (kể cả deleted) để tránh trùng lịch sử.

---

*Document này dùng để review trước khi implement. Sau khi review xong, có thể bắt đầu triển khai theo từng mục trong checklist.*
