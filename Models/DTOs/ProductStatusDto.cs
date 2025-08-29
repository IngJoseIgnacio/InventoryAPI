namespace InventoryAPI.Models.DTOs
{
    public class ProductStatusDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int TotalQuantity { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? NextExpiration { get; set; }
    }
}