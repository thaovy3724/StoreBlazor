using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using StoreBlazor.Data;
using StoreBlazor.DTO.Admin;
using StoreBlazor.DTO.Admin.Product;
using StoreBlazor.Models;
using StoreBlazor.Services.Admin.Interfaces;

namespace StoreBlazor.Services.Admin.Implementations
{
    public class ProductManagerService : BasePaginationService, IProductManagerService
    {
        public ProductManagerService(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<ServiceResult> CreateAsync(ProductFormDto model)
        {
            // Kiểm tra ImageFile 
            if (model.ImageFile == null)
            {
                return new ServiceResult
                {
                    Type = "error",
                    Message = "Vui lòng tải lên hình ảnh sản phẩm!"
                };
            }

            // Kiểm tra mã vạch trùng
            var existingProduct = await _dbContext.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Barcode == model.Barcode);

            if (existingProduct != null)
            {
                return new ServiceResult
                {
                    Type = "error",
                    Message = "Sản phẩm với mã vạch này đã tồn tại!"
                };
            }

            // Upload ảnh sản phẩm
            var uploadedFileName = await UploadImageAsync(model.ImageFile);

            if (uploadedFileName == null)
            {
                return new ServiceResult
                {
                    Type = "error",
                    Message = "Lỗi khi tải lên hình ảnh sản phẩm!"
                };
            }

            // Tạo tồn kho
            var inventory = new Inventory
            {
                Quantity = model.Inventory?.Quantity ?? 0,
                UpdatedAt = DateTime.Now
            };

            // Tạo product mới
            var newProduct = new Product
            {
                ProductName = model.ProductName,
                ProductImage = uploadedFileName, 
                CategoryId = model.CategoryId,
                SupplierId = model.SupplierId,
                Price = model.Price,
                Unit = model.Unit,
                Barcode = model.Barcode,
                CreatedAt = DateTime.Now,
                Inventory = inventory
            };

            // Lưu vào DB
            _dbContext.Products.Add(newProduct);
            await _dbContext.SaveChangesAsync();

            return new ServiceResult
            {
                Type = "success",
                Message = "Thêm sản phẩm thành công!"
            };
        }

        public async Task<ServiceResult> UpdateAsync(ProductFormDto model)
        {
            // Tìm sản phẩm cần update
            var product = await _dbContext.Products
                .Include(p => p.Inventory)
                .FirstOrDefaultAsync(p => p.ProductId == model.ProductId);

            if (product == null)
            {
                return new ServiceResult
                {
                    Type = "error",
                    Message = "Không tìm thấy sản phẩm!"
                };
            }

            // Kiểm tra trùng barcode (trừ chính nó)
            var duplicateBarcode = await _dbContext.Products
                .AsNoTracking()
                .AnyAsync(p => p.Barcode == model.Barcode && p.ProductId != model.ProductId);

            if (duplicateBarcode)
            {
                return new ServiceResult
                {
                    Type = "error",
                    Message = "Mã vạch này đã được sử dụng bởi sản phẩm khác!"
                };
            }

            // Xử lý ảnh mới (nếu có)
            if (model.ImageFile != null)
            {
                // Xóa ảnh cũ
                await DeleteImageAsync(product.ProductImage);

                // Upload ảnh mới
                var uploadedFileName = await UploadImageAsync(model.ImageFile);

                if (uploadedFileName == null)
                {
                    return new ServiceResult
                    {
                        Type = "error",
                        Message = "Lỗi khi tải lên hình ảnh mới!"
                    };
                }

                product.ProductImage = uploadedFileName;
            }
            // Nếu không có ImageFile mới, giữ nguyên ảnh cũ

            // Cập nhật thông tin sản phẩm
            product.ProductName = model.ProductName;
            product.CategoryId = model.CategoryId;
            product.SupplierId = model.SupplierId;
            product.Price = model.Price;
            product.Unit = model.Unit;
            product.Barcode = model.Barcode;

            // Cập nhật tồn kho
            if (product.Inventory != null)
            {
                product.Inventory.Quantity = model.Inventory?.Quantity ?? 0;
                product.Inventory.UpdatedAt = DateTime.Now;
            }

            // Lưu vào DB
            await _dbContext.SaveChangesAsync();

            return new ServiceResult
            {
                Type = "success",
                Message = "Cập nhật sản phẩm thành công!"
            };
        }

        public async Task<ServiceResult> DeleteAsync(ProductFormDto model)
        {
            var product = await _dbContext.Products
                .Include(p => p.Inventory)
                .FirstOrDefaultAsync(p => p.ProductId == model.ProductId);

            if (product == null)
            {
                return new ServiceResult
                {
                    Type = "error",
                    Message = "Không tìm thấy sản phẩm!"
                };
            }

            bool existsInOrder = await _dbContext.OrderItems
                .AnyAsync(oi => oi.ProductId == product.ProductId);

            if (existsInOrder)
            {
                return new ServiceResult
                {
                    Type = "error",
                    Message = "Không thể xóa! Sản phẩm đã được bán và tồn tại trong đơn hàng."
                };
            }

            await DeleteImageAsync(product.ProductImage);

            _dbContext.Products.Remove(product);
            await _dbContext.SaveChangesAsync();

            return new ServiceResult
            {
                Type = "success",
                Message = "Xóa sản phẩm thành công!"
            };
        }


        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _dbContext.Categories
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Supplier>> GetAllSuppliersAsync()
        {
            return await _dbContext.Suppliers
                .AsNoTracking()
                .ToListAsync();
        }

        public Task<PageResult<Product>> GetAllProductAsync(int page)
        {
            var query = _dbContext.Products
                .Include(p => p.Inventory)
                .OrderByDescending(x => x.CreatedAt);
            return GetPagedAsync<Product>(query, page);
        }

        public async Task<ProductFormDto?> GetProductDetailAsync(int productId)
        {
            var product = await _dbContext.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Include(p => p.Inventory)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null)
                return null;

            return new ProductFormDto
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                ProductImage = product.ProductImage,
                Price = product.Price,
                Unit = product.Unit,
                Barcode = product.Barcode,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.CategoryName ?? string.Empty,
                SupplierId = product.SupplierId,
                SupplierName = product.Supplier?.Name ?? string.Empty,
                Inventory = product.Inventory
            };
        }

        private async Task<string?> UploadImageAsync(IBrowserFile file)
        {
            try
            {
                var folder = Path.Combine("wwwroot", "uploads");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                var fileExt = Path.GetExtension(file.Name);
                var newFileName = $"{Guid.NewGuid()}{fileExt}";
                var filePath = Path.Combine(folder, newFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.OpenReadStream(10 * 1024 * 1024).CopyToAsync(stream);
                }

                return newFileName;
            }
            catch
            {
                return null;
            }
        }
        public Task<PageResult<Product>> FilterAsync(string keyword, int categoryId, decimal? priceFrom, decimal? priceTo, int page)
        {
            var query = _dbContext.Products
                .Include(p => p.Inventory)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(p => p.ProductName.Contains(keyword) || p.Barcode.Contains(keyword));
            }

            if (categoryId != -1)
            {
                query = query.Where(p => p.CategoryId == categoryId);
            }

            if (priceFrom.HasValue)
            {
                query = query.Where(p => p.Price >= priceFrom.Value);
            }

            if (priceTo.HasValue)
            {
                query = query.Where(p => p.Price <= priceTo.Value);
            }

            query = query.OrderByDescending(x => x.CreatedAt);

            return GetPagedAsync<Product>(query, page);
        }

        private async Task DeleteImageAsync(string? fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return;

            try
            {
                var filePath = Path.Combine("wwwroot", "uploads", fileName);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch
            {
                // Log error 
            }

            await Task.CompletedTask;
        }

        Task<string?> IProductManagerService.UploadImageAsync(IBrowserFile file)
        {
            return UploadImageAsync(file);
        }
    }
}