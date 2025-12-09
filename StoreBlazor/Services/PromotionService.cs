using Microsoft.EntityFrameworkCore;
using StoreBlazor.Data;
using StoreBlazor.DTO.Admin;
using StoreBlazor.Models;

namespace StoreBlazor.Services
{
    public class PromotionService : BasePaginationService, IPromotionService
    {
        public PromotionService(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<PageResult<Promotion>> GetAllPromotionAsync(int page)
        {
            var query = _dbContext.Promotions
                .OrderByDescending(p => p.PromoId)
                .AsQueryable(); 

            return await GetPagedAsync(query, page); ;
        }
        public Task<PageResult<Promotion>> FilterAsync(string keyword, int status, DateTime? fromDate, DateTime? toDate, int page)
        {
            var query = _dbContext.Promotions
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(p => p.PromoCode.Contains(keyword));
            }

            if (status != -1)
            {
                query = query.Where(p => (int)p.Status == status);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(p => p.StartDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(p => p.EndDate <= toDate.Value);
            }

            query = query.OrderByDescending(p => p.PromoId);

            return GetPagedAsync<Promotion>(query, page);
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
            // ìm Promotion cần cập nhật
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

            // Kiểm tra trùng mã (trừ chính nó ra)
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

            // Cập nhật tất cả các field
            entity.PromoCode = newPromoCode;
            entity.DiscountType = item.DiscountType;
            entity.DiscountValue = item.DiscountValue;
            entity.MinOrderAmount = item.MinOrderAmount;
            entity.UsageLimit = item.UsageLimit;
            entity.StartDate = item.StartDate;
            entity.EndDate = item.EndDate;
            entity.Description = item.Description;

            // Lưu thay đổi
            await _dbContext.SaveChangesAsync();

            return new ServiceResult
            {
                Type = "success",
                Message = "Cập nhật khuyến mãi thành công!"
            };
        }

        public async Task<ServiceResult> LockAsync(Promotion item)
        {
            // Tìm Promotion cần khóa/mở khóa
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

            // Toggle trạng thái
            entity.Status = entity.Status == PromotionStatus.Active
                            ? PromotionStatus.Inactive
                            : PromotionStatus.Active;

            // Lưu vào database
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
