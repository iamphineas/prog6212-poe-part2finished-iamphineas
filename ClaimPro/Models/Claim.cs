using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ClaimPro.Models
{
    public class Claim
    {
        [Key]
        public int ClaimId { get; set; }

        public string LecturerId { get; set; } = string.Empty;

        public string User { get; set; } = string.Empty;

        // Set decimal precision to 2 decimal places
        [Column(TypeName = "decimal(18, 2)")]
        public decimal HoursWorked { get; set; } = 0;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal HourlyRate { get; set; } = 0;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; } = 0;

        public ClaimStatus Status { get; set; } = ClaimStatus.Pending;

        public DateTime? SubmittedDate { get; set; } = DateTime.Now;

        public string? ImageUrl { get; set; } = string.Empty;

        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        public string DocumentType { get; set; } = string.Empty;

        public string? ApprovalBy { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public string ApprovalStatus { get; set; } = string.Empty;

        public string Notes { get; set; } = string.Empty;
        public string Comments { get; set; } = string.Empty;

        public string? OriginalFileName { get; set; }
    }
}