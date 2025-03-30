namespace iFood.Data.Enum
{
    public enum Status
    {
        PendingConfirmation,  // Chờ xác nhận
        Shipping,             // Đang vận chuyển
        Delivered,            // Đã giao hàng
        Completed,            // Đã hoàn thành
        Canceled,             // Đã hủy
        ReturnedOrRefunded    // Hoàn trả/Hoàn tiền
    }
}