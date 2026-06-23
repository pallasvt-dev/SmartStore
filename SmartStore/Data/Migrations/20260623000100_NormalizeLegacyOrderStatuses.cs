using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SmartStore.Data;

#nullable disable

namespace SmartStore.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260623000100_NormalizeLegacyOrderStatuses")]
    public partial class NormalizeLegacyOrderStatuses : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE [Orders]
                SET [OrderStatus] = CASE LOWER(LTRIM(RTRIM([OrderStatus])))
                    WHEN 'pending' THEN 'ChoXacNhan'
                    WHEN 'awaitingconfirmation' THEN 'ChoXacNhan'
                    WHEN 'confirmed' THEN 'DaXacNhan'
                    WHEN 'processing' THEN 'DaXacNhan'
                    WHEN 'shipping' THEN 'DangGiao'
                    WHEN 'shipped' THEN 'DangGiao'
                    WHEN 'intransit' THEN 'DangGiao'
                    WHEN 'completed' THEN 'HoanThanh'
                    WHEN 'delivered' THEN 'HoanThanh'
                    WHEN 'cancelled' THEN 'DaHuy'
                    WHEN 'canceled' THEN 'DaHuy'
                    ELSE [OrderStatus]
                END
                WHERE LOWER(LTRIM(RTRIM([OrderStatus]))) IN (
                    'pending',
                    'awaitingconfirmation',
                    'confirmed',
                    'processing',
                    'shipping',
                    'shipped',
                    'intransit',
                    'completed',
                    'delivered',
                    'cancelled',
                    'canceled'
                );
                """);

            migrationBuilder.Sql("""
                UPDATE [Orders]
                SET [PaymentMethod] = CASE LOWER(LTRIM(RTRIM([PaymentMethod])))
                    WHEN 'cash' THEN 'COD'
                    WHEN 'cod' THEN 'COD'
                    WHEN 'cashondelivery' THEN 'COD'
                    WHEN 'banktransfer' THEN 'ChuyenKhoan'
                    WHEN 'transfer' THEN 'ChuyenKhoan'
                    WHEN 'bank' THEN 'ChuyenKhoan'
                    ELSE [PaymentMethod]
                END
                WHERE LOWER(LTRIM(RTRIM([PaymentMethod]))) IN (
                    'cash',
                    'cod',
                    'cashondelivery',
                    'banktransfer',
                    'transfer',
                    'bank'
                );
                """);

            migrationBuilder.Sql("""
                UPDATE [Orders]
                SET [PaymentStatus] = CASE LOWER(LTRIM(RTRIM([PaymentStatus])))
                    WHEN 'unpaid' THEN 'ChuaThanhToan'
                    WHEN 'pending' THEN 'ChuaThanhToan'
                    WHEN 'notpaid' THEN 'ChuaThanhToan'
                    WHEN 'paid' THEN 'DaThanhToan'
                    WHEN 'completed' THEN 'DaThanhToan'
                    WHEN 'refunded' THEN 'DaHoanTien'
                    WHEN 'refund' THEN 'DaHoanTien'
                    ELSE [PaymentStatus]
                END
                WHERE LOWER(LTRIM(RTRIM([PaymentStatus]))) IN (
                    'unpaid',
                    'pending',
                    'notpaid',
                    'paid',
                    'completed',
                    'refunded',
                    'refund'
                );
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
