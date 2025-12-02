using Microsoft.EntityFrameworkCore;
using StoreBlazor.Data;
using StoreBlazor.DTO.Admin;
using StoreBlazor.Models;
using StoreBlazor.Services.Interfaces;

namespace StoreBlazor.Services.Implementations
{
    public class PromotionService : IPromotionService
    {
        private readonly ApplicationDbContext _dbContext;
        public PromotionService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Promotion>> GetAllPromotionAsync()
        {
            // Hàm chứa await → bắt buộc phải trả về Task<...>
            // có await thì method phải là async
            // Defer execution: chưa query dữ liệu ngay mà phải đợi đến khi nào có await thì mới query dữ liệu
            var list = await _dbContext.Promotions
                            .OrderBy(p => p.PromoId)
                            .ToListAsync(); // dòng này mới query dữ liệu

            return list;
        }

        public async Task<ServiceResult> CreateAsync(Promotion item)
        {
            // Kiểm tra trùng mã khuyến mãi (PromoCode)
            var existingPromoCode = await _dbContext.Promotions
                .FirstOrDefaultAsync(p => p.PromoCode.ToLower() == item.PromoCode.ToLower());

            if (existingPromoCode != null)
            {
                return new ServiceResult
                {
                    Type = "error",
                    Message = "Mã khuyến mãi đã tồn tại!"
                };
            }

            _dbContext.Promotions.Add(item);
            await _dbContext.SaveChangesAsync();

            return new ServiceResult
            {
                Type = "success",
                Message = "Thêm khuyến mãi thành công!"
            };
        }
        public async Task<ServiceResult> UpdateAsync(Promotion item)
        {
            // 1. Tìm Promotion cần cập nhật
            var entity = await _dbContext.Promotions
                .FirstOrDefaultAsync(p => p.PromoId == item.PromoId);

            if (entity == null)
            {
                return new ServiceResult
                {
                    Type = "error",
                    Message = "Khuyến mãi không tồn tại!"
                };
            }

            // 2. Kiểm tra trùng mã (trừ chính nó ra)
            var newPromoCode = item.PromoCode.Trim();
            var existingPromotion = await _dbContext.Promotions
                .FirstOrDefaultAsync(p =>
                    p.PromoId != item.PromoId &&
                    p.PromoCode.ToLower() == newPromoCode.ToLower());

            if (existingPromotion != null)
            {
                return new ServiceResult
                {
                    Type = "error",
                    Message = "Mã khuyến mãi đã tồn tại!"
                };
            }

            // 3. Cập nhật tất cả các field
            entity.PromoCode = newPromoCode;
            entity.DiscountType = item.DiscountType;
            entity.DiscountValue = item.DiscountValue;
            entity.MinOrderAmount = item.MinOrderAmount;
            entity.UsageLimit = item.UsageLimit;
            entity.StartDate = item.StartDate;
            entity.EndDate = item.EndDate;
            entity.Description = item.Description;

            // 4. Lưu thay đổi
            await _dbContext.SaveChangesAsync();

            return new ServiceResult
            {
                Type = "success",
                Message = "Cập nhật khuyến mãi thành công!"
            };
        }

        public async Task<ServiceResult> LockAsync(Promotion item)
        {
            // 1. Tìm Promotion cần khóa/mở khóa
            var entity = await _dbContext.Promotions
                .FirstOrDefaultAsync(p => p.PromoId == item.PromoId);

            if (entity == null)
            {
                return new ServiceResult
                {
                    Type = "error",
                    Message = "Khuyến mãi không tồn tại!"
                };
            }

            // 2. Toggle trạng thái
            entity.Status = entity.Status == PromotionStatus.Active
                            ? PromotionStatus.Inactive
                            : PromotionStatus.Active;

            // 3. Lưu vào database
            await _dbContext.SaveChangesAsync();

            return new ServiceResult
            {
                Type = "success",
                Message = entity.Status == PromotionStatus.Active
                          ? "Khuyến mãi đã được kích hoạt!"
                          : "Khuyến mãi đã bị khóa!"
            };
        }

       


    }

}
