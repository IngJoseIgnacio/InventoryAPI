namespace InventoryAPI.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<InventoryEntry> InventoryEntries { get; set; } = new List<InventoryEntry>();
    }
}