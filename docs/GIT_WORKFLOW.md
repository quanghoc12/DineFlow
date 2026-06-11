# Git Workflow

## Branch chính

```text
main
  Code ổn định để demo/release.

develop
  Code tích hợp của nhóm.

feature/<feature-name>
  Branch riêng của từng member.
```

## Ví dụ branch

```text
feature/auth-login
feature/table-session
feature/menu-stock
feature/order-print
feature/request
feature/bill-payment
feature/dashboard
feature/customer-web
feature/api-contract
```

## Luồng làm việc

```bash
git checkout develop
git pull origin develop
git checkout -b feature/menu-stock

# code

git add .
git commit -m "feat: add menu stock base"
git push origin feature/menu-stock
```

Sau đó tạo Pull Request vào `develop`.

## Commit message

```text
feat: thêm chức năng mới
fix: sửa lỗi
refactor: cải thiện code không đổi logic
ui: chỉnh giao diện
docs: cập nhật tài liệu
test: thêm/sửa test
chore: cấu hình project
```
