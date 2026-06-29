export function translateStatus(status: string) {
  switch (status) {
    case "PendingConfirmation":
      return "Chờ xác nhận";
    case "Pending":
      return "Chờ xác nhận";
    case "Accepted":
      return "Đã xác nhận";
    case "Confirmed":
      return "Đã nhận";
    case "Cancelled":
      return "Đã hủy";
    default:
      return status;
  }
}
