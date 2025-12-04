using Microsoft.AspNetCore.Components.Forms;
using StoreBlazor.Models;

namespace StoreBlazor.DTO.Admin.Product
{
    public class ProductFormDto
    {
        public int ProductId { get; set; }

        // Thông tin cơ bản
        public string ProductName { get; set; } = string.Empty;
        public string ProductImage { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;

        // Danh mục
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        // Nhà cung cấp
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;

        public Inventory Inventory { get; set; } = new Inventory();
        public IBrowserFile? ImageFile { get; set; }
    }
}
