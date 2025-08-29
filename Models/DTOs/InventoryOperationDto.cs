namespace InventoryAPI.Models.DTOs
{
    public class InventoryOperationDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}