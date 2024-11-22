using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClaimPro.Models
{
    public class Invoice
    {
        [Key]
        public int InvoiceId { get; set; }

        public int ClaimId { get; set; }
        [ForeignKey("ClaimId")]
        public Claim Claim { get; set; }

        public string GeneratedBy { get; set; } = string.Empty; // HR User
        public DateTime GeneratedDate { get; set; } = DateTime.Now;

        public string InvoiceNumber { get; set; } = Guid.NewGuid().ToString(); // Unique Invoice Number
    }
}