# Feature Checklist

Mỗi feature chỉ được xem là xong khi có đủ:

```text
Database / Migration:
[ ] Entity đã đúng field.
[ ] FK đã khai báo bằng navigation property.
[ ] AppDbContext đã cấu hình nếu cần.
[ ] DB owner đã tạo migration nếu schema đổi.
[ ] Có seed data nếu cần.

BusinessObjects:
[ ] Có Entity.
[ ] Có DTO nếu cần.
[ ] Có Enum nếu cần.

DataAccessObjects:
[ ] Có DAO.
[ ] Có GetAll/GetById/Search/Add/Update/Delete nếu là CRUD.
[ ] DAO không xử lý UI.
[ ] DAO không chứa business logic dài.

Repositories:
[ ] Có Interface.
[ ] Có Implementation.
[ ] Repository gọi DAO.

Services:
[ ] Có Interface.
[ ] Có Implementation.
[ ] Có validate.
[ ] Có business rule.
[ ] Service không MessageBox.

WPF/API/Web:
[ ] WPF chỉ gọi Service.
[ ] API chỉ gọi Service.
[ ] CustomerWeb chỉ gọi API.
[ ] Có try-catch/hiển thị lỗi phù hợp.

Git:
[ ] Build không lỗi.
[ ] Không sửa file chung nếu chưa thống nhất.
[ ] Không commit secret.
[ ] Pull Request vào develop.
```
