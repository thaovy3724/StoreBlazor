using StoreBlazor.Models;

namespace StoreBlazor.DTO.Admin
{
    /// <summary>
    /// DTO cho khuyến mãi trong OrderStaff
    /// </summary>
    public class PromotionDto
    {
        public int PromoId { get; set; }

        public string PromoCode { get; set; } = string.Empty;

        public string? Description { get; set; }

        /// <summary>
        /// Loại giảm giá: Percent (%) hoặc Fixed (số tiền cố định)
        /// </summary>
        public DiscountType DiscountType { get; set; }

        /// <summary>
        /// Giá trị giảm: % hoặc số tiền
        /// </summary>
        public decimal DiscountValue { get; set; }

        /// <summary>
        /// Đơn hàng tối thiểu để áp dụng
        /// </summary>
        public decimal MinOrderAmount { get; set; }

        /// <summary>
        /// Ngày hết hạn
        /// </summary>
        public DateTime EndDate { get; set; }

        // ===== HELPER METHODS =====

        /// <summary>
        /// Kiểm tra đơn hàng có đủ điều kiện áp dụng không
        /// </summary>
        public bool IsApplicable(decimal totalAmount)
        {
            return totalAmount >= MinOrderAmount;
        }

        /// <summary>
        /// Tính số tiền được giảm
        /// </summary>
        public decimal CalculateDiscount(decimal totalAmount)
        {
            if (!IsApplicable(totalAmount))
                return 0;

            if (DiscountType == DiscountType.Percent)
            {
                // Giảm theo %
                return totalAmount * (DiscountValue / 100);
            }
            else
            {
                // Giảm cố định
                return DiscountValue;
            }
        }

        /// <summary>
        /// Hiển thị giá trị giảm dạng text (VD: "20%" hoặc "50,000đ")
        /// </summary>
        public string GetDiscountDisplayText()
        {
            if (DiscountType == DiscountType.Percent)
            {
                return $"{DiscountValue}%";
            }
            else
            {
                return $"{DiscountValue:N0}đ";
            }
        }
    }
}