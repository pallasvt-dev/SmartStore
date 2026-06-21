namespace SmartStore.Models
{
    public enum OrderStatus
    {
        ChoXacNhan,
        DaXacNhan,
        DangGiao,
        HoanThanh,
        DaHuy
    }

    public enum PaymentStatus
    {
        ChuaThanhToan,
        DaThanhToan,
        DaHoanTien
    }

    public enum PaymentMethod
    {
        COD,
        ChuyenKhoan
    }
}
