using System.ComponentModel.DataAnnotations;

namespace SewingTracker.Models
{
    public class Cloth
    {
        public int Id { get; set; }

        [Required]
        public string ClothId { get; set; }

        public string Description { get; set; }

        public decimal Price { get; set; }

        public DateTime ReceiptDate { get; set; }

        [Required]
        public string ReceiptBarcode { get; set; }

        public string CustomerInfo { get; set; }
    }
}
