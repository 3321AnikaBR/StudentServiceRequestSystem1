using System.ComponentModel.DataAnnotations;

namespace StudentServiceRequestSystem.Models
{
    public class ServiceRequest
    {
        [Key]
        public int RequestId { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        [Display(Name = "Request Type")]
        public string RequestType { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public string Status { get; set; } = "Pending";

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "Updated Date")]
        public DateTime? UpdatedDate { get; set; }
    }
}