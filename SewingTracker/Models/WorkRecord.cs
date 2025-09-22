namespace SewingTracker.Models
{
    public class WorkRecord
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        public int ClothId { get; set; }

        public DateTime CompletedDate { get; set; }

        public string Notes { get; set; }

        // Navigation properties
        public Employee Employee { get; set; }
        public Cloth Cloth { get; set; }
    }
}
