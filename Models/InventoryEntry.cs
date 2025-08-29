namespace InventoryAPI.Models
{
    public class InventoryEntry
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public int Quantity { get; set; }
        public DateTime ExpirationDate { get; set; }
        public DateTime EntryDate { get; set; } = DateTime.Now;
        public bool IsOutput { get; set; } = false;
    }
}