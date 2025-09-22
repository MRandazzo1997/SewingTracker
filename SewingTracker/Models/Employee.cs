using System.ComponentModel.DataAnnotations;

namespace SewingTracker.Models
{
    public class Employee
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string EmployeeBarcode { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
