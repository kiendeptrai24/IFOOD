namespace iFood.Data.Enum
{
    public enum ProductStatus
    {
        Unknown = 0,
        Bestseller = 1,      // Sản phẩm bán chạy
        NewArrival = 2,      // Hàng mới về
        Featured = 3,        // Sản phẩm nổi bật
        OutOfStock = 4,      // Hết hàng
        Discounted = 5,      // Đang giảm giá
        LimitedEdition = 6,  // Phiên bản giới hạn
        ComingSoon = 7       // Sắp ra mắt
    }
}